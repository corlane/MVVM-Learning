using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests the material/edgebanding accumulator dictionaries on CabinetModel.
/// </summary>
public class CabinetModelAccumulatorTests
{
    //############################################################################################################
    // ResetAllMaterialAndEdgeTotals clears everything
    //############################################################################################################

    [Fact]
    public void ResetAllMaterialAndEdgeTotals_ClearsAllAccumulators()
    {
        var cab = new BaseCabinetModel();
        cab.MaterialAreaBySpecies["Maple"] = 10.0;
        cab.EdgeBandingLengthBySpecies["Wood Maple"] = 5.0;
        cab.FrontParts.Add(new FrontPartRow(
            CabinetNumber: 1,
            CabinetName: "Test",
            Type: "Door",
            Height: 24.0,
            Width: 12.0,
            Species: "Maple",
            GrainDirection: "Vertical"));
        cab.DrawerBoxes.Add(new DrawerBoxRow(
            CabinetNumber: 1,
            CabinetName: "Test",
            Type: "Drawer",
            Height: 4.0,
            Width: 12.0,
            Length: 20.0));

        cab.ResetAllMaterialAndEdgeTotals();

        Assert.Empty(cab.MaterialAreaBySpecies);
        Assert.Empty(cab.EdgeBandingLengthBySpecies);
        Assert.Empty(cab.FrontParts);
        Assert.Empty(cab.DrawerBoxes);
    }

    //############################################################################################################
    // TotalMaterialAreaFt2 sums all species
    //############################################################################################################

    [Fact]
    public void TotalMaterialAreaFt2_SumsAllSpecies()
    {
        var cab = new BaseCabinetModel();
        cab.MaterialAreaBySpecies["Maple"] = 10.5;
        cab.MaterialAreaBySpecies["Cherry"] = 5.25;

        Assert.Equal(15.75, cab.TotalMaterialAreaFt2, precision: 4);
    }

    //############################################################################################################
    // TotalEdgeBandingFeet sums all species
    //############################################################################################################

    [Fact]
    public void TotalEdgeBandingFeet_SumsAllSpecies()
    {
        var cab = new BaseCabinetModel();
        cab.EdgeBandingLengthBySpecies["Wood Maple"] = 20.0;
        cab.EdgeBandingLengthBySpecies["Wood Cherry"] = 8.5;

        Assert.Equal(28.5, cab.TotalEdgeBandingFeet, precision: 4);
    }

    //############################################################################################################
    // Empty accumulators return 0
    //############################################################################################################

    [Fact]
    public void EmptyAccumulators_ReturnZero()
    {
        var cab = new BaseCabinetModel();

        Assert.Equal(0.0, cab.TotalMaterialAreaFt2);
        Assert.Equal(0.0, cab.TotalEdgeBandingFeet);
    }

    //############################################################################################################
    // GeometryVersion increments on dimension changes
    //############################################################################################################

    [Fact]
    public void GeometryVersion_IncrementsOnWidthChange()
    {
        var cab = new BaseCabinetModel { Width = "24" };
        int v1 = cab.GeometryVersion;

        cab.Width = "30";

        Assert.True(cab.GeometryVersion > v1);
    }

    [Fact]
    public void GeometryVersion_IncrementsOnSpeciesChange()
    {
        var cab = new BaseCabinetModel { Species = "Maple" };
        int v1 = cab.GeometryVersion;

        cab.Species = "Cherry";

        Assert.True(cab.GeometryVersion > v1);
    }
}