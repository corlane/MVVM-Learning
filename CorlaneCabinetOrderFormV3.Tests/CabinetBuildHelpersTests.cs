using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class CabinetBuildHelpersTests
{
    [Theory]
    [InlineData("Maple", "Wood Maple")]
    [InlineData("Cherry", "Wood Cherry")]
    [InlineData("Red Oak", "Wood Red Oak")]
    [InlineData("White Oak", "Wood White Oak")]
    [InlineData("Walnut", "Wood Walnut")]
    [InlineData("Hickory", "Wood Hickory")]
    [InlineData("Alder", "Wood Alder")]
    [InlineData("Mahogany", "Wood Mahogany")]
    [InlineData("Prefinished Ply", "PVC Hardrock Maple")]
    [InlineData("PFP 1/4", "None")]
    [InlineData("MDF", "Wood Maple")]
    [InlineData("Melamine", "Melamine")]
    [InlineData(null, "None")]
    [InlineData("", "None")]
    [InlineData("SomeUnknownWood", "None")]
    public void GetMatchingEdgebandingSpecies_MapsCorrectly(string? input, string expected)
    {
        Assert.Equal(expected, CabinetBuildHelpers.GetMatchingEdgebandingSpecies(input));
    }

    [Fact]
    public void ResolveDoorSpeciesForTotals_NonCustom_ReturnsSpecies()
    {
        Assert.Equal("Maple", CabinetBuildHelpers.ResolveDoorSpeciesForTotals("Maple", "anything"));
    }

    [Fact]
    public void ResolveDoorSpeciesForTotals_Custom_ReturnsCustomName()
    {
        Assert.Equal("Bamboo", CabinetBuildHelpers.ResolveDoorSpeciesForTotals("Custom", "Bamboo"));
    }

    [Fact]
    public void ResolveDoorSpeciesForTotals_Custom_BlankCustomName_ReturnsFallback()
    {
        Assert.Equal("Custom", CabinetBuildHelpers.ResolveDoorSpeciesForTotals("Custom", ""));
    }
}