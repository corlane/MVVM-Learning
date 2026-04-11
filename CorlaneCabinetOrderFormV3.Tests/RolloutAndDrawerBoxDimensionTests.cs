using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests rollout width/depth and drawer box dimension formulas via CabinetBuildResult.
/// Guards against regressions like the 3.0.1.39 rollout bracket fix (1.2 → 1.0).
/// </summary>
public class RolloutAndDrawerBoxDimensionTests
{
    private static void RunOnSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { caught = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
            throw caught;
    }

    //############################################################################################################
    // Rollout width — Blum style, 1 door, 1 rollout
    //############################################################################################################

    [Fact]
    public void RolloutWidth_Blum_1Door1Rollout_MatchesFormula()
    {
        RunOnSta(() =>
        {
            // 24" wide, Blum style, 1 door, 1 rollout
            // InteriorWidth = 24 - (0.75 * 2) = 22.5
            // Blum side spacing = 0.4
            // Bracket spacing = 1.0 per door × 1 door = 1.0
            // RolloutWidth = 22.5 - 0.4 - 1.0 = 21.1
            var cab = MakeBaseWithRollouts(width: "24", doorCount: 1, rolloutCount: 1, style: "Blum Tandem H/Equivalent Undermount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(21.1, result.RolloutWidth, tolerance: 0.001);
        });
    }

    [Fact]
    public void RolloutWidth_Blum_2Doors1Rollout_SubtractsTwoBrackets()
    {
        RunOnSta(() =>
        {
            // 24" wide, Blum style, 2 doors, 1 rollout
            // RolloutWidth = 22.5 - 0.4 - (1.0 × 2) = 20.1
            var cab = MakeBaseWithRollouts(width: "24", doorCount: 2, rolloutCount: 1, style: "Blum Tandem H/Equivalent Undermount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(20.1, result.RolloutWidth, tolerance: 0.001);
        });
    }

    [Fact]
    public void RolloutWidth_Accuride_1Door1Rollout_MatchesFormula()
    {
        RunOnSta(() =>
        {
            // 24" wide, Accuride style, 1 door, 1 rollout
            // InteriorWidth = 22.5
            // Accuride side spacing = 1.0
            // Bracket spacing = 1.0 × 1 = 1.0
            // RolloutWidth = 22.5 - 1.0 - 1.0 = 20.5
            var cab = MakeBaseWithRollouts(width: "24", doorCount: 1, rolloutCount: 1, style: "Accuride/Equivalent Sidemount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(20.5, result.RolloutWidth, tolerance: 0.001);
        });
    }

    //############################################################################################################
    // Rollout height is fixed at 4"
    //############################################################################################################

    [Fact]
    public void RolloutHeight_AlwaysFour()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseWithRollouts(width: "24", doorCount: 1, rolloutCount: 1, style: "Blum Tandem H/Equivalent Undermount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(4.0, result.RolloutHeight, tolerance: 0.001);
        });
    }

    //############################################################################################################
    // Rollout depth matches drawer box depth lookup
    //############################################################################################################

    [Theory]
    [InlineData("24", 21.0)]    // depth 24, 3/4 back → effective 24 → bracket 22.625+ → 21"
    [InlineData("21", 18.0)]    // depth 21, 3/4 back → effective 21 → bracket 19.625–22.625 → 18"
    [InlineData("18", 15.0)]    // depth 18 → bracket 16.625–19.625 → 15"
    [InlineData("15", 12.0)]    // depth 15 → bracket 13.625–16.625 → 12"
    [InlineData("12", 9.0)]     // depth 12 → bracket 10.625–13.625 → 9"
    public void RolloutDepth_MatchesDrawerBoxDepthLookup(string depth, double expectedDepth)
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseWithRollouts(width: "24", doorCount: 1, rolloutCount: 1, style: "Blum Tandem H/Equivalent Undermount");
            cab.Depth = depth;
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(expectedDepth, result.RolloutDepth, tolerance: 0.001);
        });
    }

    //############################################################################################################
    // Drawer box width — Blum vs Accuride
    //############################################################################################################

    [Fact]
    public void DrawerBoxWidth_Blum_MatchesFormula()
    {
        RunOnSta(() =>
        {
            // 24" wide, Blum: interiorWidth(22.5) - 0.4 = 22.1
            var cab = MakeBaseWithDrawers(width: "24", drwCount: 2, style: "Blum Tandem H/Equivalent Undermount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(22.1, result.DrawerBoxWidth, tolerance: 0.001);
        });
    }

    [Fact]
    public void DrawerBoxWidth_Accuride_MatchesFormula()
    {
        RunOnSta(() =>
        {
            // 24" wide, Accuride: interiorWidth(22.5) - 1.0 = 21.5
            var cab = MakeBaseWithDrawers(width: "24", drwCount: 2, style: "Accuride/Equivalent Sidemount");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(21.5, result.DrawerBoxWidth, tolerance: 0.001);
        });
    }

    //############################################################################################################
    // Helpers
    //############################################################################################################

    private static BaseCabinetModel MakeBaseWithRollouts(string width, int doorCount, int rolloutCount, string style) => new()
    {
        Name = "TestRollout",
        Qty = 1,
        Style = CabinetStyles.Base.Standard,
        Width = width,
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        BackThickness = "3/4",
        TopType = CabinetOptions.TopType.Full,
        HasTK = true,
        TKHeight = "4",
        TKDepth = "3.75",
        ShelfCount = 0,
        ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
        DrillShelfHoles = false,
        DoorCount = doorCount,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = false,
        IncDoorsInList = false,
        DrillHingeHoles = false,
        LeftReveal = ".0625",
        RightReveal = ".0625",
        TopReveal = "0.4375",
        BottomReveal = ".0625",
        GapWidth = ".125",
        DrwCount = 0,
        DrwStyle = style,
        DrwFrontGrainDir = "Vertical",
        EqualizeAllDrwFronts = false,
        EqualizeBottomDrwFronts = false,
        OpeningHeight1 = "",
        OpeningHeight2 = "",
        OpeningHeight3 = "",
        OpeningHeight4 = "",
        DrwFrontHeight1 = "",
        DrwFrontHeight2 = "",
        DrwFrontHeight3 = "",
        DrwFrontHeight4 = "",
        IncDrwFronts = false,
        IncDrwFrontsInList = false,
        IncDrwFront1 = false,
        IncDrwFront2 = false,
        IncDrwFront3 = false,
        IncDrwFront4 = false,
        IncDrwFrontInList1 = false,
        IncDrwFrontInList2 = false,
        IncDrwFrontInList3 = false,
        IncDrwFrontInList4 = false,
        IncDrwBoxes = false,
        IncDrwBoxesInList = false,
        IncDrwBoxOpening1 = false,
        IncDrwBoxOpening2 = false,
        IncDrwBoxOpening3 = false,
        IncDrwBoxOpening4 = false,
        IncDrwBoxInListOpening1 = false,
        IncDrwBoxInListOpening2 = false,
        IncDrwBoxInListOpening3 = false,
        IncDrwBoxInListOpening4 = false,
        DrillSlideHoles = false,
        DrillSlideHolesOpening1 = false,
        DrillSlideHolesOpening2 = false,
        DrillSlideHolesOpening3 = false,
        DrillSlideHolesOpening4 = false,
        IncRollouts = true,
        IncRolloutsInList = true,
        RolloutCount = rolloutCount,
        RolloutStyle = style,
        DrillSlideHolesForRollouts = false,
        SinkCabinet = false,
        TrashDrawer = false,
        IncTrashDrwBox = false,
        LeftBackWidth = "",
        RightBackWidth = "",
        LeftFrontWidth = "",
        RightFrontWidth = "",
        LeftDepth = "",
        RightDepth = "",
        FrontWidth = "",
    };

    private static BaseCabinetModel MakeBaseWithDrawers(string width, int drwCount, string style) => new()
    {
        Name = "TestDrawer",
        Qty = 1,
        Style = CabinetStyles.Base.Drawer,
        Width = width,
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        BackThickness = "3/4",
        TopType = CabinetOptions.TopType.Stretcher,
        HasTK = true,
        TKHeight = "4",
        TKDepth = "3.75",
        ShelfCount = 0,
        ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
        DrillShelfHoles = false,
        DoorCount = 0,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = false,
        IncDoorsInList = false,
        DrillHingeHoles = false,
        LeftReveal = ".0625",
        RightReveal = ".0625",
        TopReveal = "0.4375",
        BottomReveal = ".0625",
        GapWidth = ".125",
        DrwCount = drwCount,
        DrwStyle = style,
        DrwFrontGrainDir = "Vertical",
        EqualizeAllDrwFronts = false,
        EqualizeBottomDrwFronts = false,
        OpeningHeight1 = "6.6875",
        OpeningHeight2 = "6.6875",
        OpeningHeight3 = "6.6875",
        OpeningHeight4 = "6.6875",
        DrwFrontHeight1 = "7.625",
        DrwFrontHeight2 = "7.3125",
        DrwFrontHeight3 = "7.3125",
        DrwFrontHeight4 = "7.625",
        IncDrwFronts = false,
        IncDrwFrontsInList = false,
        IncDrwFront1 = false,
        IncDrwFront2 = false,
        IncDrwFront3 = false,
        IncDrwFront4 = false,
        IncDrwFrontInList1 = false,
        IncDrwFrontInList2 = false,
        IncDrwFrontInList3 = false,
        IncDrwFrontInList4 = false,
        IncDrwBoxes = true,
        IncDrwBoxesInList = true,
        IncDrwBoxOpening1 = true,
        IncDrwBoxOpening2 = true,
        IncDrwBoxOpening3 = false,
        IncDrwBoxOpening4 = false,
        IncDrwBoxInListOpening1 = true,
        IncDrwBoxInListOpening2 = true,
        IncDrwBoxInListOpening3 = false,
        IncDrwBoxInListOpening4 = false,
        DrillSlideHoles = false,
        DrillSlideHolesOpening1 = false,
        DrillSlideHolesOpening2 = false,
        DrillSlideHolesOpening3 = false,
        DrillSlideHolesOpening4 = false,
        IncRollouts = false,
        IncRolloutsInList = false,
        RolloutCount = 0,
        RolloutStyle = style,
        DrillSlideHolesForRollouts = false,
        SinkCabinet = false,
        TrashDrawer = false,
        IncTrashDrwBox = false,
        LeftBackWidth = "",
        RightBackWidth = "",
        LeftFrontWidth = "",
        RightFrontWidth = "",
        LeftDepth = "",
        RightDepth = "",
        FrontWidth = "",
    };
}