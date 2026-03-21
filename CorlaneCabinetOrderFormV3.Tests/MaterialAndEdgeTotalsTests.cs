using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class MaterialAndEdgeTotalsTests
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

    // UPPER CABINET TESTS


    [Fact]
    public void Upper12x12x12_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Left End (12×12) + Right End (12×12) + Deck (10.5×12) + Top (10.5×12) + Back¾ (10.5×10.5)
            // = 144 + 144 + 126 + 126 + 110.25 = 650.25 in² → 4.5156 ft²
            double expectedFt2 = 650.25 / 144.0;
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Upper12x12x12_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // EB edge 0 lengths: LeftEnd 12 + RightEnd 12 + Deck 10.5 + Top 10.5 + Back¾ 10.5
            // = 55.5 in → 4.625 ft
            double expectedFt = 55.5 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Upper12x12x12_1Door_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", doorCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Left End (12×12) + Right End (12×12) + Deck (10.5×12) + Top (10.5×12) + Back¾ (10.5×10.5)
            // = 144 + 144 + 126 + 126 + 110.25 = 650.25 in² → 4.5156 ft²
            double expectedFt2 = 650.25 / 144.0; // Cabinet
            expectedFt2 += 138.0625 / 144.0; // Door (11.75×11.75)
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Upper12x12x12_1Door_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", doorCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // EB edge 0 lengths: LeftEnd 12 + RightEnd 12 + Deck 10.5 + Top 10.5 + Back¾ 10.5
            // = 55.5 in → 4.625 ft
            double expectedFt = 55.5 / 12.0; //  Cabinet
            expectedFt += 47.0 / 12.0; // Door (11.75 + 11.75 + 11.75 + 11.75)
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Upper12x12x12_2Door_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", doorCount: 2);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Left End (12×12) + Right End (12×12) + Deck (10.5×12) + Top (10.5×12) + Back¾ (10.5×10.5)
            // = 144 + 144 + 126 + 126 + 110.25 = 650.25 in² → 4.5156 ft²
            double expectedFt2 = 650.25 / 144.0; // Cabinet
            expectedFt2 += 136.59375 / 144.0; // Doors (11.75×11.75 with 1/8 gap)
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Upper12x12x12_2Door_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", doorCount: 2);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // EB edge 0 lengths: LeftEnd 12 + RightEnd 12 + Deck 10.5 + Top 10.5 + Back¾ 10.5
            // = 55.5 in → 4.625 ft
            double expectedFt = 55.5 / 12.0; //  Cabinet
            expectedFt += 70.25 / 12.0; // 2 Doors (11.75 x 5.8125, with 1/8 gap)
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Upper12x12x12_1Shelf_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", shelfCount: 1, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Left End (12×12) + Right End (12×12) + Deck (10.5×12) + Top (10.5×12) + Back¾ (10.5×10.5)
            // = 144 + 144 + 126 + 126 + 110.25 = 650.25 in² → 4.5156 ft²
            double expectedFt2 = 650.25 / 144.0;
            expectedFt2 += 115.421875 / 144.0; // Shelf (11.125 x 10.375, with 1/16 gap on each side & 1/8 front setback)
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Upper12x12x12_1Shelf_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper(width: "12", height: "12", depth: "12", shelfCount: 1, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // EB edge 0 lengths: LeftEnd 12 + RightEnd 12 + Deck 10.5 + Top 10.5 + Back¾ 10.5
            // = 55.5 in → 4.625 ft
            double expectedFt = 55.5 / 12.0;
            expectedFt += 10.375 / 12.0; // Shelf front edge only
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }


    /// <summary>
    /// Creates a Standard upper cabinet model with the given dimensions,
    /// Maple species, Wood Maple edgebanding, ¾" back, 0 shelves, 0 doors.
    /// </summary>
    private static UpperCabinetModel MakeStandardUpper(
        string width, string height, string depth,
        int shelfCount = 0, int doorCount = 0, string BackThickness = null) => new()
    {
        Style = CabinetStyles.Upper.Standard,
        Width = width,
        Height = height,
        Depth = depth,
        Species = "Maple",
        EBSpecies = "Wood Maple",
        BackThickness = "3/4",
        ShelfCount = shelfCount,
        DoorCount = doorCount,
        IncDoors = true,
        IncDoorsInList = false,
        DrillShelfHoles = false,
        DrillHingeHoles = false,
        DoorSpecies = "Maple",
        DoorGrainDir = "Vertical",
        LeftReveal = "1/8",
        RightReveal = "1/8",
        TopReveal = "1/8",
        BottomReveal = "1/8",
        GapWidth = "1/8",   
    };



    //############################################################################################################

    // BASE CABINET TESTS -- NO TOEKICK, NO DRAWERS, FULL TOP, ¾" BACK


    [Fact]
    public void Base24x30x24_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Left End (24×30) + Right End (24×30) + Deck (22.5×24) + Top Full (22.5×24) + Back¾ (22.5×28.5)
            // = 720 + 720 + 540 + 540 + 641.25 = 3161.25 in² → 21.953125 ft²
            double expectedFt2 = 3161.25 / 144.0;
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Base24x30x24_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // EB edge 0 lengths: LeftEnd 30 + RightEnd 30 + Deck 22.5 + Top 22.5 + Back¾ 22.5
            // = 127.5 in → 10.625 ft
            double expectedFt = 127.5 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Base24x30x24_1Door_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", doorCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 3161.25 in²
            double expectedFt2 = 3161.25 / 144.0;
            // Door: doorW = 24 - 0.25 = 23.75, doorH = 30 - 0.125 - 0.125 = 29.75 → 706.5625 in²
            expectedFt2 += 706.5625 / 144.0;
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Base24x30x24_1Door_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", doorCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 127.5 in
            double expectedFt = 127.5 / 12.0;
            // Door TBLR: 2×23.75 + 2×29.75 = 107.0 in
            expectedFt += 107.0 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Base24x30x24_2Door_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", doorCount: 2);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 3161.25 in²
            double expectedFt2 = 3161.25 / 144.0;
            // 2 Doors: each (23.75/2 - 0.0625) = 11.8125 wide × 29.75 tall = 351.421875 in²
            expectedFt2 += (351.421875 * 2) / 144.0;
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Base24x30x24_2Door_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", doorCount: 2);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 127.5 in
            double expectedFt = 127.5 / 12.0;
            // 2 Doors TBLR: each 2×11.8125 + 2×29.75 = 83.125 in, total 166.25 in
            expectedFt += 166.25 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }

    //############################################################################################################

    [Fact]
    public void Base24x30x24_1Shelf_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", shelfCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 3161.25 in²
            double expectedFt2 = 3161.25 / 144.0;
            // Shelf: (22.5-0.125) × 23.25 = 22.375 × 23.25 = 520.21875 in²
            expectedFt2 += 520.21875 / 144.0;
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 2);
        });
    }

    [Fact]
    public void Base24x30x24_1Shelf_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "30", depth: "24", shelfCount: 1);
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 127.5 in
            double expectedFt = 127.5 / 12.0;
            // Shelf front edge: 22.375 in
            expectedFt += 22.375 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 2);
        });
    }



    //############################################################################################################

    // BASE CABINET TESTS -- TOEKICK, 1 DRAWER, STRETCHER TOP, 1/4" BACK












    /// <summary>
    /// Creates a Standard base cabinet model with the given dimensions.
    /// Maple species, Wood Maple edgebanding, ¾" back, Full top, no toe kick.
    /// </summary>
    private static BaseCabinetModel MakeStandardBase(
        string width, string height, string depth,
        int shelfCount = 0, int doorCount = 0) => new()
        {
            Style = CabinetStyles.Base.Standard,
            Width = width,
            Height = height,
            Depth = depth,
            Species = "Maple",
            EBSpecies = "Wood Maple",
            BackThickness = "3/4",
            TopType = CabinetOptions.TopType.Full,
            HasTK = false,
            TKHeight = "4",
            TKDepth = "3",
            ShelfCount = shelfCount,
            ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
            DoorCount = doorCount,
            DrwCount = 0,
            IncDoors = true,
            IncDoorsInList = false,
            DrillShelfHoles = false,
            DrillHingeHoles = false,
            DoorSpecies = "Maple",
            DoorGrainDir = "Vertical",
            DrwFrontGrainDir = "Vertical",
            LeftReveal = "1/8",
            RightReveal = "1/8",
            TopReveal = "1/8",
            BottomReveal = "1/8",
            GapWidth = "1/8",
            SinkCabinet = false,
            TrashDrawer = false,
            IncRollouts = false,
            IncRolloutsInList = false,
            IncDrwBoxes = false,
            IncDrwBoxesInList = false,
            IncDrwFronts = false,
            IncDrwFrontsInList = false,
            DrwStyle = "Blum Tandem H/Equivalent Undermount"
        };
}
