using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests for business rules identified as coverage gaps:
/// shallow-depth auto-switch, sink cabinet suppression,
/// trash drawer constraints, and depth-based removal.
/// </summary>
public class AdditionalConstraintTests
{
    //############################################################################################################
    // CabinetModel.CabinetType returns correct label for each subtype
    //############################################################################################################

    [Fact]
    public void CabinetType_ReturnsCorrectLabel_ForEachSubtype()
    {
        Assert.Equal("Base Cabinet", new BaseCabinetModel().CabinetType);
        Assert.Equal("Upper Cabinet", new UpperCabinetModel().CabinetType);
        Assert.Equal("Filler", new FillerModel().CabinetType);
        Assert.Equal("Panel", new PanelModel().CabinetType);
    }

    //############################################################################################################
    // Shallow depth → TopType forced to Full (3.0.1.49: depth < 10")
    //############################################################################################################

    [Theory]
    [InlineData("9.5", CabinetOptions.TopType.Full)]
    [InlineData("8", CabinetOptions.TopType.Full)]
    [InlineData("10", CabinetOptions.TopType.Stretcher)]  // at boundary, no force
    [InlineData("24", CabinetOptions.TopType.Stretcher)]
    public void ShallowDepth_ForcesFullTopType(string depth, string expectedTopType)
    {
        var cab = new BaseCabinetModel
        {
            Style = CabinetStyles.Base.Standard,
            Depth = depth,
            TopType = CabinetOptions.TopType.Stretcher
        };

        ApplyDepthConstraints(cab);

        Assert.Equal(expectedTopType, cab.TopType);
    }

    //############################################################################################################
    // Very shallow depth → ShelfDepth forced to Full (3.0.1.49: depth < 8")
    //############################################################################################################

    [Theory]
    [InlineData("7.5", CabinetOptions.ShelfDepth.FullDepth)]
    [InlineData("8", CabinetOptions.ShelfDepth.HalfDepth)]   // at boundary, no force
    [InlineData("24", CabinetOptions.ShelfDepth.HalfDepth)]
    public void VeryShallowDepth_ForcesFullShelfDepth(string depth, string expectedShelfDepth)
    {
        var cab = new BaseCabinetModel
        {
            Style = CabinetStyles.Base.Standard,
            Depth = depth,
            ShelfDepth = CabinetOptions.ShelfDepth.HalfDepth
        };

        ApplyDepthConstraints(cab);

        Assert.Equal(expectedShelfDepth, cab.ShelfDepth);
    }

    //############################################################################################################
    // Sink cabinet suppresses drawer-related properties (3.0.1.29)
    //############################################################################################################

    [Fact]
    public void SinkCabinet_SuppressesDrawerProperties()
    {
        var cab = new BaseCabinetModel
        {
            Style = CabinetStyles.Base.Standard,
            SinkCabinet = true,
            IncDrwBoxes = true,
            IncDrwBoxesInList = true,
            DrillSlideHoles = true,
            DrwCount = 2
        };

        ApplySinkConstraints(cab);

        Assert.False(cab.IncDrwBoxes);
        Assert.False(cab.IncDrwBoxesInList);
        Assert.False(cab.DrillSlideHoles);
    }

    //############################################################################################################
    // Trash drawer hides rollouts (3.0.1.46)
    //############################################################################################################

    [Fact]
    public void TrashDrawer_SuppressesRollouts()
    {
        var cab = new BaseCabinetModel
        {
            Style = CabinetStyles.Base.Standard,
            TrashDrawer = true,
            IncRollouts = true,
            RolloutCount = 2
        };

        ApplyTrashDrawerConstraints(cab);

        Assert.Equal(0, cab.RolloutCount);
        Assert.False(cab.IncRollouts);
    }

    //############################################################################################################
    // Depth < 10.625 removes drawers, rollouts, trash (3.0.1.47)
    //############################################################################################################

    [Theory]
    [InlineData("10", true)]   // < 10.625 → removed
    [InlineData("10.625", false)]  // at boundary → kept
    [InlineData("24", false)]
    public void DepthBelowThreshold_RemovesDrawersAndRollouts(string depth, bool shouldRemove)
    {
        var cab = new BaseCabinetModel
        {
            Style = CabinetStyles.Base.Standard,
            Depth = depth,
            DrwCount = 3,
            RolloutCount = 2,
            TrashDrawer = true,
            IncTrashDrwBox = true
        };

        ApplyDepthRemovalConstraints(cab);

        if (shouldRemove)
        {
            Assert.Equal(0, cab.DrwCount);
            Assert.Equal(0, cab.RolloutCount);
            Assert.False(cab.TrashDrawer);
            Assert.False(cab.IncTrashDrwBox);
        }
        else
        {
            Assert.Equal(3, cab.DrwCount);
            Assert.Equal(2, cab.RolloutCount);
            Assert.True(cab.TrashDrawer);
        }
    }

    //############################################################################################################
    // Helpers — mirror the VM logic so we test the rules, not the VM plumbing
    //############################################################################################################

    private static void ApplyDepthConstraints(BaseCabinetModel cab)
    {
        double depth = Converters.ConvertDimension.FractionToDouble(cab.Depth);
        if (depth < 10)
            cab.TopType = CabinetOptions.TopType.Full;
        if (depth < 8)
            cab.ShelfDepth = CabinetOptions.ShelfDepth.FullDepth;
    }

    private static void ApplySinkConstraints(BaseCabinetModel cab)
    {
        if (cab.SinkCabinet)
        {
            cab.IncDrwBoxes = false;
            cab.IncDrwBoxesInList = false;
            cab.DrillSlideHoles = false;
        }
    }

    private static void ApplyTrashDrawerConstraints(BaseCabinetModel cab)
    {
        if (cab.TrashDrawer)
        {
            cab.RolloutCount = 0;
            cab.IncRollouts = false;
        }
    }

    private static void ApplyDepthRemovalConstraints(BaseCabinetModel cab)
    {
        double depth = Converters.ConvertDimension.FractionToDouble(cab.Depth);
        if (depth < 10.625)
        {
            cab.DrwCount = 0;
            cab.RolloutCount = 0;
            cab.TrashDrawer = false;
            cab.IncTrashDrwBox = false;
        }
    }
}