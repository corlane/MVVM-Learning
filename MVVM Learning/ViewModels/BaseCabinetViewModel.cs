using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Converters;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using MVVM_Learning.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MVVM_Learning.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator, INotifyPropertyChanged
{
    public BaseCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
 
    public BaseCabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;

        Type = Type1; // Default type

        ValidateAllProperties();
    }

    // Base cabinet type strings
    public static string Type1 => "Standard";
    public static string Type2 => "Drawer";
    public static string Type3 => "90° Corner";
    public static string Type4 => "45° Corner";

    // Common properties from CabinetModel
    [ObservableProperty] public partial string Type { get; set; } = ""; partial void OnTypeChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            // Update visibility based on selected type
            StdOrDrwBaseVisibility = (newValue == Type1 || newValue == Type2);
            Corner90Visibility = (newValue == Type3);
            Corner45Visibility = (newValue == Type4);
            GroupShelvesVisibility = (newValue == Type1 || newValue == Type3 || newValue == Type4);
            GroupDrawersVisibility = (newValue == Type1 || newValue == Type2);
            GroupCabinetTopTypeVisibility = (newValue == Type1 || newValue == Type2);
            GroupDrawerFrontHeightsVisibility = (newValue == Type1 || newValue == Type2);
            GroupDoorsVisibility = (newValue == Type1 || newValue == Type3 || newValue == Type4);

            if (newValue == Type2)
            {
                // Drawer cabinet selected
                ListDrwCount = [1,2,3,4];
            }
            else if (newValue == Type1)
            {
                // Standard or corner cabinet selected
                ListDrwCount = [0,1];
            }
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 120)] public partial string Height { get; set; } = ""; partial void OnHeightChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; } = "";

    // Type-specific properties for BaseCabinetModel

    // Corner Cab specific properties
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftBackWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightBackWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftFrontWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightFrontWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftDepth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightDepth { get; set; } = "";

    [ObservableProperty] public partial string BackThickness { get; set; } = "";
    [ObservableProperty] public partial string TopType { get; set; } = "";

    // Toekick-specific properties
    [ObservableProperty] public partial bool HasTK { get; set; } partial void OnHasTKChanged(bool oldValue, bool newValue)
    {
        ResizeOpeningHeights();
    }
    [ObservableProperty] public partial string TKHeight { get; set; } = ""; partial void OnTKHeightChanged(string oldValue, string newValue)
    {
        ResizeOpeningHeights();
    }
    [ObservableProperty] public partial string TKDepth { get; set; } = "";

    // Shelf-specific properties
    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial string ShelfDepth { get; set; } = "";
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }

    // Door-specific properties
    [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }

    // Drawer-specific properties
    [ObservableProperty] public partial int DrwCount { get; set; } partial void OnDrwCountChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            // Update visibility of drawer front height properties based on DrwCount
            DrwFrontHeight1Enabled = newValue >= 1;
            DrwFrontHeight2Enabled = newValue >= 2;
            DrwFrontHeight3Enabled = newValue >= 3;
            DrwFront1Visible = newValue >= 1;
            DrwFront2Visible = newValue >= 2;
            DrwFront3Visible = newValue >= 3;
            DrwFront4Visible = newValue == 4;
            Opening1Visible = newValue >= 1;
            Opening2Visible = newValue >= 2;
            Opening3Visible = newValue >= 3;
            Opening4Visible = newValue == 4;
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwStyle { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDrwFrontsInList { get; set; } partial void OnIncDrwFrontsInListChanged(bool oldValue, bool newValue)
    {
        if (newValue != oldValue)
        {
            if (IncDrwFrontsInList)
            {
                IncDrwFrontInList1 = true;
                IncDrwFrontInList2 = true;
                IncDrwFrontInList3 = true;
                IncDrwFrontInList4 = true;
            }
            else
            {
                IncDrwFrontInList1 = false;
                IncDrwFrontInList2 = false;
                IncDrwFrontInList3 = false;
                IncDrwFrontInList4 = false;
            }
        }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList1 { get; set; } partial void OnIncDrwFrontInList1Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList2 { get; set; } partial void OnIncDrwFrontInList2Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList3 { get; set; } partial void OnIncDrwFrontInList3Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList4 { get; set; } partial void OnIncDrwFrontInList4Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFronts { get; set; } partial void OnIncDrwFrontsChanged(bool oldValue, bool newValue)
    {
        if (newValue != oldValue)
        {
            if (IncDrwFronts)
            {
                IncDrwFront1 = true;
                IncDrwFront2 = true;
                IncDrwFront3 = true;
                IncDrwFront4 = true;
            }
            else
            {
                IncDrwFront1 = false;
                IncDrwFront2 = false;
                IncDrwFront3 = false;
                IncDrwFront4 = false;
            }
        }
    }
    [ObservableProperty] public partial bool IncDrwFront1 { get; set; } partial void OnIncDrwFront1Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront2 { get; set; } partial void OnIncDrwFront2Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront3 { get; set; } partial void OnIncDrwFront3Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront4 { get; set; } partial void OnIncDrwFront4Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxesInList { get; set; } partial void OnIncDrwBoxesInListChanged(bool oldValue, bool newValue)
    {
        if (newValue != oldValue)
        {
            if (IncDrwBoxesInList)
            {
                IncDrwBoxInListOpening1 = true;
                IncDrwBoxInListOpening2 = true;
                IncDrwBoxInListOpening3 = true;
                IncDrwBoxInListOpening4 = true;
            }
            else
            {
                IncDrwBoxInListOpening1 = false;
                IncDrwBoxInListOpening2 = false;
                IncDrwBoxInListOpening3 = false;
                IncDrwBoxInListOpening4 = false;
            }
        }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening1 { get; set; } partial void OnIncDrwBoxInListOpening1Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening2 { get; set; } partial void OnIncDrwBoxInListOpening2Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening3 { get; set; } partial void OnIncDrwBoxInListOpening3Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening4 { get; set; } partial void OnIncDrwBoxInListOpening4Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxes { get; set; } partial void OnIncDrwBoxesChanged(bool oldValue, bool newValue)
    {
        if (newValue != oldValue)
        {
            if (IncDrwBoxes)
            {
                IncDrwBoxOpening1 = true;
                IncDrwBoxOpening2 = true;
                IncDrwBoxOpening3 = true;
                IncDrwBoxOpening4 = true;
            }
            else
            {
                IncDrwBoxOpening1 = false;
                IncDrwBoxOpening2 = false;
                IncDrwBoxOpening3 = false;
                IncDrwBoxOpening4 = false;
            }
        }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening1 { get; set; } partial void OnIncDrwBoxOpening1Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening2 { get; set; } partial void OnIncDrwBoxOpening2Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening3 { get; set; } partial void OnIncDrwBoxOpening3Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening4 { get; set; } partial void OnIncDrwBoxOpening4Changed(bool oldValue, bool newValue)
    {
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool DrillSlideHoles { get; set; } partial void OnDrillSlideHolesChanged(bool oldValue, bool newValue)
    {
        if (!newValue)
        {
            DrillSlideHolesOpening1 = false;
            DrillSlideHolesOpening2 = false;
            DrillSlideHolesOpening3 = false;
            DrillSlideHolesOpening4 = false;
        }
        else
        {
            DrillSlideHolesOpening1 = true;
            DrillSlideHolesOpening2 = true;
            DrillSlideHolesOpening3 = true;
            DrillSlideHolesOpening4 = true;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening1 { get; set; } partial void OnDrillSlideHolesOpening1Changed(bool oldValue, bool newValue)
    {
        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening2 { get; set; } partial void OnDrillSlideHolesOpening2Changed(bool oldValue, bool newValue)
    {
        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening3 { get; set; } partial void OnDrillSlideHolesOpening3Changed(bool oldValue, bool newValue)
    {
        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening4 { get; set; } partial void OnDrillSlideHolesOpening4Changed(bool oldValue, bool newValue)
    {
        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight1 { get; set; } = ""; partial void OnOpeningHeight1Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight2 { get; set; } = ""; partial void OnOpeningHeight2Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight3 { get; set; } = ""; partial void OnOpeningHeight3Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight4 { get; set; } = ""; partial void OnOpeningHeight4Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight2 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = "";

    // Reveal and gap properties
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
    [ObservableProperty] public partial ObservableCollection<int> ListDrwCount { get; set; } = [];

    // Visibility properties
    [ObservableProperty] public partial bool GroupDoorsVisibility { get; set; }
    [ObservableProperty] public partial bool GroupDrawersVisibility { get; set; }
    [ObservableProperty] public partial bool StdOrDrwBaseVisibility { get; set; }
    [ObservableProperty] public partial bool Corner90Visibility { get; set; }
    [ObservableProperty] public partial bool Corner45Visibility { get; set; }
    [ObservableProperty] public partial bool GroupCabinetTopTypeVisibility { get; set; }
    [ObservableProperty] public partial bool GroupDrawerFrontHeightsVisibility { get; set; }
    [ObservableProperty] public partial bool GroupShelvesVisibility { get; set; }
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


    // Calculate opening heights based on drawer count and cabinet height
    private void ResizeOpeningHeights()
    {
        const double MaterialThickness34 = 0.75; // 3/4" material thickness

        double tkHeight = ConvertDimension.FractionToDouble(TKHeight!);
        if (!HasTK) { tkHeight = 0; }
        double height = ConvertDimension.FractionToDouble(Height);
        double opening1Height = ConvertDimension.FractionToDouble(OpeningHeight1);
        double opening2Height = ConvertDimension.FractionToDouble(OpeningHeight2);
        double opening3Height = ConvertDimension.FractionToDouble(OpeningHeight3);
        double opening4Height = ConvertDimension.FractionToDouble(OpeningHeight4);

        if (DrwCount == 2)
        {
            opening2Height = height - tkHeight - (3 * MaterialThickness34) - opening1Height;
            OpeningHeight2 = opening2Height.ToString();
        }

        if (DrwCount == 3)
        {
            opening3Height = height - tkHeight - (4 * MaterialThickness34) - opening1Height - opening2Height;
            OpeningHeight3 = opening3Height.ToString();

        }

        if (DrwCount == 4)
        {
            opening4Height = height - tkHeight - (5 * MaterialThickness34) - opening1Height - opening2Height - opening3Height;
            OpeningHeight4 = opening4Height.ToString();
        }
    }


    // Commands
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
            DrillSlideHoles = DrillSlideHoles,
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
            IncDrwFrontInList1 = IncDrwFrontInList1,
            IncDrwFrontInList2 = IncDrwFrontInList2,
            IncDrwFrontInList3 = IncDrwFrontInList3,
            IncDrwFrontInList4 = IncDrwFrontInList4,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type

    }
}