using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Verifies that preview hide toggles are visualization-only:
/// material and edge totals must always include hidden parts.
/// </summary>
public class HiddenPartsMaterialTotalsTests
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

    [Fact]
    public void HidingAllParts_DoesNotChangeMaterialTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase();

            // Build with nothing hidden
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                leftEndHidden: false, rightEndHidden: false,
                deckHidden: false, topHidden: false);

            double fullMat = cab.TotalMaterialAreaFt2;
            double fullEB = cab.TotalEdgeBandingFeet;

            // Build with everything hidden
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                leftEndHidden: true, rightEndHidden: true,
                deckHidden: true, topHidden: true);

            double hiddenMat = cab.TotalMaterialAreaFt2;
            double hiddenEB = cab.TotalEdgeBandingFeet;

            Assert.True(fullMat > 0, "Full build should have material area > 0");
            Assert.Equal(fullMat, hiddenMat, precision: 4);
            Assert.Equal(fullEB, hiddenEB, precision: 4);
        });
    }

    //############################################################################################################

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void HidingSinglePart_DoesNotChangeMaterialTotals(
        bool leftHidden, bool rightHidden, bool deckHidden, bool topHidden)
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardBase();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                false, false, false, false);

            double baselineMat = cab.TotalMaterialAreaFt2;
            double baselineEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                leftHidden, rightHidden, deckHidden, topHidden);

            Assert.Equal(baselineMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baselineEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################

    [Fact]
    public void HidingAllParts_UpperCabinet_DoesNotChangeTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeStandardUpper();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                false, false, false, false);

            double baselineMat = cab.TotalMaterialAreaFt2;
            double baselineEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab,
                true, true, true, true);

            Assert.True(baselineMat > 0);
            Assert.Equal(baselineMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baselineEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################

    private static BaseCabinetModel MakeStandardBase() => new()
    {
        Name = "Test",
        Qty = 1,
        Style = CabinetStyles.Base.Standard,
        Width = "24",
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        BackThickness = "0.75",
        TopType = CabinetOptions.TopType.Full,
        HasTK = true,
        TKHeight = "4",
        TKDepth = "3.75",
        ShelfCount = 1,
        ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
        DrillShelfHoles = false,
        DoorCount = 1,
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
        DrwStyle = "Blum Tandem H/Equivalent Undermount",
        DrwFrontGrainDir = "Vertical",
        OpeningHeight1 = "6.6875",
        OpeningHeight2 = "6.6875",
        OpeningHeight3 = "6.6875",
        OpeningHeight4 = "6.6875",
        DrwFrontHeight1 = "7.625",
        DrwFrontHeight2 = "7.625",
        DrwFrontHeight3 = "7.625",
        DrwFrontHeight4 = "7.625"
    };

    private static UpperCabinetModel MakeStandardUpper() => new()
    {
        Name = "Test Upper",
        Qty = 1,
        Style = CabinetStyles.Upper.Standard,
        Width = "30",
        Height = "30",
        Depth = "12",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        BackThickness = "0.75",
        ShelfCount = 1,
        DrillShelfHoles = false,
        DoorCount = 2,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = false,
        IncDoorsInList = false,
        DrillHingeHoles = false,
        LeftReveal = ".0625",
        RightReveal = ".0625",
        TopReveal = ".0625",
        BottomReveal = ".0625",
        GapWidth = ".125"
    };
}