using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

public class PriceBreakdownServiceTests
{
    /// <summary>
    /// Minimal stub for IMaterialPricesService with a single plywood species and single EB species.
    /// </summary>
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

        public void AddYield(string species, double yield)
        {
            _yields[species] = yield;
        }

        public bool TryGetSheetMaterial(string species, out MaterialPriceRow row)
            => _sheets.TryGetValue(species, out row!);

        public bool TryGetEdgeBand(string species, out EdgeBandPriceRow row)
            => _eb.TryGetValue(species, out row!);

        public bool TryGetYield(string species, out double yield)
            => _yields.TryGetValue(species, out yield);
    }

    private static (FakeMaterialPrices prices, PriceBreakdownService svc) MakeService()
    {
        var prices = new FakeMaterialPrices();
        prices.AddSheet("Maple", pricePerSqFt: 3.50m);        // 48×96 = 32 sq ft sheet
        prices.AddEdgeBand("Wood Maple", pricePerFt: 0.25m);
        prices.CncPricePerSheet = 50m;                         // $50 per 48×96 sheet
        prices.DefaultSheetYield = 0.75;
        return (prices, new PriceBreakdownService(prices));
    }

    //############################################################################################################

    [Fact]
    public void EmptyInputs_ReturnsZeroTotal()
    {
        var (_, svc) = MakeService();

        var result = svc.Build(
            new Dictionary<string, double>(),
            new Dictionary<string, double>());

        Assert.Equal(0m, result.Total);
        Assert.Empty(result.Lines);
    }

    //############################################################################################################

    [Fact]
    public void SingleSheet_CalculatesCorrectTotal()
    {
        var (_, svc) = MakeService();

        // 20 sq ft of Maple at 75% yield → ceil((20/0.75)/32) = ceil(0.833) = 1 sheet
        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 20.0 },
            new Dictionary<string, double> { ["Wood Maple"] = 10.0 });

        // Sheet line: 1 sheet × ($3.50 × 32 sq ft) = $112.00
        // EB line:    10 ft × $0.25               =   $2.50
        // CNC line:   32 sq ft × ($50/32)         =  $50.00
        //                                    Total = $164.50
        Assert.Equal(164.50m, result.Total);
    }

    //############################################################################################################

    [Fact]
    public void MultipleSheets_RoundsUpCorrectly()
    {
        var (_, svc) = MakeService();

        // 25 sq ft at 75% yield → ceil((25/0.75)/32) = ceil(1.042) = 2 sheets
        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 25.0 },
            new Dictionary<string, double>());

        // Sheet line: 2 sheets × $112.00          = $224.00
        // CNC line:   64 sq ft × ($50/32)         = $100.00
        //                                    Total = $324.00
        Assert.Equal(324.00m, result.Total);
    }

    //############################################################################################################

    [Fact]
    public void NoneSpecies_PricedAtZero()
    {
        var (_, svc) = MakeService();

        var result = svc.Build(
            new Dictionary<string, double> { ["None"] = 5.0 }, 
            new Dictionary<string, double> { ["None"] = 8.0 });

        // "None" resolves to $0 price — no sheets billed, no EB billed, no CNC
        Assert.Equal(0m, result.Total);
    }

    //############################################################################################################

    [Fact]
    public void SingleSheet_LineItems_AreCorrect()
    {
        var (_, svc) = MakeService();

        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 20.0 },
            new Dictionary<string, double> { ["Wood Maple"] = 10.0 });

        // Should have 3 lines: Maple sheets, Wood Maple EB, CNC
        Assert.Equal(3, result.Lines.Count);

        var sheetLine = result.Lines.First(l => l.Species == "Maple");
        Assert.Equal(1, sheetLine.Quantity);        // 1 sheet
        Assert.Equal("Sheets", sheetLine.Unit);
        Assert.Equal(112.00m, sheetLine.UnitPrice); // $3.50/sqft × 32 sqft

        var ebLine = result.Lines.First(l => l.Species == "Wood Maple");
        Assert.Equal(10.0, ebLine.Quantity);
        Assert.Equal("ft", ebLine.Unit);
        Assert.Equal(0.25m, ebLine.UnitPrice);

        var cncLine = result.Lines.First(l => l.Species == "CNC cutting");
        Assert.Equal(32.0, cncLine.Quantity);        // 1 sheet × 32 sq ft
        Assert.Equal("Sq Ft", cncLine.Unit);
    }

    //############################################################################################################

    [Fact]
    public void CustomYield_OverridesDefault()
    {
        var (prices, svc) = MakeService();
        prices.AddYield("Maple", 0.50); // 50% yield override

        // 20 sq ft at 50% yield → ceil((20/0.50)/32) = ceil(1.25) = 2 sheets
        var result = svc.Build(
            new Dictionary<string, double> { ["Maple"] = 20.0 },
            new Dictionary<string, double>());

        // Sheet line: 2 × $112.00 = $224.00
        // CNC line:   64 × $1.5625 = $100.00
        // Total = $324.00
        Assert.Equal(324.00m, result.Total);
    }
}