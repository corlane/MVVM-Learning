//using CommunityToolkit.Mvvm.ComponentModel;
//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Services;
//using System;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Windows;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public partial class POJobMaterialListViewModel : ObservableObject
//{
//    private readonly ICabinetService? _cabinetService;
//    private readonly IMaterialPricesService? _materialPrices;

//    public POJobMaterialListViewModel()
//    {
//        // design-time support
//    }

//    public POJobMaterialListViewModel(ICabinetService cabinetService, IMaterialPricesService materialPrices)
//    {
//        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
//        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));

//        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
//        {
//            cc.CollectionChanged += (_, __) => Refresh();
//        }

//        Refresh();
//    }

//    public ObservableCollection<MaterialBreakdownRow> SheetGoods { get; } = new();
//    public ObservableCollection<MaterialBreakdownRow> EdgeBanding { get; } = new();

//    [ObservableProperty]
//    public partial double TotalSheetGoodsSqFt { get; set; }

//    [ObservableProperty]
//    public partial int TotalSheetGoodsSheets { get; set; }

//    [ObservableProperty]
//    public partial double TotalEdgeBandingFeet { get; set; }

//    public void Refresh()
//    {
//        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
//        {
//            Application.Current.Dispatcher.Invoke(Refresh);
//            return;
//        }

//        SheetGoods.Clear();
//        EdgeBanding.Clear();

//        TotalSheetGoodsSqFt = 0;
//        TotalSheetGoodsSheets = 0;
//        TotalEdgeBandingFeet = 0;

//        if (_cabinetService == null || _materialPrices == null)
//        {
//            return;
//        }

//        // Aggregate
//        var materials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
//        var edgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

//        foreach (var cab in _cabinetService.Cabinets)
//        {
//            var qty = Math.Max(1, cab.Qty);

//            foreach (var kv in cab.MaterialAreaBySpecies)
//            {
//                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
//                var area = kv.Value * qty;

//                if (materials.TryGetValue(species, out var existing))
//                {
//                    materials[species] = existing + area;
//                }
//                else
//                {
//                    materials[species] = area;
//                }
//            }

//            foreach (var kv in cab.EdgeBandingLengthBySpecies)
//            {
//                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
//                var feet = kv.Value * qty;

//                if (edgebanding.TryGetValue(species, out var existing))
//                {
//                    edgebanding[species] = existing + feet;
//                }
//                else
//                {
//                    edgebanding[species] = feet;
//                }
//            }
//        }

//        // Project: sheet goods
//        foreach (var kv in materials.OrderBy(k => k.Key))
//        {
//            var species = kv.Key;
//            var qtySqFt = kv.Value;

//            var sheetAreaSqFt = GetSheetAreaSqFt(species);
//            var yield = GetYield(species);

//            var sheets = (sheetAreaSqFt <= 0 || yield <= 0)
//                ? 0
//                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

//            SheetGoods.Add(new MaterialBreakdownRow
//            {
//                Species = species,
//                SqFt = qtySqFt,
//                Sheets = sheets
//            });

//            TotalSheetGoodsSqFt += qtySqFt;
//            TotalSheetGoodsSheets += sheets;
//        }

//        // Project: edgebanding
//        foreach (var kv in edgebanding.OrderBy(k => k.Key))
//        {
//            var species = kv.Key;
//            var feet = kv.Value;

//            EdgeBanding.Add(new MaterialBreakdownRow
//            {
//                Species = species,
//                LinearFeet = feet
//            });

//            TotalEdgeBandingFeet += feet;
//        }

//        TotalSheetGoodsSqFt = Math.Round(TotalSheetGoodsSqFt, 2);
//        TotalEdgeBandingFeet = Math.Round(TotalEdgeBandingFeet, 2);
//    }

//    private double GetYield(string species)
//    {
//        if (_materialPrices != null && _materialPrices.TryGetYield(species, out var y))
//        {
//            return y;
//        }

//        return _materialPrices?.DefaultSheetYield ?? 0.82;
//    }

//    private double GetSheetAreaSqFt(string species)
//    {
//        if (_materialPrices != null && _materialPrices.TryGetSheetMaterial(species, out var row))
//        {
//            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
//            if (areaSqIn > 0)
//            {
//                return areaSqIn / 144.0;
//            }
//        }

//        // fallback 4x8
//        return 32.0;
//    }
//}























//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Services;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Windows;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public partial class POJobMaterialListViewModel : ObservableObject
//{
//    private readonly ICabinetService? _cabinetService;
//    private readonly IMaterialPricesService? _materialPrices;

//    public POJobMaterialListViewModel()
//    {
//        // design-time support
//    }

//    public POJobMaterialListViewModel(ICabinetService cabinetService, IMaterialPricesService materialPrices)
//    {
//        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
//        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));

//        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
//        {
//            cc.CollectionChanged += (_, __) => Refresh();
//        }

//        Refresh();
//    }

//    public ObservableCollection<MaterialBreakdownRow> SheetGoods { get; } = new();
//    public ObservableCollection<MaterialBreakdownRow> EdgeBanding { get; } = new();

//    [ObservableProperty]
//    public partial double TotalSheetGoodsSqFt { get; set; }

//    [ObservableProperty]
//    public partial int TotalSheetGoodsSheets { get; set; }

//    [ObservableProperty]
//    public partial double TotalEdgeBandingFeet { get; set; }

//    [RelayCommand]
//    private void Refresh()
//    {
//        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
//        {
//            Application.Current.Dispatcher.Invoke(Refresh);
//            return;
//        }

//        SheetGoods.Clear();
//        EdgeBanding.Clear();

//        TotalSheetGoodsSqFt = 0;
//        TotalSheetGoodsSheets = 0;
//        TotalEdgeBandingFeet = 0;

//        if (_cabinetService == null || _materialPrices == null)
//        {
//            return;
//        }

//        // Aggregate
//        var materials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
//        var edgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

//        foreach (var cab in _cabinetService.Cabinets)
//        {
//            var qty = Math.Max(1, cab.Qty);

//            foreach (var kv in cab.MaterialAreaBySpecies)
//            {
//                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
//                var area = kv.Value * qty;

//                if (materials.TryGetValue(species, out var existing))
//                {
//                    materials[species] = existing + area;
//                }
//                else
//                {
//                    materials[species] = area;
//                }
//            }

//            foreach (var kv in cab.EdgeBandingLengthBySpecies)
//            {
//                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
//                var feet = kv.Value * qty;

//                if (edgebanding.TryGetValue(species, out var existing))
//                {
//                    edgebanding[species] = existing + feet;
//                }
//                else
//                {
//                    edgebanding[species] = feet;
//                }
//            }
//        }

//        // Project: sheet goods
//        foreach (var kv in materials.OrderBy(k => k.Key))
//        {
//            var species = kv.Key;
//            var qtySqFt = kv.Value;

//            var sheetAreaSqFt = GetSheetAreaSqFt(species);
//            var yield = GetYield(species);

//            var sheets = (sheetAreaSqFt <= 0 || yield <= 0)
//                ? 0
//                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

//            SheetGoods.Add(new MaterialBreakdownRow
//            {
//                Species = species,
//                SqFt = qtySqFt,
//                Sheets = sheets
//            });

//            TotalSheetGoodsSqFt += qtySqFt;
//            TotalSheetGoodsSheets += sheets;
//        }

//        // Project: edgebanding
//        foreach (var kv in edgebanding.OrderBy(k => k.Key))
//        {
//            var species = kv.Key;
//            var feet = kv.Value;

//            EdgeBanding.Add(new MaterialBreakdownRow
//            {
//                Species = species,
//                LinearFeet = feet
//            });

//            TotalEdgeBandingFeet += feet;
//        }

//        TotalSheetGoodsSqFt = Math.Round(TotalSheetGoodsSqFt, 2);
//        TotalEdgeBandingFeet = Math.Round(TotalEdgeBandingFeet, 2);
//    }

//    private double GetYield(string species)
//    {
//        if (_materialPrices != null && _materialPrices.TryGetYield(species, out var y))
//        {
//            return y;
//        }

//        return _materialPrices?.DefaultSheetYield ?? 0.82;
//    }

//    private double GetSheetAreaSqFt(string species)
//    {
//        if (_materialPrices != null && _materialPrices.TryGetSheetMaterial(species, out var row))
//        {
//            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
//            if (areaSqIn > 0)
//            {
//                return areaSqIn / 144.0;
//            }
//        }

//        // fallback 4x8
//        return 32.0;
//    }
//}





































using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        // Aggregate
        var materials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var edgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var cab in _cabinetService.Cabinets)
        {
            var qty = Math.Max(1, cab.Qty);

            foreach (var kv in cab.MaterialAreaBySpecies)
            {
                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
                var area = kv.Value * qty;

                if (materials.TryGetValue(species, out var existing))
                {
                    materials[species] = existing + area;
                }
                else
                {
                    materials[species] = area;
                }
            }

            foreach (var kv in cab.EdgeBandingLengthBySpecies)
            {
                var species = string.IsNullOrWhiteSpace(kv.Key) ? "None" : kv.Key;
                var feet = kv.Value * qty;

                if (edgebanding.TryGetValue(species, out var existing))
                {
                    edgebanding[species] = existing + feet;
                }
                else
                {
                    edgebanding[species] = feet;
                }
            }
        }

        // Price breakdown (shared logic)
        var breakdown = _priceBreakdownService.Build(materials, edgebanding);
        foreach (var line in breakdown.Lines)
        {
            PriceBreakdown.Add(line);
        }
        TotalMaterialPrice = breakdown.Total;

        // Project: sheet goods
        foreach (var kv in materials.OrderBy(k => k.Key))
        {
            var species = kv.Key;
            var qtySqFt = kv.Value;

            var sheetAreaSqFt = GetSheetAreaSqFt(species);
            var yield = GetYield(species);

            var sheets = (sheetAreaSqFt <= 0 || yield <= 0)
                ? 0
                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

            SheetGoods.Add(new MaterialBreakdownRow
            {
                Species = species,
                SqFt = qtySqFt,
                Sheets = sheets
            });

            TotalSheetGoodsSqFt += qtySqFt;
            TotalSheetGoodsSheets += sheets;
        }

        // Project: edgebanding
        foreach (var kv in edgebanding.OrderBy(k => k.Key))
        {
            var species = kv.Key;
            var feet = kv.Value;

            EdgeBanding.Add(new MaterialBreakdownRow
            {
                Species = species,
                LinearFeet = feet
            });

            TotalEdgeBandingFeet += feet;
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