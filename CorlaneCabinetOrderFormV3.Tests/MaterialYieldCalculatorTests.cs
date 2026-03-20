using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

public class MaterialYieldCalculatorTests
{
    [Theory]
    [InlineData(32.0, 32.0, 0.82, 2)]   // 32 sqft at 82% yield on a 4x8 = ceil(32/0.82/32) = ceil(1.22) = 2
    [InlineData(16.0, 32.0, 0.82, 1)]   // half a sheet
    [InlineData(0.0, 32.0, 0.82, 0)]    // nothing needed
    public void ComputeSheetCount_VariousInputs(double sqFt, double sheetArea, double yield, int expected)
    {
        int sheets = MaterialYieldCalculator.ComputeSheetCount(sqFt, sheetArea, yield);
        Assert.Equal(expected, sheets);
    }

    [Fact]
    public void ComputeSheetCount_ZeroYield_ReturnsZero()
    {
        Assert.Equal(0, MaterialYieldCalculator.ComputeSheetCount(100, 32, 0));
    }

    [Fact]
    public void ComputeSheetCount_ZeroSheetArea_ReturnsZero()
    {
        Assert.Equal(0, MaterialYieldCalculator.ComputeSheetCount(100, 0, 0.82));
    }

    [Fact]
    public void AggregateMaterialAreas_CombinesDuplicateSpecies()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(1, null, null,
                new Dictionary<string, double> { ["Maple UP"] = 10 },
                new Dictionary<string, double>()),
            new CabinetMaterialSnapshot(2, null, null,
                new Dictionary<string, double> { ["Maple UP"] = 5 },
                new Dictionary<string, double>())
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.Equal(20.0, result["Maple UP"]); // 10*1 + 5*2
    }

    [Fact]
    public void AggregateMaterialAreas_ResolvesCustomSpecies()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(1, "Bamboo", null,
                new Dictionary<string, double> { ["Custom UP"] = 8 },
                new Dictionary<string, double>())
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.True(result.ContainsKey("Bamboo UP"));
        Assert.Equal(8.0, result["Bamboo UP"]);
    }

    [Theory]
    [InlineData("Maple UP", "Maple")]
    [InlineData("Cherry DOWN", "Cherry")]
    [InlineData("Walnut", "Walnut")]
    [InlineData("", "None")]
    public void CollapseFaceKey_RemovesSuffix(string input, string expected)
    {
        Assert.Equal(expected, MaterialYieldCalculator.CollapseFaceKey(input));
    }
}