// DrawerFrontWidthTests.cs
// ─────────────────────────────────────────────────────────────────────────────
// Verifies that CabinetPreviewBuilder.BuildCabinetWithResult correctly populates
// CabinetBuildResult.DrawerFrontWidth for base-drawer cabinets under several
// reveal and width configurations.
//
// DrawerFrontWidth is computed in BaseCabinetBuilder.Standard.DrawerFronts.cs as:
//   drwFrontWidth = cabinetWidth - (DoorSideReveal * 2)
// where:
//   DoorSideReveal = (leftReveal + rightReveal) / 2
//
// So the expanded formula is:
//   DrawerFrontWidth = cabinetWidth - leftReveal - rightReveal
//
// Tests cover:
//   1. Symmetric equal reveals (0.0625 each) — standard factory default.
//   2. Asymmetric reveals (left=0.125, right=0.0625).
//   3. Wider cabinet (36") to confirm formula scales linearly.
//   4. Zero reveals — front equals full cabinet width.
//
// Uses STA thread wrapper (required for WPF Model3DGroup construction).
// Mirrors the test structure established in DoorSizeDimensionTests.cs.
// ─────────────────────────────────────────────────────────────────────────────

using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class DrawerFrontWidthTests
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

    // ── helper ────────────────────────────────────────────────────────────────

    private static BaseCabinetModel MakeDrawerBase(
        string width = "24",
        string height = "34.5",
        int drwCount = 4,
        string leftReveal = ".0625",
        string rightReveal = ".0625",
        bool hasTk = true,
        string tkHeight = "4") => new()
    {
        Name = "TestDrawerFront",
        Qty = 1,
        Style = CabinetStyles.Base.Drawer,
        Width = width,
        Height = height,
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
        HasTK = hasTk,
        TKHeight = tkHeight,
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
        EdgebandDoorsAndDrawers = false,
        LeftReveal = leftReveal,
        RightReveal = rightReveal,
        TopReveal = "0.4375",
        BottomReveal = ".0625",
        GapWidth = ".125",
        DrwCount = drwCount,
        DrwStyle = "Blum Tandem H/Equivalent Undermount",
        DrwFrontGrainDir = "Vertical",
        EqualizeAllDrwFronts = true,   // let the builder compute equal heights
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
        IncDrwFront1 = true,    // must be true for BuildDrawerFronts to run + populate result
        IncDrwFront2 = true,
        IncDrwFront3 = true,
        IncDrwFront4 = true,
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
        IncRollouts = false,
        IncRolloutsInList = false,
        RolloutCount = 0,
        RolloutStyle = "",
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
        HasTop = true,
        HasDeck = true,
        HasLeftEnd = true,
        HasRightEnd = true,
        HasBack = true,
        HasToeKickBoard = hasTk,
    };

    // ── tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 24" cabinet, symmetric reveals (0.0625 + 0.0625).
    /// DrawerFrontWidth = 24 - 0.0625 - 0.0625 = 23.875
    /// </summary>
    [Fact]
    public void DrawerFrontWidth_SymmetricReveals_Standard()
    {
        RunOnSta(() =>
        {
            var cab = MakeDrawerBase(width: "24", leftReveal: ".0625", rightReveal: ".0625");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(23.875, result.DrawerFrontWidth, tolerance: 0.001);
        });
    }

    /// <summary>
    /// 24" cabinet, asymmetric reveals (left=0.125, right=0.0625).
    /// DrawerFrontWidth = 24 - 0.125 - 0.0625 = 23.8125
    /// </summary>
    [Fact]
    public void DrawerFrontWidth_AsymmetricReveals()
    {
        RunOnSta(() =>
        {
            var cab = MakeDrawerBase(width: "24", leftReveal: ".125", rightReveal: ".0625");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(23.8125, result.DrawerFrontWidth, tolerance: 0.001);
        });
    }

    /// <summary>
    /// 36" cabinet, symmetric reveals (0.0625 each).
    /// DrawerFrontWidth = 36 - 0.0625 - 0.0625 = 35.875
    /// </summary>
    [Fact]
    public void DrawerFrontWidth_WiderCabinet_ScalesLinearly()
    {
        RunOnSta(() =>
        {
            var cab = MakeDrawerBase(width: "36", leftReveal: ".0625", rightReveal: ".0625");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(35.875, result.DrawerFrontWidth, tolerance: 0.001);
        });
    }

    /// <summary>
    /// 24" cabinet, zero reveals on both sides.
    /// DrawerFrontWidth = 24 - 0 - 0 = 24.0
    /// </summary>
    [Fact]
    public void DrawerFrontWidth_ZeroReveals_EqualsFullWidth()
    {
        RunOnSta(() =>
        {
            var cab = MakeDrawerBase(width: "24", leftReveal: "0", rightReveal: "0");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(24.0, result.DrawerFrontWidth, tolerance: 0.001);
        });
    }
}