//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using CorlaneCabinetOrderFormV3.Converters;
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
//    private readonly IPriceBreakdownService? _priceBreakdownService;

//    public POJobMaterialListViewModel()
//    {
//        // design-time support
//    }

//    public POJobMaterialListViewModel(
//        ICabinetService cabinetService,
//        IMaterialPricesService materialPrices,
//        IPriceBreakdownService priceBreakdownService)
//    {
//        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
//        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));
//        _priceBreakdownService = priceBreakdownService ?? throw new ArgumentNullException(nameof(priceBreakdownService));

//        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
//        {
//            cc.CollectionChanged += (_, __) => Refresh();
//        }

//        Refresh();
//    }

//    public ObservableCollection<MaterialBreakdownRow> SheetGoods { get; } = new();
//    public ObservableCollection<MaterialBreakdownRow> EdgeBanding { get; } = new();

//    public ObservableCollection<MaterialTotal> PriceBreakdown { get; } = new();

//    [ObservableProperty]
//    public partial double TotalSheetGoodsSqFt { get; set; }

//    [ObservableProperty]
//    public partial int TotalSheetGoodsSheets { get; set; }

//    [ObservableProperty]
//    public partial double TotalEdgeBandingFeet { get; set; }

//    [ObservableProperty]
//    public partial decimal TotalMaterialPrice { get; set; }

//    public string FormattedTotalMaterialPrice => TotalMaterialPrice.ToString("C2");

//    partial void OnTotalMaterialPriceChanged(decimal value)
//    {
//        OnPropertyChanged(nameof(FormattedTotalMaterialPrice));
//    }

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
//        PriceBreakdown.Clear();

//        TotalSheetGoodsSqFt = 0;
//        TotalSheetGoodsSheets = 0;
//        TotalEdgeBandingFeet = 0;
//        TotalMaterialPrice = 0m;

//        if (_cabinetService == null || _materialPrices == null || _priceBreakdownService == null)
//        {
//            return;
//        }

//        static string CollapseFaceKey(string species)
//        {
//            if (string.IsNullOrWhiteSpace(species))
//            {
//                return "None";
//            }

//            var s = species.Trim();

//            if (s.EndsWith(" UP", StringComparison.OrdinalIgnoreCase))
//            {
//                return s[..^3].TrimEnd();
//            }

//            if (s.EndsWith(" DOWN", StringComparison.OrdinalIgnoreCase))
//            {
//                return s[..^5].TrimEnd();
//            }

//            return s;
//        }

//        // Aggregate (keeps UP/DOWN split at source)
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

//            if (cab is UpperCabinetModel)
//            {
//                string upperCabExtraEbSpecies = GetMatchingEdgebandingSpecies(cab.Species); // sets species extra banding on bottom of upper cabinet end panels

//                var depthIn = ConvertDimension.FractionToDouble(cab.Depth);
//                var extraFeet = ((2.0 * depthIn) / 12.0) * qty;

//                if (extraFeet > 0 &&
//                    !string.IsNullOrWhiteSpace(upperCabExtraEbSpecies) &&
//                    !string.Equals(upperCabExtraEbSpecies, "None", StringComparison.OrdinalIgnoreCase))
//                {
//                    if (edgebanding.TryGetValue(upperCabExtraEbSpecies, out var existing))
//                    {
//                        edgebanding[upperCabExtraEbSpecies] = existing + extraFeet;
//                    }
//                    else
//                    {
//                        edgebanding[upperCabExtraEbSpecies] = extraFeet;
//                    }
//                }
//            }
//        }


//        // Price breakdown: KEEP AS-IS for now (will price UP/DOWN separately unless you also collapse there).
//        var breakdown = _priceBreakdownService.Build(materials, edgebanding);
//        foreach (var line in breakdown.Lines)
//        {
//            PriceBreakdown.Add(line);
//        }
//        TotalMaterialPrice = breakdown.Total;

//        // Show split rows (UP and DOWN as separate lines)
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
//        }

//        // Compute combined total sheets (UP/DOWN collapsed)
//        var collapsedMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
//        foreach (var kv in materials)
//        {
//            var collapsed = CollapseFaceKey(kv.Key);
//            if (collapsedMaterials.TryGetValue(collapsed, out var existing))
//            {
//                collapsedMaterials[collapsed] = existing + kv.Value;
//            }
//            else
//            {
//                collapsedMaterials[collapsed] = kv.Value;
//            }
//        }

//        foreach (var kv in collapsedMaterials)
//        {
//            var species = kv.Key;
//            var qtySqFt = kv.Value;

//            var sheetAreaSqFt = GetSheetAreaSqFt(species);
//            var yield = GetYield(species);

//            var sheets = (sheetAreaSqFt <= 0 || yield <= 0)
//                ? 0
//                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

//            TotalSheetGoodsSheets += sheets;
//        }

//        // Edgebanding unchanged
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

//            if (kv.Key == "None") // Remove "None" from total
//            {
//                TotalEdgeBandingFeet -= feet;
//            }
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



//    private static string GetMatchingEdgebandingSpecies(string? species) // Helper to map common species/material names to edgebanding names
//    {
//        return species switch
//        {
//            null or "" => "None",

//            // Match common species/material names -> edgebanding names
//            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
//            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
//            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
//            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
//            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
//            string s when s.Contains("Maply Ply", StringComparison.OrdinalIgnoreCase) => "Wood Maple", // your example
//            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
//            string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Custom",
//            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "PVC Hardrock Maple",
//            string s when s.Contains("PFP 1/4", StringComparison.OrdinalIgnoreCase) => "None",
//            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
//            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
//            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood White Oak",

//            _ => "None"
//        };
//    }

//}




























using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
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

        static string CollapseFaceKey(string species)
        {
            if (string.IsNullOrWhiteSpace(species))
            {
                return "None";
            }

            var s = species.Trim();

            if (s.EndsWith(" UP", StringComparison.OrdinalIgnoreCase))
            {
                return s[..^3].TrimEnd();
            }

            if (s.EndsWith(" DOWN", StringComparison.OrdinalIgnoreCase))
            {
                return s[..^5].TrimEnd();
            }

            return s;
        }

        static bool IsCustomToken(string s) =>
            string.Equals(s?.Trim(), "Custom", StringComparison.OrdinalIgnoreCase);

        static string NormalizeBlankToNone(string? s) =>
            string.IsNullOrWhiteSpace(s) ? "None" : s.Trim();

        static string ResolveCustomSpeciesKey(string keyFromTotals, CabinetModel cab)
        {
            var key = NormalizeBlankToNone(keyFromTotals);

            if (!IsCustomToken(key))
            {
                return key;
            }

            // If the totals dictionary used "Custom", replace it with the cab's actual entered custom name.
            // Fall back to "Custom" only if they somehow didn't enter a name.
            var custom = NormalizeBlankToNone(cab.CustomSpecies);
            return string.Equals(custom, "None", StringComparison.OrdinalIgnoreCase) ? "Custom" : custom;
        }

        static string ResolveCustomEdgebandingKey(string keyFromTotals, CabinetModel cab)
        {
            var key = NormalizeBlankToNone(keyFromTotals);

            if (!IsCustomToken(key))
            {
                return key;
            }

            var custom = NormalizeBlankToNone(cab.CustomEBSpecies);
            return string.Equals(custom, "None", StringComparison.OrdinalIgnoreCase) ? "Custom" : custom;
        }

        // Aggregate (keeps UP/DOWN split at source)
        var materials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var edgebanding = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var cab in _cabinetService.Cabinets)
        {
            var qty = Math.Max(1, cab.Qty);

            foreach (var kv in cab.MaterialAreaBySpecies)
            {
                var species = ResolveCustomSpeciesKey(kv.Key, cab);
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
                var species = ResolveCustomEdgebandingKey(kv.Key, cab);
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

            if (cab is UpperCabinetModel)
            {
                string upperCabExtraEbSpecies = GetMatchingEdgebandingSpecies(cab.Species); // sets species extra banding on bottom of upper cabinet end panels

                var depthIn = ConvertDimension.FractionToDouble(cab.Depth);
                var extraFeet = ((2.0 * depthIn) / 12.0) * qty;

                if (extraFeet > 0 &&
                    !string.IsNullOrWhiteSpace(upperCabExtraEbSpecies) &&
                    !string.Equals(upperCabExtraEbSpecies, "None", StringComparison.OrdinalIgnoreCase))
                {
                    if (edgebanding.TryGetValue(upperCabExtraEbSpecies, out var existing))
                    {
                        edgebanding[upperCabExtraEbSpecies] = existing + extraFeet;
                    }
                    else
                    {
                        edgebanding[upperCabExtraEbSpecies] = extraFeet;
                    }
                }
            }
        }

        // Price breakdown: KEEP AS-IS for now (will price UP/DOWN separately unless you also collapse there).
        var breakdown = _priceBreakdownService.Build(materials, edgebanding);
        foreach (var line in breakdown.Lines)
        {
            PriceBreakdown.Add(line);
        }
        TotalMaterialPrice = breakdown.Total;

        // Show split rows (UP and DOWN as separate lines)
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
        }

        // Compute combined total sheets (UP/DOWN collapsed)
        var collapsedMaterials = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in materials)
        {
            var collapsed = CollapseFaceKey(kv.Key);
            if (collapsedMaterials.TryGetValue(collapsed, out var existing))
            {
                collapsedMaterials[collapsed] = existing + kv.Value;
            }
            else
            {
                collapsedMaterials[collapsed] = kv.Value;
            }
        }

        foreach (var kv in collapsedMaterials)
        {
            var species = kv.Key;
            var qtySqFt = kv.Value;

            var sheetAreaSqFt = GetSheetAreaSqFt(species);
            var yield = GetYield(species);

            var sheets = (sheetAreaSqFt <= 0 || yield <= 0)
                ? 0
                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

            TotalSheetGoodsSheets += sheets;
        }

        // Edgebanding unchanged
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

            if (kv.Key == "None") // Remove "None" from total
            {
                TotalEdgeBandingFeet -= feet;
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

    private static string GetMatchingEdgebandingSpecies(string? species) // Helper to map common species/material names to edgebanding names
    {
        return species switch
        {
            null or "" => "None",

            // Match common species/material names -> edgebanding names
            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Maply Ply", StringComparison.OrdinalIgnoreCase) => "Wood Maple", // your example
            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Custom",
            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "PVC Hardrock Maple",
            string s when s.Contains("PFP 1/4", StringComparison.OrdinalIgnoreCase) => "None",
            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood White Oak",

            _ => "None"
        };
    }
}