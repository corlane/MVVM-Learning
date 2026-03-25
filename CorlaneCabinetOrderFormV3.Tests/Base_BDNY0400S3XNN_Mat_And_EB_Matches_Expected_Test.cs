using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class Base_BDNY0400S3XNN_Mat_And_EB_Matches_Expected_Test
{
    /// <summary>
    /// Runs an action on an STA thread (required by WPF 3D types used in the builder).
    /// </summary>
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

    // Numbers pulled from eCabs  BSNY1000F3XNN

    [Fact]
    public void Base_BDNY0400S3XNN_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "34.5", depth: "24", shelfCount: 0, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 7114 in² of 3/4 ply according to eCabs WITH JOINERY REMOVED. This is the number we want to match, as the joinery is not currently included in our material area calculations.
            double expectedFt2 = 7114 / 144.0;

            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 0);
        });
    }

    [Fact]
    public void Base_BDNY0400S3XNN_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "34.5", depth: "24", shelfCount: 0, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            double expectedFt = 504 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 0);
        });
    }



    /// <summary>
    /// Creates a Standard base cabinet model with the given parameters.
    /// </summary>
    private static BaseCabinetModel MakeStandardBase(
        string width, string height, string depth,
        int shelfCount = 0, int doorCount = 0, string BackThickness = "3/4") => new()
        {
            // ── CabinetModel (base) ──
            Name = "Test",
            Qty = 1,
            Style = CabinetStyles.Base.Drawer,
            Width = width,
            Height = height,
            Depth = depth,
            Species = "Maple",
            CustomSpecies = "",
            EBSpecies = "Wood Maple",
            CustomEBSpecies = "",
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25,
            Notes = "",

            // ── Back / Top ──
            BackThickness = BackThickness,
            TopType = CabinetOptions.TopType.Stretcher,

            // ── Toe Kick ──
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",

            // ── Shelves ──
            ShelfCount = shelfCount,
            ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
            DrillShelfHoles = false,

            // ── Doors ──
            DoorCount = doorCount,
            DoorSpecies = "Maple",
            CustomDoorSpecies = "",
            DoorGrainDir = "Vertical",
            IncDoors = false,
            IncDoorsInList = false,
            DrillHingeHoles = false,

            // ── Reveals / Gaps ──
            LeftReveal = "1/8",
            RightReveal = "1/8",
            TopReveal = "1/8",
            BottomReveal = "1/8",
            GapWidth = "1/8",

            // ── Drawers (count & style) ──
            DrwCount = 4,
            DrwStyle = "Blum Tandem H/Equivalent Undermount",
            DrwFrontGrainDir = "Vertical",
            EqualizeAllDrwFronts = false,
            EqualizeBottomDrwFronts = false,

            // ── Opening Heights (4 equal openings for 34.5h − 4 TK − 3.75 decks = 26.75 / 4) ──
            OpeningHeight1 = "6.6875",
            OpeningHeight2 = "6.6875",
            OpeningHeight3 = "6.6875",
            OpeningHeight4 = "6.6875",

            // ── Drawer Front Heights (computed from openings, reveals & gap) ──
            DrwFrontHeight1 = "7.625",
            DrwFrontHeight2 = "7.3125",
            DrwFrontHeight3 = "7.3125",
            DrwFrontHeight4 = "7.625",

            // ── Drawer Fronts (include per-opening) ──
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

            // ── Drawer Boxes (include per-opening) ──
            IncDrwBoxes = true,
            IncDrwBoxesInList = false,
            IncDrwBoxOpening1 = true,
            IncDrwBoxOpening2 = true,
            IncDrwBoxOpening3 = true,
            IncDrwBoxOpening4 = true,
            IncDrwBoxInListOpening1 = false,
            IncDrwBoxInListOpening2 = false,
            IncDrwBoxInListOpening3 = false,
            IncDrwBoxInListOpening4 = false,

            // ── Drill Slide Holes ──
            DrillSlideHoles = false,
            DrillSlideHolesOpening1 = false,
            DrillSlideHolesOpening2 = false,
            DrillSlideHolesOpening3 = false,
            DrillSlideHolesOpening4 = false,

            // ── Rollouts ──
            IncRollouts = false,
            IncRolloutsInList = false,
            RolloutCount = 0,
            RolloutStyle = "Blum Tandem H/Equivalent Undermount",
            DrillSlideHolesForRollouts = false,

            // ── Misc ──
            SinkCabinet = false,
            TrashDrawer = false,

            // ── Corner-only (leave empty for standard) ──
            LeftBackWidth = "",
            RightBackWidth = "",
            LeftFrontWidth = "",
            RightFrontWidth = "",
            LeftDepth = "",
            RightDepth = "",
            FrontWidth = "",
        };
}
