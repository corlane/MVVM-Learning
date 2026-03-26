using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.Tests;

public class Base_BSNY2012F3FYN_Mat_And_EB_Matches_Expected
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

    // This is a large cabinet with 1 shelf and 2 rollouts pulled from 3041 Oneta Woodworks 364 GA job.
    // Numbers pulled from eCabs  BSNY2012F3FYN

    [Fact]
    public void Base_BSNY2012F3FYN_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "46.5", height: "85", depth: "19.25", shelfCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 11583 in² of 3/4 ply according to eCabs WITH JOINERY REMOVED. This is the number we want to match, as the joinery is not currently included in our material area calculations.
            double expectedFt2 = 11583 / 144.0;

            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, tolerance: .5);
        });
    }

    [Fact]
    public void Base_BSNY2012F3FYN_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "46.5", height: "85", depth: "19.25", shelfCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 549 in
            double expectedFt = 549 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, tolerance: 2);
        });
    }



    /// <summary>
    /// Creates a Standard base cabinet model with the given parameters.
    /// </summary>
    private static BaseCabinetModel MakeStandardBase(
        string width, string height, string depth,
        int shelfCount, int doorCount = 0) => new()
        {
            // ── CabinetModel (base) ──
            Name = "Test",
            Qty = 1,
            Style = CabinetStyles.Base.Standard,
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
            BackThickness = "3/4",
            TopType = CabinetOptions.TopType.Full,

            // ── Toe Kick ──
            HasTK = true,
            TKHeight = "4",
            TKDepth = "0",

            // ── Shelves ──
            ShelfCount = shelfCount,
            ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
            DrillShelfHoles = true,

            // ── Doors ──
            DoorCount = doorCount,
            DoorSpecies = "Maple",
            CustomDoorSpecies = "",
            DoorGrainDir = "Vertical",
            IncDoors = false,
            IncDoorsInList = false,
            DrillHingeHoles = false,

            // ── Reveals / Gaps ──
            LeftReveal = ".0625",
            RightReveal = ".0625",
            TopReveal = "0.4375",
            BottomReveal = ".0625",
            GapWidth = ".125",

            // ── Drawers (count & style) ──
            DrwCount = 0,
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

            // ── Drill Slide Holes ──
            DrillSlideHoles = false,
            DrillSlideHolesOpening1 = false,
            DrillSlideHolesOpening2 = false,
            DrillSlideHolesOpening3 = false,
            DrillSlideHolesOpening4 = false,

            // ── Rollouts ──
            IncRollouts = true,
            IncRolloutsInList = false,
            RolloutCount = 2,
            RolloutStyle = "Blum Tandem H/Equivalent Undermount",
            DrillSlideHolesForRollouts = true,

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
