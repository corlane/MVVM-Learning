using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Verifies that "Custom" species always resolves to the custom name for display and totals.
/// </summary>
public class CustomSpeciesResolutionTests
{
    //############################################################################################################
    // Door species resolution
    //############################################################################################################

    [Theory]
    [InlineData("Maple", "anything", "Maple")]
    [InlineData("Cherry", "", "Cherry")]
    [InlineData("Red Oak", "ignored", "Red Oak")]
    public void NonCustomSpecies_ReturnsSpeciesDirectly(string species, string customName, string expected)
    {
        Assert.Equal(expected, CabinetBuildHelpers.ResolveDoorSpeciesForTotals(species, customName));
    }

    [Theory]
    [InlineData("Custom", "Bamboo", "Bamboo")]
    [InlineData("Custom", "White Ash", "White Ash")]
    [InlineData("Custom", "My Special Wood", "My Special Wood")]
    public void CustomSpecies_ReturnsCustomName(string species, string customName, string expected)
    {
        Assert.Equal(expected, CabinetBuildHelpers.ResolveDoorSpeciesForTotals(species, customName));
    }

    [Theory]
    [InlineData("Custom", "", "Custom")]
    [InlineData("Custom", null, "Custom")]
    [InlineData("Custom", "   ", "Custom")]
    public void CustomSpecies_BlankCustomName_FallsBackToCustomLiteral(string species, string? customName, string expected)
    {
        Assert.Equal(expected, CabinetBuildHelpers.ResolveDoorSpeciesForTotals(species, customName!));
    }

    //############################################################################################################
    // Edgebanding species mapping for custom materials
    //############################################################################################################

    [Fact]
    public void CustomMaterial_EdgebandingMapsToNone()
    {
        // "Custom" isn't in the known species list, so EB should map to "None"
        Assert.Equal("None", CabinetBuildHelpers.GetMatchingEdgebandingSpecies("Custom"));
    }

    [Theory]
    [InlineData("Bamboo")]
    [InlineData("White Ash")]
    [InlineData("SomeNewWood")]
    public void UnknownSpecies_EdgebandingMapsToNone(string species)
    {
        Assert.Equal("None", CabinetBuildHelpers.GetMatchingEdgebandingSpecies(species));
    }
}