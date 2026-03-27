using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests pricing with multiple species, unknown species, and mixed EB.
/// </summary>
public class MultiSpeciesPricingTests
{
    private sealed class FakeMaterialPrices : IMaterialPricesService
    {
        private readonly Dictionary<string, MaterialPriceRow> _sheets = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EdgeBandPriceRow> _eb = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, double> _yields = new(StringComparer.OrdinalIgnoreCase);

        public decimal CncPricePerSheet { get; set; } = 50m;
        public double DefaultSheetYield { get; set; } = 0.75;
        public bool HasPrices => true;

        public IReadOnlyList<MaterialPriceRow> SheetMaterials => _sheets.Values.ToList();
        public IReadOnlyList<EdgeBandPriceRow> EdgeBanding => _eb.Values.ToList();
        public IReadOnlyDictionary<string, double> YieldBySpecies => _yields;

        public Task RefreshFromServerAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void AddSheet(string species, decimal pricePerSqFt, double widthIn = 48, double lengthIn = 96)
        {
            _sheets[species] = new MaterialPriceRow
            {
                Species = species,
                PricePerSqFt = pricePerSqFt,
                SheetWidthIn = widthIn,
                SheetLengthIn = lengthIn
            };
        }

        public void AddEdgeBand(string species, decimal pricePerFt)
        {
            _eb[species] = new EdgeBandPriceRow { Species = species, PricePerFt = pricePerFt };
        }

        public void AddYield(string species, double yield) => _yields[species] = yield;

        public bool TryGetSheetMaterial(string species, out MaterialPriceRow row)
            => _sheets.TryGetValue(species, out row!);
        public bool TryGetEdgeBand(string species, out EdgeBandPriceRow row)
            => _eb.TryGetValue(species, out row!);
        public bool TryGetYield(string species, out double yield)
            => _yields.TryGetValue(species, out yield);
    }

    //############################################################################################################

    [Fact]
    public void TwoSpecies_IndependentSheetRounding()
    {
        var prices = new FakeMaterialPrices();
        prices.AddSheet("Maple", 3.50m);       // 48×96 = 32 sq ft
        prices.AddSheet("Cherry", 5.00m);      // 48×96 = 32 sq ft
        prices.AddEdgeBand("Wood Maple", 0.25m);
        prices.AddEdgeBand("Wood Cherry", 0.30m);
        prices.CncPricePerSheet = 50m;
        prices.DefaultSheetYield = 0.75;

        var svc = new PriceBreakdownService(prices);

        // Maple: 20 sqft → ceil(20/0.75/32) = 1 sheet
        // Cherry: 20 sqft → ceil(20/0.75/32) = 1 sheet
        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 20.0, ["Cherry"] = 20.0 },
            new Dictionary<string, double> { ["Wood Maple"] = 10.0, ["Wood Cherry"] = 5.0 });

        // Maple sheets:  1 × (3.50 × 32) = $112.00
        // Cherry sheets: 1 × (5.00 × 32) = $160.00
        // Maple EB:      10 × $0.25       =   $2.50
        // Cherry EB:      5 × $0.30       =   $1.50
        // CNC:           64 sqft × ($50/32) = $100.00
        // Total = $376.00
        Assert.Equal(376.00m, result.Total);
    }

    //############################################################################################################

    [Fact]
    public void UnknownSpecies_PricedAtZero_NoException()
    {
        var prices = new FakeMaterialPrices();
        prices.AddSheet("Maple", 3.50m);
        prices.CncPricePerSheet = 50m;
        prices.DefaultSheetYield = 0.75;

        var svc = new PriceBreakdownService(prices);

        // "UnknownWood" has no price entry, should fall back to 0 price
        var result = svc.Build(
            new Dictionary<string, double> { ["UnknownWood"] = 10.0 },
            new Dictionary<string, double> { ["UnknownEB"] = 5.0 });

        // Unknown sheet @ $0/sqft = $0 sheets
        // Unknown EB @ $0/ft = $0
        // CNC: default 32 sqft sheet area, ceil(10/0.75/32) = 1 sheet → 32 sqft × $50/32 = $50
        Assert.Equal(50.00m, result.Total);
    }

    //############################################################################################################

    [Fact]
    public void TwoSpecies_LineItemsAreDistinct()
    {
        var prices = new FakeMaterialPrices();
        prices.AddSheet("Maple", 3.50m);
        prices.AddSheet("Cherry", 5.00m);
        prices.CncPricePerSheet = 50m;
        prices.DefaultSheetYield = 0.75;

        var svc = new PriceBreakdownService(prices);

        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 10.0, ["Cherry"] = 10.0 },
            new Dictionary<string, double>());

        // 2 sheet lines + 1 CNC line = 3
        Assert.Equal(3, result.Lines.Count);

        var mapleSheet = result.Lines.First(l => l.Species == "Maple");
        var cherrySheet = result.Lines.First(l => l.Species == "Cherry");
        Assert.NotEqual(mapleSheet.UnitPrice, cherrySheet.UnitPrice);
    }
}