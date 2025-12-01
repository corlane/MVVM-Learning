using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class DefaultSettingsViewModel : ObservableObject
{

    public DefaultSettingsViewModel()
    {
        // empty constructor for design-time support
    }

    public DefaultSettingsViewModel(DefaultSettingsService defaults)
    {
        _defaults = defaults;
    }


    private readonly DefaultSettingsService? _defaults;


    // Mirror every default as a bindable property

    // Dimension Format
    public string DefaultDimensionFormat { get => _defaults!.DefaultDimensionFormat; set => _defaults!.DefaultDimensionFormat = value; }

    // Species
    public string DefaultSpecies { get => _defaults!.DefaultSpecies; set => _defaults!.DefaultSpecies = value; }
    public string DefaultEBSpecies { get => _defaults!.DefaultEBSpecies; set => _defaults!.DefaultEBSpecies = value; }

    //Top
    public string DefaultTopType { get => _defaults!.DefaultTopType; set => _defaults!.DefaultTopType = value; }

    // Back
    public string DefaultBaseBackThickness { get => _defaults!.DefaultBaseBackThickness; set => _defaults!.DefaultBaseBackThickness = value; }
    public string DefaultUpperBackThickness { get => _defaults!.DefaultUpperBackThickness; set => _defaults!.DefaultUpperBackThickness = value; }

    //Toekick
    public bool DefaultHasTK { get => _defaults!.DefaultHasTK; set => _defaults!.DefaultHasTK = value; }
    public string DefaultTKHeight { get => _defaults!.DefaultTKHeight; set => _defaults!.DefaultTKHeight = value; }
    public string DefaultTKDepth { get => _defaults!.DefaultTKDepth; set => _defaults!.DefaultTKDepth = value; }

    // Shelves
    public int DefaultShelfCount { get => _defaults!.DefaultShelfCount; set => _defaults!.DefaultShelfCount = value; }
    public string DefaultShelfDepth { get => _defaults!.DefaultShelfDepth; set => _defaults!.DefaultShelfDepth = value; }
    public bool DefaultDrillShelfHoles { get => _defaults!.DefaultDrillShelfHoles; set => _defaults!.DefaultDrillShelfHoles = value; }

    // Openings
    public string DefaultOpeningHeight1 { get => _defaults!.DefaultOpeningHeight1; set => _defaults!.DefaultOpeningHeight1 = value; }
    public string DefaultOpeningHeight2 { get => _defaults!.DefaultOpeningHeight2; set => _defaults!.DefaultOpeningHeight2 = value; }
    public string DefaultOpeningHeight3 { get => _defaults!.DefaultOpeningHeight3; set => _defaults!.DefaultOpeningHeight3 = value; }


    // Doors
    public string DefaultDoorDrwSpecies { get => _defaults!.DefaultDoorDrwSpecies; set => _defaults!.DefaultDoorDrwSpecies = value; }
    public int DefaultDoorCount { get => _defaults!.DefaultDoorCount; set => _defaults!.DefaultDoorCount = value; }
    public bool DefaultDrillHingeHoles { get => _defaults!.DefaultDrillHingeHoles; set => _defaults!.DefaultDrillHingeHoles = value; }
    public string DefaultDoorGrainDir { get => _defaults!.DefaultDoorGrainDir; set => _defaults!.DefaultDoorGrainDir = value; }
    public bool DefaultIncDoorsInList { get => _defaults!.DefaultIncDoorsInList; set => _defaults!.DefaultIncDoorsInList = value; }
    public bool DefaultIncDoors { get => _defaults!.DefaultIncDoors; set => _defaults!.DefaultIncDoors = value; }

    // Drawers
    public int DefaultStdDrawerCount { get => _defaults!.DefaultStdDrawerCount; set => _defaults!.DefaultStdDrawerCount = value; }
    public int DefaultDrawerStackDrawerCount { get => _defaults!.DefaultDrawerStackDrawerCount; set => _defaults!.DefaultDrawerStackDrawerCount = value; }
    public string DefaultDrwStyle { get => _defaults!.DefaultDrwStyle; set => _defaults!.DefaultDrwStyle = value; }
    public string DefaultDrwGrainDir { get => _defaults!.DefaultDrwGrainDir; set => _defaults!.DefaultDrwGrainDir = value; }
    public bool DefaultIncDrwFrontsInList { get => _defaults!.DefaultIncDrwFrontsInList; set => _defaults!.DefaultIncDrwFrontsInList = value; }
    public bool DefaultIncDrwFronts { get => _defaults!.DefaultIncDrwFronts; set => _defaults!.DefaultIncDrwFronts = value; }
    public bool DefaultIncDrwBoxesInList { get => _defaults!.DefaultIncDrwBoxesInList; set => _defaults!.DefaultIncDrwBoxesInList = value; }
    public bool DefaultIncDrwBoxes { get => _defaults!.DefaultIncDrwBoxes; set => _defaults!.DefaultIncDrwBoxes = value; }
    public bool DefaultDrillSlideHoles { get => _defaults!.DefaultDrillSlideHoles; set => _defaults!.DefaultDrillSlideHoles = value; }
    public string DefaultDrwFrontHeight1 { get => _defaults!.DefaultDrwFrontHeight1; set => _defaults!.DefaultDrwFrontHeight1 = value; }
    public string DefaultDrwFrontHeight2 { get => _defaults!.DefaultDrwFrontHeight2; set => _defaults!.DefaultDrwFrontHeight2 = value; }
    public string DefaultDrwFrontHeight3 { get => _defaults!.DefaultDrwFrontHeight3; set => _defaults!.DefaultDrwFrontHeight3 = value; }

    // Reveals and Gaps
    public string DefaulBasetLeftReveal { get => _defaults!.DefaultBaseLeftReveal; set => _defaults!.DefaultBaseLeftReveal = value; }
    public string DefaultBaseRightReveal { get => _defaults!.DefaultBaseRightReveal; set => _defaults!.DefaultBaseRightReveal = value; }
    public string DefaultBaseTopReveal { get => _defaults!.DefaultBaseTopReveal; set => _defaults!.DefaultBaseTopReveal = value; }
    public string DefaultBaseBottomReveal { get => _defaults!.DefaultBaseBottomReveal; set => _defaults!.DefaultBaseBottomReveal = value; }

    public string DefaultUpperLeftReveal { get => _defaults!.DefaultUpperLeftReveal; set => _defaults!.DefaultUpperLeftReveal = value; }
    public string DefaultUpperRightReveal { get => _defaults!.DefaultUpperRightReveal; set => _defaults!.DefaultUpperRightReveal = value; }
    public string DefaultUpperTopReveal { get => _defaults!.DefaultUpperTopReveal; set => _defaults!.DefaultUpperTopReveal = value; }
    public string DefaultUpperBottomReveal { get => _defaults!.DefaultUpperBottomReveal; set => _defaults!.DefaultUpperBottomReveal = value; }

    public string DefaultGapWidth { get => _defaults!.DefaultGapWidth; set => _defaults!.DefaultGapWidth = value; }

    // Add more mirrored properties here as you create new default properties

    // Combobox Lists
    public List<string> ListDimensionFormat { get; } =
    [
        "Fraction",
        "Decimal"
    ];      
    public List<int> ListShelfCount { get; } = [0, 1, 2, 3, 4, 5];
    public List<int> ListStdDrwCount { get; } = [0, 1];
    public List<int> ListDrwStackDrwCount { get; } = [1, 2, 3, 4];
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


    [RelayCommand]
    private void LoadDefaults()
    {
        DefaultDimensionFormat = _defaults!.DefaultDimensionFormat;
        DefaultSpecies = _defaults!.DefaultSpecies;
        DefaultEBSpecies = _defaults.DefaultEBSpecies;
        DefaultTopType = _defaults!.DefaultTopType;
        DefaultBaseBackThickness = _defaults!.DefaultBaseBackThickness;
        DefaultUpperBackThickness = _defaults!.DefaultUpperBackThickness;
        DefaultHasTK = _defaults!.DefaultHasTK;
        DefaultTKHeight = _defaults!.DefaultTKHeight;
        DefaultTKDepth = _defaults!.DefaultTKDepth;
        DefaultShelfCount = _defaults!.DefaultShelfCount;
        DefaultShelfDepth = _defaults!.DefaultShelfDepth;
        DefaultDrillShelfHoles = _defaults!.DefaultDrillShelfHoles;
        DefaultOpeningHeight1 = _defaults!.DefaultOpeningHeight1;
        DefaultOpeningHeight2 = _defaults!.DefaultOpeningHeight2;
        DefaultOpeningHeight3 = _defaults!.DefaultOpeningHeight3;
        DefaultDoorDrwSpecies = _defaults!.DefaultDoorDrwSpecies;
        DefaultDoorCount = _defaults!.DefaultDoorCount;
        DefaultDrillHingeHoles = _defaults.DefaultDrillHingeHoles;
        DefaultDoorGrainDir = _defaults!.DefaultDoorGrainDir;
        DefaultIncDoorsInList = _defaults!.DefaultIncDoorsInList;
        DefaultIncDoors = _defaults!.DefaultIncDoors;

        // etc.
    }


    // Command to save defaults
    [RelayCommand]
    private async Task SaveDefaults()
    {
        await _defaults!.SaveAsync();
        // Optional: show toast/message
    }

}