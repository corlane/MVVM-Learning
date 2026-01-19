//using CorlaneCabinetOrderFormV3.Models;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;

//namespace CorlaneCabinetOrderFormV3.Services;

//public sealed class PriceBreakdownService : IPriceBreakdownService
//{
//    private readonly IMaterialPricesService _materialPrices;

//    public PriceBreakdownService(IMaterialPricesService materialPrices)
//    {
//        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));
//    }

//    public PriceBreakdownResult Build(
//        Dictionary<string, double> materialsSqFtBySpecies,
//        Dictionary<string, double> edgebandingFeetBySpecies)
//        {
//        var lines = new List<MaterialTotal>();
//        decimal total = 0m;
//        int totalSheetsTally = 0;
//        int totalCNCSqFt = 0;

//            foreach (var kv in materialsSqFtBySpecies.OrderBy(k => k.Key))
//            {
//                var species = kv.Key;
//                var qtySqFt = kv.Value;

//                var sheetAreaSqFt = GetSheetAreaSqFt(species);
//                var yield = GetYield(species);

//                int sheetQty = (sheetAreaSqFt <= 0 || yield <= 0)
//                    ? 0
//                    : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

//                totalSheetsTally += sheetQty;

//                var unitPricePerSheet = GetSheetPricePerSqFt(species) * (decimal)sheetAreaSqFt;

//                var line = new MaterialTotal
//                {
//                    Species = species,
//                    Quantity = sheetQty,
//                    Unit = "Sheets",
//                    UnitPrice = unitPricePerSheet,
//                    SqFt = qtySqFt
//                };

//                lines.Add(line);
//                total += line.LineTotal;

//            }

//            foreach (var kv in edgebandingFeetBySpecies.OrderBy(k => k.Key))
//            {
//                var species = kv.Key;
//                var qtyFt = kv.Value;

//                var unitPrice = GetEdgeBandPricePerFt(species);

//                var line = new MaterialTotal
//                {
//                    Species = species,
//                    Quantity = qtyFt,
//                    Unit = "ft",
//                    UnitPrice = unitPrice
//                };

//                lines.Add(line);
//                total += line.LineTotal;
//            }

//            if (totalSheetsTally > 0)
//            {
//                var cnc = new MaterialTotal
//                {
//                    Species = "Sheets of CNC cutting",
//                    Quantity = totalSheetsTally,
//                    Unit = "Sheets",
//                    UnitPrice = _materialPrices.CncPricePerSheet
//                };

//                lines.Add(cnc);
//                total += cnc.LineTotal;
//            }

//        return new PriceBreakdownResult(Math.Round(total, 2), lines);
//    }

//    private decimal GetSheetPricePerSqFt(string? species)
//    {
//        if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
//        {
//            return 0m;
//        }

//        if (_materialPrices.TryGetSheetMaterial(species, out var row))
//        {
//            return row.PricePerSqFt;
//        }

//        return 0m;
//    }

//    private decimal GetEdgeBandPricePerFt(string? species)
//    {
//        if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
//        {
//            return 0m;
//        }

//        if (_materialPrices.TryGetEdgeBand(species, out var row))
//        {
//            return row.PricePerFt;
//        }

//        return 0m;
//    }

//    private double GetYield(string species)
//    {
//        if (_materialPrices.TryGetYield(species, out var y))
//        {
//            return y;
//        }

//        return _materialPrices.DefaultSheetYield;
//    }

//    private double GetSheetAreaSqFt(string species)
//    {
//        if (_materialPrices.TryGetSheetMaterial(species, out var row))
//        {
//            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
//            if (areaSqIn > 0)
//            {
//                return areaSqIn / 144.0;
//            }
//        }

//        return 32.0;
//    }
//}















using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CorlaneCabinetOrderFormV3.Services;

public sealed class PriceBreakdownService : IPriceBreakdownService
{
    private const double BaselineCncSheetAreaSqFt = 32.0; // 48x96

    private readonly IMaterialPricesService _materialPrices;

    public PriceBreakdownService(IMaterialPricesService materialPrices)
    {
        _materialPrices = materialPrices ?? throw new ArgumentNullException(nameof(materialPrices));
    }

    public PriceBreakdownResult Build(
        Dictionary<string, double> materialsSqFtBySpecies,
        Dictionary<string, double> edgebandingFeetBySpecies)
    {
        var lines = new List<MaterialTotal>();
        decimal total = 0m;

        double totalCncBillableSqFt = 0;

        foreach (var kv in materialsSqFtBySpecies.OrderBy(k => k.Key))
        {
            var species = kv.Key;
            var qtySqFt = kv.Value;

            var sheetAreaSqFt = GetSheetAreaSqFt(species);
            var yield = GetYield(species);

            int sheetQty = (sheetAreaSqFt <= 0 || yield <= 0)
                ? 0
                : (int)Math.Ceiling((qtySqFt / yield) / sheetAreaSqFt);

            // CNC is billed based on actual sheet(s) that must be cut (rounded up), not net sqft.
            totalCncBillableSqFt += sheetQty * sheetAreaSqFt;

            var unitPricePerSheet = GetSheetPricePerSqFt(species) * (decimal)sheetAreaSqFt;

            var line = new MaterialTotal
            {
                Species = species,
                Quantity = sheetQty,
                Unit = "Sheets",
                UnitPrice = unitPricePerSheet,
                SqFt = qtySqFt
            };

            lines.Add(line);
            total += line.LineTotal;
        }

        foreach (var kv in edgebandingFeetBySpecies.OrderBy(k => k.Key))
        {
            var species = kv.Key;
            var qtyFt = kv.Value;

            var unitPrice = GetEdgeBandPricePerFt(species);

            var line = new MaterialTotal
            {
                Species = species,
                Quantity = qtyFt,
                Unit = "ft",
                UnitPrice = unitPrice
            };

            lines.Add(line);
            total += line.LineTotal;
        }

        if (totalCncBillableSqFt > 0 && BaselineCncSheetAreaSqFt > 0)
        {
            // Keep storing CNC as "$ per (48x96) sheet", but bill by sqft with rounding-to-sheets applied above.
            var cncPricePerSqFt = _materialPrices.CncPricePerSheet / (decimal)BaselineCncSheetAreaSqFt;

            var cnc = new MaterialTotal
            {
                Species = "CNC cutting",
                Quantity = totalCncBillableSqFt,
                Unit = "Sq Ft",
                UnitPrice = cncPricePerSqFt
            };

            lines.Add(cnc);
            total += cnc.LineTotal;
        }

        return new PriceBreakdownResult(Math.Round(total, 2), lines);
    }

    private decimal GetSheetPricePerSqFt(string? species)
    {
        if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        if (_materialPrices.TryGetSheetMaterial(species, out var row))
        {
            return row.PricePerSqFt;
        }

        return 0m;
    }

    private decimal GetEdgeBandPricePerFt(string? species)
    {
        if (string.IsNullOrWhiteSpace(species) || string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
        {
            return 0m;
        }

        if (_materialPrices.TryGetEdgeBand(species, out var row))
        {
            return row.PricePerFt;
        }

        return 0m;
    }

    private double GetYield(string species)
    {
        if (_materialPrices.TryGetYield(species, out var y))
        {
            return y;
        }

        return _materialPrices.DefaultSheetYield;
    }

    private double GetSheetAreaSqFt(string species)
    {
        if (_materialPrices.TryGetSheetMaterial(species, out var row))
        {
            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
            if (areaSqIn > 0)
            {
                return areaSqIn / 144.0;
            }
        }

        return 32.0;
    }
}