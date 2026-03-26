using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class UpperCabinetTest
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
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 0);
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
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 0);
        });
    }




    /// <summary>
    /// Creates a Standard upper cabinet model with the given parameters.
    /// </summary>
    private static UpperCabinetModel MakeStandardUpper(
        string width, string height, string depth,
        int shelfCount = 0, int doorCount = 0, string BackThickness = "3/4") => new()
        {
            // ── CabinetModel (base) ──
            Name = "Test",
            Qty = 1,
            Style = CabinetStyles.Upper.Standard,
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

            // ── Back ──
            BackThickness = BackThickness,

            // ── Shelves ──
            ShelfCount = shelfCount,
            DrillShelfHoles = false,

            // ── Doors ──
            DoorCount = doorCount,
            DoorSpecies = "Maple",
            CustomDoorSpecies = "",
            DoorGrainDir = "Vertical",
            IncDoors = true,
            IncDoorsInList = false,
            DrillHingeHoles = false,

            // ── Reveals / Gaps ──
            LeftReveal = "1/8",
            RightReveal = "1/8",
            TopReveal = "1/8",
            BottomReveal = "1/8",
            GapWidth = "1/8",

            // ── Corner-only (leave empty for standard) ──
            LeftBackWidth = "",
            RightBackWidth = "",
            LeftFrontWidth = "",
            RightFrontWidth = "",
            LeftDepth = "",
            RightDepth = "",
        };
}
