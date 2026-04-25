// Corner90DoorSizeDimensionTests.cs
// ─────────────────────────────────────────────────────────────────────────────
// Verifies that CabinetPreviewBuilder.BuildCabinetWithResult correctly computes
// Door1Width, Door2Width, and DoorHeight for Base 90-degree corner cabinets
// (CabinetStyles.Base.Corner90 / Type3).
//
// Door widths use an independent reveal per side PLUS a fixed 0.875" open-side
// reveal (cornerCabDoorOpenSideReveal) that accounts for the hinge-side clearance
// on each door. The two doors are independent — left front width drives door 1,
// right front width drives door 2.
//
// Formulas (from BaseCabinetBuilder.Corner90.Doors.cs):
//   door1Width = leftFrontWidth  - doorLeftReveal  - cornerCabDoorOpenSideReveal (0.875)
//   door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal (0.875)
//   doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height
//
// Uses STA thread wrapper (required for WPF Model3DGroup construction).
// ─────────────────────────────────────────────────────────────────────────────

using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Tests;

public class Corner90DoorSizeDimensionTests
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

    // ── helper ───────────────────────────────────────────────────────────────

    private static BaseCabinetModel MakeCorner90(
        string leftFrontWidth  = "12",
        string rightFrontWidth = "12",
        string height          = "34.5",
        bool   hasTk           = true,
        string tkHeight        = "4") => new()
    {
        Name                  = "TestCorner90Door",
        Qty                   = 1,
        Style                 = CabinetStyles.Base.Corner90,
        Width                 = "36",
        Height                = height,
        Depth                 = "24",
        Species               = "Maple",
        CustomSpecies         = "",
        EBSpecies             = "Wood Maple",
        CustomEBSpecies       = "",
        MaterialThickness34   = 0.75,
        MaterialThickness14   = 0.25,
        Notes                 = "",
        BackThickness         = "0.75",
        TopType               = CabinetOptions.TopType.Full,
        HasTK                 = hasTk,
        TKHeight              = tkHeight,
        TKDepth               = "3.75",
        ShelfCount            = 0,
        ShelfDepth            = CabinetOptions.ShelfDepth.FullDepth,
        DrillShelfHoles       = false,
        DoorCount             = 2,
        DoorSpecies           = "Maple",
        CustomDoorSpecies     = "",
        DoorGrainDir          = "Vertical",
        IncDoors              = false,
        IncDoorsInList        = true,   // triggers BuildDoors + result population
        DrillHingeHoles       = false,
        EdgebandDoorsAndDrawers = false,
        LeftReveal            = ".0625",
        RightReveal           = ".0625",
        TopReveal             = "0.4375",
        BottomReveal          = ".0625",
        GapWidth              = ".125",
        DrwCount              = 0,
        DrwStyle              = "Blum Tandem H/Equivalent Undermount",
        DrwFrontGrainDir      = "Vertical",
        EqualizeAllDrwFronts  = false,
        EqualizeBottomDrwFronts = false,
        OpeningHeight1 = "", OpeningHeight2 = "", OpeningHeight3 = "", OpeningHeight4 = "",
        DrwFrontHeight1 = "", DrwFrontHeight2 = "", DrwFrontHeight3 = "", DrwFrontHeight4 = "",
        IncDrwFronts = false, IncDrwFrontsInList = false,
        IncDrwFront1 = false, IncDrwFront2 = false, IncDrwFront3 = false, IncDrwFront4 = false,
        IncDrwFrontInList1 = false, IncDrwFrontInList2 = false, IncDrwFrontInList3 = false, IncDrwFrontInList4 = false,
        IncDrwBoxes = false, IncDrwBoxesInList = false,
        IncDrwBoxOpening1 = false, IncDrwBoxOpening2 = false, IncDrwBoxOpening3 = false, IncDrwBoxOpening4 = false,
        IncDrwBoxInListOpening1 = false, IncDrwBoxInListOpening2 = false, IncDrwBoxInListOpening3 = false, IncDrwBoxInListOpening4 = false,
        DrillSlideHoles = false,
        DrillSlideHolesOpening1 = false, DrillSlideHolesOpening2 = false, DrillSlideHolesOpening3 = false, DrillSlideHolesOpening4 = false,
        IncRollouts = false, IncRolloutsInList = false,
        RolloutCount = 0, RolloutStyle = "", DrillSlideHolesForRollouts = false,
        SinkCabinet = false, TrashDrawer = false, IncTrashDrwBox = false,
        LeftBackWidth  = "36",
        RightBackWidth = "36",
        LeftFrontWidth  = leftFrontWidth,
        RightFrontWidth = rightFrontWidth,
        LeftDepth  = "24",
        RightDepth = "24",
        FrontWidth = "",
        HasTop = true, HasDeck = true, HasLeftEnd = true, HasRightEnd = true,
        HasBack = true, HasToeKickBoard = hasTk,
    };

    // ── tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Symmetric corner (12" × 12"), with TK.
    /// door1Width = door2Width = 12 - 0.0625 - 0.875 = 11.0625
    /// doorHeight = 34.5 - 0.4375 - 0.0625 - 4.0 = 30.0
    /// </summary>
    [Fact]
    public void Corner90_DoorSizes_Symmetric_WithTK()
    {
        RunOnSta(() =>
        {
            var cab = MakeCorner90();
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(11.0625, result.Door1Width,  tolerance: 0.001);
            Assert.Equal(11.0625, result.Door2Width,  tolerance: 0.001);
            Assert.Equal(30.0,    result.DoorHeight,  tolerance: 0.001);
        });
    }

    /// <summary>
    /// No toe-kick — door height should increase by TK amount (4").
    /// doorHeight = 34.5 - 0.4375 - 0.0625 - 0.0 = 34.0
    /// </summary>
    [Fact]
    public void Corner90_DoorHeight_NoTK_IncreasedByTKAmount()
    {
        RunOnSta(() =>
        {
            var cab = MakeCorner90(hasTk: false, tkHeight: "0");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(34.0, result.DoorHeight, tolerance: 0.001);
        });
    }

    /// <summary>
    /// Asymmetric corner — left 15", right 12".
    /// door1Width = 15 - 0.0625 - 0.875 = 14.0625
    /// door2Width = 12 - 0.0625 - 0.875 = 11.0625
    /// </summary>
    [Fact]
    public void Corner90_DoorSizes_Asymmetric_LeftWiderThanRight()
    {
        RunOnSta(() =>
        {
            var cab = MakeCorner90(leftFrontWidth: "15", rightFrontWidth: "12");
            cab.ResetAllMaterialAndEdgeTotals();
            var result = CabinetPreviewBuilder.BuildCabinetWithResult(cab);

            Assert.Equal(14.0625, result.Door1Width, tolerance: 0.001);
            Assert.Equal(11.0625, result.Door2Width, tolerance: 0.001);
        });
    }
}