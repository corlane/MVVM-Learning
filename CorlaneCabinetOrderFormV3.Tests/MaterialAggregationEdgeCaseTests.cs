using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Edge-case tests for MaterialYieldCalculator aggregation:
/// Qty=0, empty dictionaries, multiple custom species.
/// </summary>
public class MaterialAggregationEdgeCaseTests
{
    //############################################################################################################
    // Qty = 0 produces zero material
    //############################################################################################################

    [Fact]
    public void AggregateMaterialAreas_QtyZero_ProducesZero()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(0, null, null,
                new Dictionary<string, double> { ["Maple UP"] = 25.0 },
                new Dictionary<string, double> { ["Wood Maple"] = 10.0 })
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.True(result.ContainsKey("Maple UP"));
        Assert.Equal(0.0, result["Maple UP"]);
    }

    [Fact]
    public void AggregateEdgeBanding_QtyZero_ProducesZero()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(0, null, null,
                new Dictionary<string, double>(),
                new Dictionary<string, double> { ["Wood Maple"] = 10.0 })
        };

        var result = MaterialYieldCalculator.AggregateEdgeBanding(cabinets);

        Assert.True(result.ContainsKey("Wood Maple"));
        Assert.Equal(0.0, result["Wood Maple"]);
    }

    //############################################################################################################
    // Qty > 1 multiplies correctly
    //############################################################################################################

    [Fact]
    public void AggregateMaterialAreas_QtyThree_TriplesMaterial()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(3, null, null,
                new Dictionary<string, double> { ["Cherry UP"] = 10.0 },
                new Dictionary<string, double>())
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.Equal(30.0, result["Cherry UP"]);
    }

    //############################################################################################################
    // Empty material dictionary produces empty result
    //############################################################################################################

    [Fact]
    public void AggregateMaterialAreas_EmptyDictionary_ProducesEmptyResult()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(1, null, null,
                new Dictionary<string, double>(),
                new Dictionary<string, double>())
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.Empty(result);
    }

    //############################################################################################################
    // Multiple cabinets with different custom species stay separate
    //############################################################################################################

    [Fact]
    public void AggregateMaterialAreas_DifferentCustomSpecies_StaySeparate()
    {
        var cabinets = new[]
        {
            new CabinetMaterialSnapshot(1, "Bamboo", null,
                new Dictionary<string, double> { ["Custom UP"] = 10.0 },
                new Dictionary<string, double>()),
            new CabinetMaterialSnapshot(1, "White Ash", null,
                new Dictionary<string, double> { ["Custom UP"] = 8.0 },
                new Dictionary<string, double>())
        };

        var result = MaterialYieldCalculator.AggregateMaterialAreas(cabinets);

        Assert.Equal(10.0, result["Bamboo UP"]);
        Assert.Equal(8.0, result["White Ash UP"]);
        Assert.False(result.ContainsKey("Custom UP"));
    }

    //############################################################################################################
    // No cabinets produces empty result
    //############################################################################################################

    [Fact]
    public void AggregateMaterialAreas_NoCabinets_ProducesEmptyResult()
    {
        var result = MaterialYieldCalculator.AggregateMaterialAreas([]);

        Assert.Empty(result);
    }
}