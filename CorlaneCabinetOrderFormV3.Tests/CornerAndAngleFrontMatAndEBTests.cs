using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Material area and edgebanding regression tests for Corner90 and AngleFront cabinet styles.
/// These lock down the builder output so geometry regressions (like those fixed in 3.0.1.41–3.0.1.50)
/// are caught immediately.
/// </summary>
public class CornerAndAngleFrontMatAndEBTests
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
    // Base Corner90 — material area is non-zero and stable
    //############################################################################################################

    [Fact]
    public void BaseCorner90_36x34_5x24_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseCorner90();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalMaterialAreaFt2 > 0,
                "Base Corner90 should produce material area > 0");
        });
    }

    [Fact]
    public void BaseCorner90_36x34_5x24_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseCorner90();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalEdgeBandingFeet > 0,
                "Base Corner90 should produce edgebanding > 0");
        });
    }

    [Fact]
    public void BaseCorner90_36x34_5x24_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseCorner90();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat1 = cab.TotalMaterialAreaFt2;
            double eb1 = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat2 = cab.TotalMaterialAreaFt2;
            double eb2 = cab.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    [Fact]
    public void BaseCorner90_HidingParts_DoesNotChangeTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseCorner90();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, false, false, false, false, false);
            double baseMat = cab.TotalMaterialAreaFt2;
            double baseEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, true, true, true, true, true);

            Assert.Equal(baseMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baseEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################
    // Base Corner90 — with shelves
    //############################################################################################################

    [Fact]
    public void BaseCorner90_WithShelves_HasMoreMaterialThanWithout()
    {
        RunOnSta(() =>
        {
            var noShelves = MakeBaseCorner90();
            noShelves.ShelfCount = 0;
            noShelves.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(noShelves);
            double matNoShelves = noShelves.TotalMaterialAreaFt2;

            var withShelves = MakeBaseCorner90();
            withShelves.ShelfCount = 2;
            withShelves.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(withShelves);
            double matWithShelves = withShelves.TotalMaterialAreaFt2;

            Assert.True(matWithShelves > matNoShelves,
                "Adding shelves should increase material area");
        });
    }

    //############################################################################################################
    // Base Corner90 — with doors
    //############################################################################################################

    [Fact]
    public void BaseCorner90_WithDoors_HasMoreMaterialThanWithout()
    {
        RunOnSta(() =>
        {
            var noDoors = MakeBaseCorner90();
            noDoors.DoorCount = 0;
            noDoors.IncDoors = false;
            noDoors.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(noDoors);
            double matNoDoors = noDoors.TotalMaterialAreaFt2;

            var withDoors = MakeBaseCorner90();
            withDoors.DoorCount = 2;
            withDoors.IncDoors = true;
            withDoors.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(withDoors);
            double matWithDoors = withDoors.TotalMaterialAreaFt2;

            Assert.True(matWithDoors > matNoDoors,
                "Including doors should increase material area");
        });
    }

    //############################################################################################################
    // Base AngleFront — material area is non-zero and stable
    //############################################################################################################

    [Fact]
    public void BaseAngleFront_36x34_5x24_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseAngleFront();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalMaterialAreaFt2 > 0,
                "Base AngleFront should produce material area > 0");
        });
    }

    [Fact]
    public void BaseAngleFront_36x34_5x24_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseAngleFront();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalEdgeBandingFeet > 0,
                "Base AngleFront should produce edgebanding > 0");
        });
    }

    [Fact]
    public void BaseAngleFront_36x34_5x24_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseAngleFront();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat1 = cab.TotalMaterialAreaFt2;
            double eb1 = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat2 = cab.TotalMaterialAreaFt2;
            double eb2 = cab.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    [Fact]
    public void BaseAngleFront_HidingParts_DoesNotChangeTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeBaseAngleFront();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, false, false, false, false, false);
            double baseMat = cab.TotalMaterialAreaFt2;
            double baseEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, true, true, true, true, true);

            Assert.Equal(baseMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baseEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################
    // Upper Corner90 — material area is non-zero and stable
    //############################################################################################################

    [Fact]
    public void UpperCorner90_30x30x12_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperCorner90();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalMaterialAreaFt2 > 0,
                "Upper Corner90 should produce material area > 0");
        });
    }

    [Fact]
    public void UpperCorner90_30x30x12_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperCorner90();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalEdgeBandingFeet > 0,
                "Upper Corner90 should produce edgebanding > 0");
        });
    }

    [Fact]
    public void UpperCorner90_30x30x12_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperCorner90();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat1 = cab.TotalMaterialAreaFt2;
            double eb1 = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat2 = cab.TotalMaterialAreaFt2;
            double eb2 = cab.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    [Fact]
    public void UpperCorner90_HidingParts_DoesNotChangeTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperCorner90();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, false, false, false, false, false);
            double baseMat = cab.TotalMaterialAreaFt2;
            double baseEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, true, true, true, true, true);

            Assert.Equal(baseMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baseEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################
    // Upper AngleFront — material area is non-zero and stable
    //############################################################################################################

    [Fact]
    public void UpperAngleFront_30x30x12_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperAngleFront();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalMaterialAreaFt2 > 0,
                "Upper AngleFront should produce material area > 0");
        });
    }

    [Fact]
    public void UpperAngleFront_30x30x12_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperAngleFront();
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);

            Assert.True(cab.TotalEdgeBandingFeet > 0,
                "Upper AngleFront should produce edgebanding > 0");
        });
    }

    [Fact]
    public void UpperAngleFront_30x30x12_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperAngleFront();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat1 = cab.TotalMaterialAreaFt2;
            double eb1 = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            double mat2 = cab.TotalMaterialAreaFt2;
            double eb2 = cab.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    [Fact]
    public void UpperAngleFront_HidingParts_DoesNotChangeTotals()
    {
        RunOnSta(() =>
        {
            var cab = MakeUpperAngleFront();

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, false, false, false, false, false);
            double baseMat = cab.TotalMaterialAreaFt2;
            double baseEB = cab.TotalEdgeBandingFeet;

            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForPreview(cab, true, true, true, true, true);

            Assert.Equal(baseMat, cab.TotalMaterialAreaFt2, precision: 4);
            Assert.Equal(baseEB, cab.TotalEdgeBandingFeet, precision: 4);
        });
    }

    //############################################################################################################
    // Base Corner90 — back thickness is always 3/4 regardless of model value
    //############################################################################################################

    [Fact]
    public void BaseCorner90_AlwaysUses34Back_RegardlessOfModelValue()
    {
        RunOnSta(() =>
        {
            var cab34 = MakeBaseCorner90();
            cab34.BackThickness = "0.75";
            cab34.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab34);
            double mat34 = cab34.TotalMaterialAreaFt2;

            // Even if someone sets 1/4, the builder should force 3/4 for corner cabs
            var cab14 = MakeBaseCorner90();
            cab14.BackThickness = "0.25";
            cab14.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab14);
            double mat14 = cab14.TotalMaterialAreaFt2;

            // Material totals should be identical — the builder hardcodes 3/4 back for corners
            Assert.Equal(mat34, mat14, precision: 4);
        });
    }

    //############################################################################################################
    // Helpers
    //############################################################################################################

    private static BaseCabinetModel MakeBaseCorner90() => new()
    {
        Name = "TestCorner90",
        Qty = 1,
        Style = CabinetStyles.Base.Corner90,
        Width = "36",
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        BackThickness = "0.75",
        TopType = CabinetOptions.TopType.Full,
        HasTK = true,
        TKHeight = "4",
        TKDepth = "3.75",
        ShelfCount = 1,
        ShelfDepth = CabinetOptions.ShelfDepth.FullDepth,
        DrillShelfHoles = false,
        DoorCount = 2,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = true,
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
        IncRollouts = false,
        IncRolloutsInList = false,
        RolloutCount = 0,
        RolloutStyle = "Blum Tandem H/Equivalent Undermount",
        DrillSlideHolesForRollouts = false,
        SinkCabinet = false,
        TrashDrawer = false,
        LeftBackWidth = "36",
        RightBackWidth = "36",
        LeftFrontWidth = "12",
        RightFrontWidth = "12",
        LeftDepth = "24",
        RightDepth = "24",
        FrontWidth = "",
    };

    private static BaseCabinetModel MakeBaseAngleFront() => new()
    {
        Name = "TestAngleFront",
        Qty = 1,
        Style = CabinetStyles.Base.AngleFront,
        Width = "36",
        Height = "34.5",
        Depth = "24",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
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
        IncDoors = true,
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
        IncRollouts = false,
        IncRolloutsInList = false,
        RolloutCount = 0,
        RolloutStyle = "Blum Tandem H/Equivalent Undermount",
        DrillSlideHolesForRollouts = false,
        SinkCabinet = false,
        TrashDrawer = false,
        LeftBackWidth = "24",
        RightBackWidth = "24",
        LeftFrontWidth = "",
        RightFrontWidth = "",
        LeftDepth = "24",
        RightDepth = "24",
        FrontWidth = "",
    };

    private static UpperCabinetModel MakeUpperCorner90() => new()
    {
        Name = "TestUpperCorner90",
        Qty = 1,
        Style = CabinetStyles.Upper.Corner90,
        Width = "30",
        Height = "30",
        Depth = "12",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        BackThickness = "0.75",
        ShelfCount = 1,
        DrillShelfHoles = false,
        DoorCount = 2,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = true,
        IncDoorsInList = false,
        DrillHingeHoles = false,
        LeftReveal = "1/8",
        RightReveal = "1/8",
        TopReveal = "1/8",
        BottomReveal = "1/8",
        GapWidth = "1/8",
        LeftBackWidth = "30",
        RightBackWidth = "30",
        LeftFrontWidth = "12",
        RightFrontWidth = "12",
        LeftDepth = "12",
        RightDepth = "12",
    };

    private static UpperCabinetModel MakeUpperAngleFront() => new()
    {
        Name = "TestUpperAngleFront",
        Qty = 1,
        Style = CabinetStyles.Upper.AngleFront,
        Width = "30",
        Height = "30",
        Depth = "12",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        BackThickness = "0.75",
        ShelfCount = 1,
        DrillShelfHoles = false,
        DoorCount = 1,
        DoorSpecies = "Maple",
        CustomDoorSpecies = "",
        DoorGrainDir = "Vertical",
        IncDoors = true,
        IncDoorsInList = false,
        DrillHingeHoles = false,
        LeftReveal = "1/8",
        RightReveal = "1/8",
        TopReveal = "1/8",
        BottomReveal = "1/8",
        GapWidth = "1/8",
        LeftBackWidth = "18",
        RightBackWidth = "18",
        LeftFrontWidth = "",
        RightFrontWidth = "",
        LeftDepth = "12",
        RightDepth = "12",
    };
}