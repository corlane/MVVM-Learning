using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.ViewModels;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Guards against OnXChanged handlers overwriting model values during mapping.
/// If a new handler is added without an _isMapping guard and it modifies a data
/// property, one of these tests will fail.
/// </summary>
public class BaseCabinetMappingFidelityTests
{
    /// <summary>
    /// Creates a model whose properties are deliberately different from every
    /// default the app would apply. Any handler that overwrites a value with
    /// a default during mapping will cause an assertion failure.
    /// </summary>
    private static BaseCabinetModel MakeNonDefaultModel() => new()
    {
        Style = CabinetStyles.Base.Standard,
        Width = "21",
        Height = "30",
        Depth = "22",            // > 10.625 — handlers would set defaults if guard is missing
        Species = "Cherry Ply",
        CustomSpecies = "MyWood",
        EBSpecies = "Wood Cherry",
        CustomEBSpecies = "MyEB",
        Name = "Test Cab",
        Qty = 3,
        Notes = "some notes",
        HasTK = true,
        TKHeight = "3.5",
        TKDepth = "3",
        BackThickness = "0.25",
        TopType = "Full",            // differs from default "Stretcher"
        ShelfCount = 2,
        ShelfDepth = "Full Depth",   // differs from default "Half Depth"
        DrillShelfHoles = false,     // differs from default true
        DoorSpecies = "Cherry Ply",
        CustomDoorSpecies = "MyDoor",
        DoorCount = 2,
        DoorGrainDir = "Horizontal",
        IncDoorsInList = false,
        IncDoors = false,
        DrillHingeHoles = false,
        DrwCount = 1,
        DrwStyle = "Grass Zargen",
        DrwFrontGrainDir = "Vertical",
        IncDrwFrontsInList = false,
        IncDrwFronts = false,
        IncDrwBoxesInList = false,
        IncDrwBoxes = false,         // differs from default true — the exact property clobbered by the Depth bug
        DrillSlideHoles = false,
        OpeningHeight1 = "20",
        OpeningHeight2 = "0",
        OpeningHeight3 = "0",
        OpeningHeight4 = "0",
        DrwFrontHeight1 = "6.5",
        DrwFrontHeight2 = "0",
        DrwFrontHeight3 = "0",
        DrwFrontHeight4 = "0",
        IncDrwFront1 = false,
        IncDrwFront2 = false,
        IncDrwFront3 = false,
        IncDrwFront4 = false,
        IncDrwFrontInList1 = false,
        IncDrwFrontInList2 = false,
        IncDrwFrontInList3 = false,
        IncDrwFrontInList4 = false,
        IncDrwBoxOpening1 = false,
        IncDrwBoxOpening2 = false,
        IncDrwBoxOpening3 = false,
        IncDrwBoxOpening4 = false,
        IncDrwBoxInListOpening1 = false,
        IncDrwBoxInListOpening2 = false,
        IncDrwBoxInListOpening3 = false,
        IncDrwBoxInListOpening4 = false,
        DrillSlideHolesOpening1 = false,
        DrillSlideHolesOpening2 = false,
        DrillSlideHolesOpening3 = false,
        DrillSlideHolesOpening4 = false,
        LeftReveal = "0.125",
        RightReveal = "0.125",
        TopReveal = "0.5",
        BottomReveal = "0.125",
        GapWidth = "0.25",
        IncRollouts = false,
        IncRolloutsInList = false,
        RolloutCount = 0,
        RolloutStyle = "Standard",
        DrillSlideHolesForRollouts = false,
        SinkCabinet = false,
        TrashDrawer = false,
        IncTrashDrwBox = false,
        EqualizeAllDrwFronts = false,
        EqualizeBottomDrwFronts = false,
        LeftBackWidth = "36",
        RightBackWidth = "36",
        LeftFrontWidth = "12",
        RightFrontWidth = "12",
        LeftDepth = "24",
        RightDepth = "24",
    };

    [Fact]
    public void MapModelToViewModel_PreservesAllNonDefaultValues()
    {
        // Arrange — parameterless ctor gives us a VM without DI, which is fine
        // because _isMapping suppresses all handlers during mapping.
        var vm = new BaseCabinetViewModel();
        var model = MakeNonDefaultModel();

        // Act
        vm.MapModelToViewModel(model, "Decimal");

        // Assert — every data property must match the model, NOT the app defaults.
        Assert.Equal("21", vm.Width);
        Assert.Equal("30", vm.Height);
        Assert.Equal("22", vm.Depth);
        Assert.Equal("Cherry Ply", vm.Species);
        Assert.Equal("MyWood", vm.CustomSpecies);
        Assert.Equal("Wood Cherry", vm.EBSpecies);
        Assert.Equal("MyEB", vm.CustomEBSpecies);
        Assert.Equal("Test Cab", vm.Name);
        Assert.Equal(3, vm.Qty);
        Assert.Equal("some notes", vm.Notes);
        Assert.True(vm.HasTK);
        Assert.Equal("3.5", vm.TKHeight);
        Assert.Equal("3", vm.TKDepth);
        Assert.Equal("0.25", vm.BackThickness);

        // These three are the exact properties the Depth-handler bug was clobbering:
        Assert.Equal("Full", vm.TopType);
        Assert.Equal("Full Depth", vm.ShelfDepth);

        Assert.Equal(2, vm.ShelfCount);
        Assert.False(vm.DrillShelfHoles);
        Assert.Equal("Cherry Ply", vm.DoorSpecies);
        Assert.Equal(2, vm.DoorCount);
        Assert.Equal("Horizontal", vm.DoorGrainDir);
        Assert.False(vm.IncDoorsInList);
        Assert.False(vm.IncDoors);
        Assert.False(vm.DrillHingeHoles);
        Assert.Equal(1, vm.DrwCount);
        Assert.Equal("Grass Zargen", vm.DrwStyle);
        Assert.Equal("20", vm.OpeningHeight1);
        Assert.Equal("6.5", vm.DrwFrontHeight1);
        Assert.False(vm.IncDrwBoxOpening1);
        Assert.False(vm.DrillSlideHolesOpening1);
        Assert.False(vm.SinkCabinet);
        Assert.False(vm.TrashDrawer);
        Assert.False(vm.IncTrashDrwBox);
        Assert.False(vm.EqualizeAllDrwFronts);
        Assert.False(vm.EqualizeBottomDrwFronts);
        Assert.False(vm.IncRollouts);
        Assert.Equal(0, vm.RolloutCount);
    }

    [Fact]
    public void MapModelToViewModel_SecondCabinet_FullyOverwritesFirst()
    {
        // Simulates clicking cabinet A then cabinet B — B's values must win.
        var vm = new BaseCabinetViewModel();

        var cabinetA = MakeNonDefaultModel();
        cabinetA.TopType = "Stretcher";
        cabinetA.ShelfDepth = "Half Depth";
        cabinetA.IncDrwBoxes = true;
        cabinetA.Depth = "24";

        var cabinetB = MakeNonDefaultModel();
        cabinetB.TopType = "Full";
        cabinetB.ShelfDepth = "Full Depth";
        cabinetB.IncDrwBoxes = false;
        cabinetB.Depth = "18";

        // Act — map A, then immediately map B (same sequence as clicking two cabinets)
        vm.MapModelToViewModel(cabinetA, "Decimal");
        vm.MapModelToViewModel(cabinetB, "Decimal");

        // Assert — all values must come from cabinet B
        Assert.Equal("18", vm.Depth);
        Assert.Equal("Full", vm.TopType);
        Assert.Equal("Full Depth", vm.ShelfDepth);
    }

    [Fact]
    public void MapModelToViewModel_ShallowDepth_DoesNotRevertToDefaults()
    {
        // A cabinet saved with depth < 10.625 should keep its stored values,
        // not have them overwritten by OnDepthChanged's default logic.
        var vm = new BaseCabinetViewModel();

        var model = MakeNonDefaultModel();
        model.Depth = "8";              // shallow — triggers OnDepthChanged path
        model.IncDrwBoxes = false;      // explicitly saved as false
        model.TopType = "Full";         // explicitly saved as Full
        model.ShelfDepth = "Full Depth";

        vm.MapModelToViewModel(model, "Decimal");

        Assert.Equal("8", vm.Depth);
        Assert.Equal("Full", vm.TopType);
        Assert.Equal("Full Depth", vm.ShelfDepth);
    }
}