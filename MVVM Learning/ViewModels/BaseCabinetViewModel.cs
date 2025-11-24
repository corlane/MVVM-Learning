using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using System.Collections.ObjectModel;

namespace MVVM_Learning.ViewModels;

public partial class BaseCabinetViewModel : ObservableObject
{
    public BaseCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;

    public BaseCabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
    }

    // Base cabinet type strings
    public static string Type1 => "Standard";
    public static string Type2 => "Drawer";
    public static string Type3 => "90° Corner";
    public static string Type4 => "45° Corner";

    [ObservableProperty] public partial string Type { get; set; } = "";
    [ObservableProperty] public partial string Width { get; set; } = "";
    [ObservableProperty] public partial string Height { get; set; } = "";
    [ObservableProperty] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; } = "";

    // Type-specific properties for BaseCabinetModel
    [ObservableProperty] public partial string LeftBackWidth { get; set; } = "";
    [ObservableProperty] public partial string RightBackWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string RightFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftDepth { get; set; } = "";
    [ObservableProperty] public partial string RightDepth { get; set; } = "";
    [ObservableProperty] public partial bool HasTK { get; set; }
    [ObservableProperty] public partial string TKHeight { get; set; } = "";
    [ObservableProperty] public partial string TKDepth { get; set; } = "";
    [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty] public partial string BackThickness { get; set; } = "";
    [ObservableProperty] public partial string TopType { get; set; } = "";
    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial string ShelfDepth { get; set; } = "";
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }
    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial string DrwFrontGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDrwFrontsInList { get; set; }
    [ObservableProperty] public partial bool IncDrwFronts { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxesInList { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxes { get; set; }
    [ObservableProperty] public partial bool DrillDrwSlideMountingHoles { get; set; }
    [ObservableProperty] public partial int DrwCount { get; set; }
    [ObservableProperty] public partial string DrwStyle { get; set; } = "";
    [ObservableProperty] public partial string OpeningHeight1 { get; set; } = "";
    [ObservableProperty] public partial string OpeningHeight2 { get; set; } = "";
    [ObservableProperty] public partial string OpeningHeight3 { get; set; } = "";
    [ObservableProperty] public partial string OpeningHeight4 { get; set; } = "";
    [ObservableProperty] public partial bool IncDrwBoxOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening4 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening1 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening2 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening3 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening4 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening4 { get; set; } 
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight2 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = "";
    [ObservableProperty] public partial bool IncDrwFront1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront4 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInSizeList1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInSizeList2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInSizeList3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInSizeList4 { get; set; }
    [ObservableProperty] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty] public partial string RightReveal { get; set; } = "";
    [ObservableProperty] public partial string TopReveal { get; set; } = "";
    [ObservableProperty] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty] public partial string GapWidth { get; set; } = "";


    // Combobox options
    public List<int> ComboShelfCount { get; } = [0, 1, 2, 3, 4, 5];
    public static List<string> TypeList => [Type1, Type2, Type3, Type4];
    public List<string> ListDrawerStyle { get; } =
    [
        "Blum Tandem H/Equivalent Undermount",
        "Accuride/Equivalent Sidemount"
    ];
    public List<int> ListDoorCount { get; } =
    [
        0,
        1,
        2
    ];
    public List<string> ListGrainDirection { get; } =
    [
        "Horizontal",
        "Vertical"
    ];
    public List<string> ListCabSpecies { get; } =
[
    "Prefinished Ply",
        "Maple Ply",
        "Red Oak Ply",
        "White Oak Ply",
        "Cherry Ply",
        "Alder Ply",
        "Mahogany Ply",
        "Walnut Ply",
        "Hickory Ply",
        "MDF",
        "Melamine",
        "Custom"
];
    public List<string> ListEBSpecies { get; } =
    [
        "None",
        "PVC White",
        "PVC Black",
        "PVC Hardrock Maple",
        "PVC Paint Grade",
        "Wood Prefinished Maple",
        "Wood Maple",
        "Wood Red Oak",
        "Wood White Oak",
        "Wood Walnut",
        "Wood Cherry",
        "Wood Alder",
        "Wood Hickory",
        "Wood Mahogany",
        "Custom"
    ];
    public List<string> ListShelfDepth { get; } =
        [
            "Half Depth",
            "Full Depth"
        ];
    public List<string> ListBackThickness { get; } =
        [
            "0.25",
            "0.75"
        ];
    public List<string> ListTopType { get; } =
        [
            "Stretcher",
            "Full"
        ];
    public ObservableCollection<int> ListDrwCount { get; set; } = [];

    // Visibility properties
    [ObservableProperty] public partial bool GroupBaseDoorsVisibility { get; set; }
    [ObservableProperty] public partial bool GroupDrawersVisibility { get; set; }
    [ObservableProperty] public partial bool GroupDoorDrwFrontsVisibility { get; set; }
    [ObservableProperty] public partial bool StdOrDrwBaseVisibility { get; set; }
    [ObservableProperty] public partial bool BaseCorner90Visibility { get; set; }
    [ObservableProperty] public partial bool BaseCorner45Visibility { get; set; }
    [ObservableProperty] public partial bool GroupCabinetTopTypeVisibility { get; set; }
    [ObservableProperty] public partial bool GroupDrawerFrontHeightsVisibility { get; set; }
    [ObservableProperty] public partial bool GroupShelvesBaseVisibility { get; set; }
    [ObservableProperty] public partial bool DrwFrontHeight1Enabled { get; set; }
    [ObservableProperty] public partial bool DrwFrontHeight2Enabled { get; set; }
    [ObservableProperty] public partial bool DrwFrontHeight3Enabled { get; set; }
    [ObservableProperty] public partial bool BaseShowRevealSettings { get; set; }
    [ObservableProperty] public partial bool DrwFront1Visible { get; set; }
    [ObservableProperty] public partial bool DrwFront2Visible { get; set; }
    [ObservableProperty] public partial bool DrwFront3Visible { get; set; }
    [ObservableProperty] public partial bool DrwFront4Visible { get; set; }
    [ObservableProperty] public partial bool DrwFront1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront4PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening1Visible { get; set; }
    [ObservableProperty] public partial bool Opening2Visible { get; set; }
    [ObservableProperty] public partial bool Opening3Visible { get; set; }
    [ObservableProperty] public partial bool Opening4Visible { get; set; }
    [ObservableProperty] public partial bool Opening1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening4PropertiesVisible { get; set; } = false;






    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new BaseCabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,

            TKHeight = TKHeight,  // Subtype-specific
            Type = Type,
            LeftBackWidth = LeftBackWidth,
            RightBackWidth = RightBackWidth,
            LeftFrontWidth = LeftFrontWidth,
            RightFrontWidth = RightFrontWidth,
            LeftDepth = LeftDepth,
            RightDepth = RightDepth,
            HasTK = HasTK,
            TKDepth = TKDepth,
            DoorSpecies = DoorSpecies,
            BackThickness = BackThickness,
            TopType = TopType,
            ShelfCount = ShelfCount,
            ShelfDepth = ShelfDepth,
            DrillShelfHoles = DrillShelfHoles,
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            IncDoorsInList = IncDoorsInList,
            IncDoors = IncDoors,
            DrillHingeHoles = DrillHingeHoles,
            DrwFrontGrainDir = DrwFrontGrainDir,
            IncDrwFrontsInList = IncDrwFrontsInList,
            IncDrwFronts = IncDrwFronts,
            IncDrwBoxesInList = IncDrwBoxesInList,
            IncDrwBoxes = IncDrwBoxes,
            DrillDrwSlideMountingHoles = DrillDrwSlideMountingHoles,
            DrwCount = DrwCount,
            DrwStyle = DrwStyle,
            OpeningHeight1 = OpeningHeight1,
            OpeningHeight2 = OpeningHeight2,
            OpeningHeight3 = OpeningHeight3,
            OpeningHeight4 = OpeningHeight4,
            IncDrwBoxOpening1 = IncDrwBoxOpening1,
            IncDrwBoxOpening2 = IncDrwBoxOpening2,
            IncDrwBoxOpening3 = IncDrwBoxOpening3,
            IncDrwBoxOpening4 = IncDrwBoxOpening4,
            DrillSlideHolesOpening1 = DrillSlideHolesOpening1,
            DrillSlideHolesOpening2 = DrillSlideHolesOpening2,
            DrillSlideHolesOpening3 = DrillSlideHolesOpening3,
            DrillSlideHolesOpening4 = DrillSlideHolesOpening4,
            IncDrwBoxInListOpening1 = IncDrwBoxInListOpening1,
            IncDrwBoxInListOpening2 = IncDrwBoxInListOpening2,
            IncDrwBoxInListOpening3 = IncDrwBoxInListOpening3,
            IncDrwBoxInListOpening4 = IncDrwBoxInListOpening4,
            DrwFrontHeight1 = DrwFrontHeight1,
            DrwFrontHeight2 = DrwFrontHeight2,
            DrwFrontHeight3 = DrwFrontHeight3,
            DrwFrontHeight4 = DrwFrontHeight4,
            IncDrwFront1 = IncDrwFront1,
            IncDrwFront2 = IncDrwFront2,
            IncDrwFront3 = IncDrwFront3,
            IncDrwFront4 = IncDrwFront4,
            IncDrwFrontInSizeList1 = IncDrwFrontInSizeList1,
            IncDrwFrontInSizeList2 = IncDrwFrontInSizeList2,
            IncDrwFrontInSizeList3 = IncDrwFrontInSizeList3,
            IncDrwFrontInSizeList4 = IncDrwFrontInSizeList4,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type

    }
}