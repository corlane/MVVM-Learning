// DoorSizeDimensionTests.cs
// ─────────────────────────────────────────────────────────────────────────────
// Verifies that CabinetPreviewBuilder.BuildCabinetWithResult correctly computes
// door width and door height for base-standard cabinets under several conditions:
//
//   1. Single door, no drawer — full-width door, full-height minus reveals + TK.
//   2. Two doors, no drawer   — each door is half the opening width minus gap/2.
//   3. Standard cabinet with 1 drawer — door height is shortened by the drawer
//      opening (opening1Height + 0.75 + 0.375 + gap/2 + bottomReveal + TK).
//   4. No toe-kick (HasTK = false) — door height increases by TK amount.
//
// All formulas mirror BaseCabinetBuilder.Standard.Doors.cs exactly.
// Uses STA thread wrapper (required for WPF Model3DGroup construction).
// ─────────────────────────────────────────────────────────────────────────────

using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Tests;

public class DoorSizeDimensionTests
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

    // ── helpers ──────────────────────────────────────────────────────────────

    private static BaseCabinetModel MakeStandardBase(
        string width = "24",
        string height = "34.5",
        int doorCount = 1,
        int drwCount = 0,
        string opening1Height = "",
        bool hasTk = true,
        string tkHeight = "4") => new()
    {
        Name = "TestDoor",
        Qty = 1,
        Style = CabinetStyles.Base.Standard,
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
        DoorCount = doorCount,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = false,
        IncDoorsInList = true,   // enough to trigger BuildDoors + result population
        DrillHingeHoles = false,
        EdgebandDoorsAndDrawers = false,
        LeftReveal = ".0625",
        RightReveal = ".0625",
        TopReveal = "0.4375",
        BottomReveal = ".0625",
        GapWidth = ".125",
        DrwCount = drwCount,
        DrwStyle = "Blum Tandem H/Equivalent Undermount",
        DrwFrontGrainDir = "Vertical",
        EqualizeAllDrwFronts = false,
        EqualizeBottomDrwFronts = false,
        OpeningHeight1 = opening1Height,
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
    /// Standard base, 1 door, no drawer.
    /// doorWidth  = 24 - (sideReveal * 2)  where sideReveal = (0.0625 + 0.0625) / 2 = 0.0625
    ///           = 24 - 0.125 = 23.875
    /// doorHeight = 34.5 - topReveal(0.4375) - bottomReveal(0.0625) - TK(4) = 30.0
    /// </summary>
    [Fact]
    public void DoorSize_1Door_NoDrw_Standard()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(doorCount: 1, drwCount: 0);
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(23.875, result.DoorWidth,  tolerance: 0.001);
            Assert.Equal(30.0,   result.DoorHeight, tolerance: 0.001);
        });
    }

    /// <summary>
    /// Standard base, 2 doors, no drawer.
    /// Each door width = (23.875 / 2) - (gap(0.125) / 2) = 11.9375 - 0.0625 = 11.875
    /// Door height unchanged = 30.0
    /// </summary>
    [Fact]
    public void DoorSize_2Doors_NoDrw_EachDoorHalfWidth()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(doorCount: 2, drwCount: 0);
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(11.875, result.DoorWidth,  tolerance: 0.001);
            Assert.Equal(30.0,   result.DoorHeight, tolerance: 0.001);
        });
    }

    /// <summary>
    /// Standard base with 1 drawer (opening1 = 6.375").
    /// doorHeight = 34.5 - opening1(6.375) - 0.75 - 0.375 - gap/2(0.0625) - bottomReveal(0.0625) - TK(4)
    ///           = 34.5 - 6.375 - 0.75 - 0.375 - 0.0625 - 0.0625 - 4 = 22.875
    /// </summary>
    [Fact]
    public void DoorSize_1Door_1Drw_DoorHeightShortenedByDrawerOpening()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(doorCount: 1, drwCount: 1, opening1Height: "6.375");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(23.875, result.DoorWidth,  tolerance: 0.001);
            Assert.Equal(22.875, result.DoorHeight, tolerance: 0.001);
        });
    }

    /// <summary>
    /// Standard base, 1 door, no TK.
    /// doorHeight = 34.5 - topReveal(0.4375) - bottomReveal(0.0625) - TK(0) = 34.0
    /// </summary>
    [Fact]
    public void DoorSize_1Door_NoTK_DoorHeightFullCabinetHeight()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(doorCount: 1, drwCount: 0, hasTk: false, tkHeight: "0");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(23.875, result.DoorWidth,  tolerance: 0.001);
            Assert.Equal(34.0,   result.DoorHeight, tolerance: 0.001);
        });
    }

    /// <summary>
    /// Wider cabinet (36") — door width scales correctly.
    /// doorWidth = 36 - 0.125 = 35.875  (1 door)
    /// doorHeight = 30.0  (same reveals + TK)
    /// </summary>
    [Fact]
    public void DoorSize_1Door_36Wide_WidthScales()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "36", doorCount: 1, drwCount: 0);
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(35.875, result.DoorWidth,  tolerance: 0.001);
            Assert.Equal(30.0,   result.DoorHeight, tolerance: 0.001);
        });
    }
}