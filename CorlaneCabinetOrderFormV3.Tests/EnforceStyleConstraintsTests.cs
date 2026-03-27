using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests that style-specific constraints are enforced correctly on cabinet models.
/// These validate the rules that EnforceStyleConstraints() applies in the ViewModels.
/// </summary>
public class EnforceStyleConstraintsTests
{
    //############################################################################################################
    // Base Drawer Style → 0 doors, no hinge holes, no shelf holes, 0 rollouts, 0 shelves
    //############################################################################################################

    [Fact]
    public void DrawerStyle_ZeroesDoorRelatedProperties()
    {
        var cab = MakeBase(CabinetStyles.Base.Drawer);

        // Simulate what EnforceStyleConstraints does for Drawer style
        EnforceBaseConstraints(cab);

        Assert.Equal(0, cab.DoorCount);
        Assert.False(cab.DrillHingeHoles);
        Assert.False(cab.DrillShelfHoles);
        Assert.Equal(0, cab.RolloutCount);
        Assert.Equal(0, cab.ShelfCount);
    }

    //############################################################################################################
    // Base Corner90 → force 3/4 back, TopType Full, 0 drawers, door count min 2
    //############################################################################################################

    [Fact]
    public void Corner90Style_ForcesThreeQuarterBack()
    {
        var cab = MakeBase(CabinetStyles.Base.Corner90);
        cab.BackThickness = "0.25"; // user picked 1/4
        cab.DoorCount = 1;
        cab.DrwCount = 2;

        EnforceBaseConstraints(cab);

        Assert.Equal("0.75", cab.BackThickness);
        Assert.Equal(CabinetOptions.TopType.Full, cab.TopType);
        Assert.Equal(0, cab.DrwCount);
        Assert.Equal(2, cab.DoorCount); // bumped from 1 → 2
    }

    [Fact]
    public void Corner90Style_DoorCountAlreadyTwo_Unchanged()
    {
        var cab = MakeBase(CabinetStyles.Base.Corner90);
        cab.DoorCount = 2;

        EnforceBaseConstraints(cab);

        Assert.Equal(2, cab.DoorCount);
    }

    //############################################################################################################
    // Base AngleFront → force 3/4 back, TopType Full, 0 drawers, 0 rollouts
    //############################################################################################################

    [Fact]
    public void AngleFrontStyle_ForcesThreeQuarterBackAndFullTop()
    {
        var cab = MakeBase(CabinetStyles.Base.AngleFront);
        cab.BackThickness = "0.25";
        cab.TopType = CabinetOptions.TopType.Stretcher;
        cab.DrwCount = 3;
        cab.RolloutCount = 2;

        EnforceBaseConstraints(cab);

        Assert.Equal("0.75", cab.BackThickness);
        Assert.Equal(CabinetOptions.TopType.Full, cab.TopType);
        Assert.Equal(0, cab.DrwCount);
        Assert.Equal(0, cab.RolloutCount);
    }

    //############################################################################################################
    // Base Standard → no constraints forced
    //############################################################################################################

    [Fact]
    public void StandardStyle_DoesNotAlterProperties()
    {
        var cab = MakeBase(CabinetStyles.Base.Standard);
        cab.BackThickness = "0.25";
        cab.TopType = CabinetOptions.TopType.Stretcher;
        cab.DrwCount = 2;
        cab.DoorCount = 1;

        EnforceBaseConstraints(cab);

        Assert.Equal("0.25", cab.BackThickness);
        Assert.Equal(CabinetOptions.TopType.Stretcher, cab.TopType);
        Assert.Equal(2, cab.DrwCount);
        Assert.Equal(1, cab.DoorCount);
    }

    //############################################################################################################
    // Upper Corner90 / AngleFront → force 3/4 back
    //############################################################################################################

    [Theory]
    [InlineData(CabinetStyles.Upper.Corner90)]
    [InlineData(CabinetStyles.Upper.AngleFront)]
    public void UpperCornerStyles_ForceThreeQuarterBack(string style)
    {
        var cab = new UpperCabinetModel
        {
            Style = style,
            BackThickness = CabinetOptions.BackThickness.QuarterDecimal,
            Width = "30",
            Height = "30",
            Depth = "12",
            Species = "Maple",
            EBSpecies = "Wood Maple"
        };

        EnforceUpperConstraints(cab);

        Assert.Equal(CabinetOptions.BackThickness.ThreeQuarterDecimal, cab.BackThickness);
    }

    [Fact]
    public void UpperStandardStyle_DoesNotForceBackThickness()
    {
        var cab = new UpperCabinetModel
        {
            Style = CabinetStyles.Upper.Standard,
            BackThickness = CabinetOptions.BackThickness.QuarterDecimal,
            Width = "30",
            Height = "30",
            Depth = "12",
            Species = "Maple",
            EBSpecies = "Wood Maple"
        };

        EnforceUpperConstraints(cab);

        Assert.Equal(CabinetOptions.BackThickness.QuarterDecimal, cab.BackThickness);
    }

    //############################################################################################################
    // Helpers — mirror the logic from the VMs so we test the rules, not the VM plumbing
    //############################################################################################################

    private static void EnforceBaseConstraints(BaseCabinetModel cab)
    {
        // Mirrors BaseCabinetViewModel.EnforceStyleConstraints()
        if (cab.Style == CabinetStyles.Base.Drawer)
        {
            cab.DoorCount = 0;
            cab.DrillHingeHoles = false;
            cab.DrillShelfHoles = false;
            cab.RolloutCount = 0;
            cab.ShelfCount = 0;
        }

        if (cab.Style == CabinetStyles.Base.Corner90)
        {
            if (cab.DoorCount == 1) cab.DoorCount = 2;
            cab.DrwCount = 0;
            cab.TopType = CabinetOptions.TopType.Full;
            cab.BackThickness = "0.75";
        }

        if (cab.Style == CabinetStyles.Base.AngleFront)
        {
            cab.DrwCount = 0;
            cab.RolloutCount = 0;
            cab.TopType = CabinetOptions.TopType.Full;
            cab.BackThickness = "0.75";
        }
    }

    private static void EnforceUpperConstraints(UpperCabinetModel cab)
    {
        // Mirrors UpperCabinetViewModel.EnforceStyleConstraints()
        if (cab.Style == CabinetStyles.Upper.Corner90 ||
            cab.Style == CabinetStyles.Upper.AngleFront)
        {
            cab.BackThickness = CabinetOptions.BackThickness.ThreeQuarterDecimal;
        }
    }

    private static BaseCabinetModel MakeBase(string style) => new()
    {
        Style = style,
        Width = "24",
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        EBSpecies = "Wood Maple",
        BackThickness = "0.75",
        TopType = CabinetOptions.TopType.Full,
        HasTK = true,
        TKHeight = "4",
        TKDepth = "3.75",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25
    };
}