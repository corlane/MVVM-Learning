using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POJobMaterialListViewModel : ObservableObject
{
    private readonly ICabinetService? _cabinetService;
    private readonly IMaterialPricesService? _materialPrices;
    private readonly IPriceBreakdownService? _priceBreakdownService;

    public POJobMaterialListViewModel()
    {
        // design-time support
    }

    public POJobMaterialListViewModel(
        ICabinetService cabinetService,
        IMaterialPricesService materialPrices,
        IPriceBreakdownService priceBreakdownService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));
        _priceBreakdownService = priceBreakdownService ?? throw new ArgumentNullException(nameof(priceBreakdownService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    public ObservableCollection<MaterialBreakdownRow> SheetGoods { get; } = new();
    public ObservableCollection<MaterialBreakdownRow> EdgeBanding { get; } = new();

    public ObservableCollection<MaterialTotal> PriceBreakdown { get; } = new();

    [ObservableProperty]
    public partial double TotalSheetGoodsSqFt { get; set; }

    [ObservableProperty]
    public partial int TotalSheetGoodsSheets { get; set; }

    [ObservableProperty]
    public partial double TotalEdgeBandingFeet { get; set; }

    [ObservableProperty]
    public partial decimal TotalMaterialPrice { get; set; }

    public string FormattedTotalMaterialPrice => TotalMaterialPrice.ToString("C2");

    partial void OnTotalMaterialPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(FormattedTotalMaterialPrice));
    }

    [RelayCommand]
    private void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        SheetGoods.Clear();
        EdgeBanding.Clear();
        PriceBreakdown.Clear();

        TotalSheetGoodsSqFt = 0;
        TotalSheetGoodsSheets = 0;
        TotalEdgeBandingFeet = 0;
        TotalMaterialPrice = 0m;

        if (_cabinetService == null || _materialPrices == null || _priceBreakdownService == null)
        {
            return;
        }

        // Build lightweight snapshots so the calculator doesn't need CabinetModel
        var snapshots = _cabinetService.Cabinets.Select(cab => new CabinetMaterialSnapshot(
            Math.Max(1, cab.Qty),
            cab.CustomSpecies,
            cab.CustomEBSpecies,
            new Dictionary<string, double>(cab.MaterialAreaBySpecies, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, double>(cab.EdgeBandingLengthBySpecies, StringComparer.OrdinalIgnoreCase)
        )).ToList();

        // Delegate aggregation to the calculator
        var materials = MaterialYieldCalculator.AggregateMaterialAreas(snapshots);
        var edgebanding = MaterialYieldCalculator.AggregateEdgeBanding(snapshots);

        // Upper cabinet extra edgebanding (bottom of end panels) — stays here because it needs CabinetModel type check
        foreach (var cab in _cabinetService.Cabinets)
        {
            if (cab is UpperCabinetModel)
            {
                var qty = Math.Max(1, cab.Qty);
                string upperCabExtraEbSpecies = CabinetBuildHelpers.GetMatchingEdgebandingSpecies(cab.Species);

                var depthIn = ConvertDimension.FractionToDouble(cab.Depth);
                var extraFeet = ((2.0 * depthIn) / 12.0) * qty;

                if (extraFeet > 0 &&
                    !string.IsNullOrWhiteSpace(upperCabExtraEbSpecies) &&
                    !string.Equals(upperCabExtraEbSpecies, "None", StringComparison.OrdinalIgnoreCase))
                {
                    if (edgebanding.TryGetValue(upperCabExtraEbSpecies, out var existing))
                        edgebanding[upperCabExtraEbSpecies] = existing + extraFeet;
                    else
                        edgebanding[upperCabExtraEbSpecies] = extraFeet;
                }
            }
        }

        // Price breakdown
        var breakdown = _priceBreakdownService.Build(materials, edgebanding);
        foreach (var line in breakdown.Lines)
        {
            PriceBreakdown.Add(line);
        }
        TotalMaterialPrice = breakdown.Total;

        // Sheet goods rows (UP and DOWN as separate lines)
        foreach (var kv in materials.OrderBy(k => k.Key))
        {
            var species = kv.Key;
            var qtySqFt = kv.Value;

            var sheetAreaSqFt = GetSheetAreaSqFt(species);
            var yield = GetYield(species);
            var sheets = MaterialYieldCalculator.ComputeSheetCount(qtySqFt, sheetAreaSqFt, yield);

            SheetGoods.Add(new MaterialBreakdownRow
            {
                Species = species,
                SqFt = qtySqFt,
                Sheets = sheets
            });

            TotalSheetGoodsSqFt += qtySqFt;
        }

        // Combined total sheets (UP/DOWN collapsed)
        var collapsedMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in materials)
        {
            var collapsed = MaterialYieldCalculator.CollapseFaceKey(kv.Key);
            if (collapsedMaterials.TryGetValue(collapsed, out var existing))
                collapsedMaterials[collapsed] = existing + kv.Value;
            else
                collapsedMaterials[collapsed] = kv.Value;
        }

        foreach (var kv in collapsedMaterials)
        {
            var sheetAreaSqFt = GetSheetAreaSqFt(kv.Key);
            var yield = GetYield(kv.Key);
            TotalSheetGoodsSheets += MaterialYieldCalculator.ComputeSheetCount(kv.Value, sheetAreaSqFt, yield);
        }

        // Edgebanding rows
        foreach (var kv in edgebanding.OrderBy(k => k.Key))
        {
            EdgeBanding.Add(new MaterialBreakdownRow
            {
                Species = kv.Key,
                LinearFeet = kv.Value
            });

            TotalEdgeBandingFeet += kv.Value;

            if (kv.Key == "None")
            {
                TotalEdgeBandingFeet -= kv.Value;
            }
        }

        TotalSheetGoodsSqFt = Math.Round(TotalSheetGoodsSqFt, 2);
        TotalEdgeBandingFeet = Math.Round(TotalEdgeBandingFeet, 2);
    }



    private double GetYield(string species)
    {
        if (_materialPrices != null && _materialPrices.TryGetYield(species, out var y))
        {
            return y;
        }

        return _materialPrices?.DefaultSheetYield ?? 0.82;
    }

    private double GetSheetAreaSqFt(string species)
    {
        if (_materialPrices != null && _materialPrices.TryGetSheetMaterial(species, out var row))
        {
            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
            if (areaSqIn > 0)
            {
                return areaSqIn / 144.0;
            }
        }

        // fallback 4x8
        return 32.0;
    }
}