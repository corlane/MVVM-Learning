using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

public class Base_BSNY1000F3XNN_Mat_And_EB_Matches_Expected_Test
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
    public void Base_BSNY1000F3XNN_MaterialArea_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "34.5", depth: "24", shelfCount: 0, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 3549 in² of 3/4 ply
            double expectedFt2 = 3549 / 144.0;
            
            Assert.Equal(expectedFt2, cab.TotalMaterialAreaFt2, precision: 0);
        });
    }

    [Fact]
    public void Base_BSNY1000F3XNN_EdgeBanding_MatchesExpected()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase(width: "24", height: "34.5", depth: "24", shelfCount: 0, BackThickness: "3/4");
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            // Cabinet: 237.12 in
            double expectedFt = 105.96 / 12.0;
            Assert.Equal(expectedFt, cab.TotalEdgeBandingFeet, precision: 1);
        });
    }












    /// <summary>
    /// Creates a Standard base cabinet model with the given dimensions.
    /// Maple species, Wood Maple edgebanding, ¾" back, Full top, no toe kick.
    /// </summary>
    private static BaseCabinetModel MakeStandardBase(
        string width, string height, string depth,
        int shelfCount = 0, int doorCount = 0, string BackThickness = null) => new()
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
            TKDepth = "3.75",
            ShelfCount = shelfCount,
            ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
            DoorCount = doorCount,
            DrwCount = 0,
            IncDoors = false,
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
