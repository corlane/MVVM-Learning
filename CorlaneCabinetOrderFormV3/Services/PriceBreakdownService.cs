using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

// PriceBreakdownService.cs
// Concrete implementation of IPriceBreakdownService responsible for calculating
// the full material cost breakdown for a cabinet order. Given two dictionaries —
// one mapping species names to total square footage of sheet goods used, and one
// mapping species names to total linear feet of edgebanding used — it looks up
// current prices via IMaterialPricesService to produce a PriceBreakdownResult
// containing per-line MaterialTotal entries and a rounded grand total.
//
// Sheet quantities are rounded up to whole sheets (ceiling), and CNC cutting
// charges are billed based on the total billable sheet area (sheets × sheet size)
// rather than net square footage, using a baseline of the default 48×96 sheet
// area defined in MaterialDefaults. Edgebanding is billed per linear foot.
// Species values of null, empty, or "None" are treated as free (price = $0).

public sealed class PriceBreakdownService : IPriceBreakdownService
{
    private const double BaselineCncSheetAreaSqFt = MaterialDefaults.DefaultSheetAreaSqFt;

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

            var sheetAreaSqFt = _materialPrices.GetSheetAreaForSpecies(species);
            var yield = _materialPrices.GetYieldForSpecies(species);

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
}