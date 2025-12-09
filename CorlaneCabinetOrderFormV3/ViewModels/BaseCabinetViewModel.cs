using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
{

    public BaseCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private bool _isResizing;

    public BaseCabinetViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;
        _defaults = defaults;

        // Subscribe to ALL property changes in this ViewModel
        this.PropertyChanged += (_, __) => UpdatePreview();

        _mainVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
                LoadSelectedIfMine();
        };

        Width = "18";
        Height = "34.5";
        Depth = "24";
        LeftFrontWidth = "12";
        RightFrontWidth = "12";
        LeftDepth = "24";
        RightDepth = "24";

        ValidateAllProperties();
    }


    // Base cabinet type strings
    public static string Style1 => "Standard";
    public static string Style2 => "Drawer";
    public static string Style3 => "90° Corner";
    public static string Style4 => "45° Corner";

    // Common properties from CabinetModel
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string Style { get; set; } = ""; partial void OnStyleChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            // Update visibility based on selected type
            StdOrDrwBaseVisibility = (newValue == Style1 || newValue == Style2);
            Corner90Visibility = (newValue == Style3);
            Corner45Visibility = (newValue == Style4);
            GroupShelvesVisibility = (newValue == Style1 || newValue == Style3 || newValue == Style4);
            GroupDrawersVisibility = (newValue == Style1 || newValue == Style2);
            GroupCabinetTopTypeVisibility = (newValue == Style1 || newValue == Style2);
            GroupDrawerFrontHeightsVisibility = (newValue == Style1 || newValue == Style2);
            GroupDoorsVisibility = (newValue == Style1 || newValue == Style3 || newValue == Style4);

            if (newValue == Style2)
            {
                // Drawer cabinet selected
                ListDrwCount = [1,2,3,4];
            }
            else if (newValue == Style1)
            {
                // Standard or corner cabinet selected
                ListDrwCount = [0,1];
            }
            LoadDefaults();
            UpdatePreview();
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
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1,100)] public partial int Qty { get; set; }
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
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = ""; partial void OnDrwFrontHeight1Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight2 { get; set; } = ""; partial void OnDrwFrontHeight2Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = ""; partial void OnDrwFrontHeight3Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = ""; partial void OnDrwFrontHeight4Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }

    // Reveal and gap properties
    [ObservableProperty] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty] public partial string RightReveal { get; set; } = "";
    [ObservableProperty] public partial string TopReveal { get; set; } = "";
    [ObservableProperty] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty] public partial string GapWidth { get; set; } = "";


    // Combobox options
    public List<int> ComboShelfCount { get; } = [0, 1, 2, 3, 4, 5];
    public static List<string> TypeList => [Style1, Style2, Style3, Style4];
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
    [ObservableProperty] public partial bool GroupDoorsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupDrawersVisibility { get; set; } = true;
    [ObservableProperty] public partial bool StdOrDrwBaseVisibility { get; set; } = true;
    [ObservableProperty] public partial bool Corner90Visibility { get; set; }
    [ObservableProperty] public partial bool Corner45Visibility { get; set; }
    [ObservableProperty] public partial bool GroupCabinetTopTypeVisibility { get; set; } = false;
    [ObservableProperty] public partial bool GroupDrawerFrontHeightsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupShelvesVisibility { get; set; } = false;
    [ObservableProperty] public partial bool DrwFrontHeight1Enabled { get; set; }
    [ObservableProperty] public partial bool DrwFrontHeight2Enabled { get; set; }
    [ObservableProperty] public partial bool DrwFrontHeight3Enabled { get; set; }
    [ObservableProperty] public partial bool BaseShowRevealSettings { get; set; }
    [ObservableProperty] public partial bool DrwFront1Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront2Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront3Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront4Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront4PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening1Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening2Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening3Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening4Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening4PropertiesVisible { get; set; } = false;


    [ObservableProperty] public partial bool AdjustOpeningHeightsChecked { get; set; } = true; partial void OnAdjustOpeningHeightsCheckedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {

            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial bool AdjustDrwFrontHeightsChecked { get; set; } = false; partial void OnAdjustDrwFrontHeightsCheckedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            ResizeOpeningHeights();
        }
    }





    private void ResizeOpeningHeights()
    {
        // Prevent re-entrancy caused by property-change handlers
        if (_isResizing) return;

        double opening1Height = ConvertDimension.FractionToDouble(OpeningHeight1);
        double opening2Height = ConvertDimension.FractionToDouble(OpeningHeight2);
        double opening3Height = ConvertDimension.FractionToDouble(OpeningHeight3);
        double opening4Height = ConvertDimension.FractionToDouble(OpeningHeight4);

        double topReveal = ConvertDimension.FractionToDouble(TopReveal);
        double bottomReveal = ConvertDimension.FractionToDouble(BottomReveal);
        double gapWidth = ConvertDimension.FractionToDouble(GapWidth);

        try
        {
            _isResizing = true;

            const double MaterialThickness34 = 0.75; // 3/4" material thickness
            //double halfMaterialThickness34 = MaterialThickness34 / 2;
            double doubleMaterialThickness34 = MaterialThickness34 * 2;

            double tkHeight = ConvertDimension.FractionToDouble(TKHeight);
            if (!HasTK) { tkHeight = 0; }
            double height = ConvertDimension.FractionToDouble(Height);

            if (AdjustOpeningHeightsChecked)
            {
                DrwFrontHeight1Enabled = false;
                DrwFrontHeight2Enabled = false;
                DrwFrontHeight3Enabled = false;


                if (DrwCount == 2)
                {
                    opening2Height = height - tkHeight - (4 * MaterialThickness34) - opening1Height;
                    OpeningHeight2 = opening2Height.ToString();
                    DrwFrontHeight2 = ConvertDimension.DoubleToFraction(opening2Height + MaterialThickness34 - (gapWidth/2) - bottomReveal);
                }

                if (DrwCount == 3)
                {
                    opening3Height = height - tkHeight - (5 * MaterialThickness34) - opening1Height - opening2Height;
                    OpeningHeight3 = opening3Height.ToString();
                    DrwFrontHeight3 = ConvertDimension.DoubleToFraction(opening3Height + MaterialThickness34 - (gapWidth / 2) - bottomReveal);
                }

                if (DrwCount == 4)
                {
                    opening4Height = height - tkHeight - (6 * MaterialThickness34) - opening1Height - opening2Height - opening3Height;
                    OpeningHeight4 = opening4Height.ToString();
                    DrwFrontHeight4 = ConvertDimension.DoubleToFraction(opening4Height + MaterialThickness34 - (gapWidth / 2) - bottomReveal);
                }
            }

            if (AdjustDrwFrontHeightsChecked)
            {
                double drwFrontHeight1 = ConvertDimension.FractionToDouble(DrwFrontHeight1);
                double drwFrontHeight2 = ConvertDimension.FractionToDouble(DrwFrontHeight2);
                double drwFrontHeight3 = ConvertDimension.FractionToDouble(DrwFrontHeight3);
                double drwFrontHeight4 = height - tkHeight  - drwFrontHeight1 - drwFrontHeight2 - drwFrontHeight3 - topReveal - bottomReveal - (3*gapWidth);


                if (DrwCount == 2)
                {
                    opening1Height = drwFrontHeight1 + topReveal + (gapWidth / 2) - doubleMaterialThickness34;
                    opening2Height = drwFrontHeight2 + bottomReveal + (gapWidth / 2) - (MaterialThickness34);  // This is bottom drawer
                    //OpeningHeight2 = opening2Height.ToString();
                }

                if (DrwCount == 3)
                {
                    opening1Height = drwFrontHeight1 + topReveal + (gapWidth / 2) - doubleMaterialThickness34;
                    opening2Height = drwFrontHeight2 + (gapWidth) - (MaterialThickness34);
                    opening3Height = drwFrontHeight3 + bottomReveal + (gapWidth / 2) - (MaterialThickness34); // This is bottom drawer
                    //OpeningHeight3 = opening3Height.ToString();
                }

                if (DrwCount == 4)
                {
                    opening1Height = drwFrontHeight1 + topReveal + (gapWidth / 2) - doubleMaterialThickness34;
                    opening2Height = drwFrontHeight2 + (gapWidth) - (MaterialThickness34);
                    opening3Height = drwFrontHeight3 + (gapWidth) - (MaterialThickness34);
                    opening4Height = drwFrontHeight4 + bottomReveal + (gapWidth / 2) - (MaterialThickness34); // This is bottom drawer
                    //OpeningHeight4 = opening4Height.ToString();
                }

                OpeningHeight1 = opening1Height.ToString();
                OpeningHeight2 = opening2Height.ToString();
                OpeningHeight3 = opening3Height.ToString();
                OpeningHeight4 = opening4Height.ToString();
                DrwFrontHeight4 = drwFrontHeight4.ToString();
            }

            UpdatePreview();
        }
        finally
        {
            _isResizing = false;
        }
    }























    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel baseCab)
        {   
            Style = baseCab.Style;
            Width = baseCab.Width;
            Height = baseCab.Height;
            Depth = baseCab.Depth;
            Species = baseCab.Species;
            EBSpecies = baseCab.EBSpecies;
            Name = baseCab.Name;
            Qty = baseCab.Qty;
            Notes = baseCab.Notes;
            TKHeight = baseCab.TKHeight;  // Subtype-specific
            LeftBackWidth = baseCab.LeftBackWidth;
            RightBackWidth = baseCab.RightBackWidth;
            LeftFrontWidth = baseCab.LeftFrontWidth;
            RightFrontWidth = baseCab.RightFrontWidth;
            LeftDepth = baseCab.LeftDepth;
            RightDepth = baseCab.RightDepth;
            HasTK = baseCab.HasTK;
            TKDepth = baseCab.TKDepth;
            DoorSpecies = baseCab.DoorSpecies;
            BackThickness = baseCab.BackThickness;
            TopType = baseCab.TopType;
            ShelfCount = baseCab.ShelfCount;
            ShelfDepth = baseCab.ShelfDepth;
            DrillShelfHoles = baseCab.DrillShelfHoles;
            DoorCount = baseCab.DoorCount;
            DoorGrainDir = baseCab.DoorGrainDir;
            IncDoorsInList = baseCab.IncDoorsInList;
            IncDoors = baseCab.IncDoors;
            DrillHingeHoles = baseCab.DrillHingeHoles;
            DrwCount = baseCab.DrwCount;
            DrwStyle = baseCab.DrwStyle;
            DrwFrontGrainDir = baseCab.DrwFrontGrainDir;
            IncDrwFrontsInList = baseCab.IncDrwFrontsInList;
            IncDrwFrontInList1 = baseCab.IncDrwFrontInList1;
            IncDrwFrontInList2 = baseCab.IncDrwFrontInList2;
            IncDrwFrontInList3 = baseCab.IncDrwFrontInList3;
            IncDrwFrontInList4 = baseCab.IncDrwFrontInList4;
            IncDrwFronts = baseCab.IncDrwFronts;
            IncDrwFront1 = baseCab.IncDrwFront1;
            IncDrwFront2 = baseCab.IncDrwFront2;
            IncDrwFront3 = baseCab.IncDrwFront3;
            IncDrwFront4 = baseCab.IncDrwFront4;
            IncDrwBoxesInList = baseCab.IncDrwBoxesInList;
            IncDrwBoxInListOpening1 = baseCab.IncDrwBoxInListOpening1;
            IncDrwBoxInListOpening2 = baseCab.IncDrwBoxInListOpening2;
            IncDrwBoxInListOpening3 = baseCab.IncDrwBoxInListOpening3;
            IncDrwBoxInListOpening4 = baseCab.IncDrwBoxInListOpening4;
            IncDrwBoxes = baseCab.IncDrwBoxes;
            IncDrwBoxOpening1 = baseCab.IncDrwBoxOpening1;
            IncDrwBoxOpening2 = baseCab.IncDrwBoxOpening2;
            IncDrwBoxOpening3 = baseCab.IncDrwBoxOpening3;
            IncDrwBoxOpening4 = baseCab.IncDrwBoxOpening4;
            DrillSlideHoles = baseCab.DrillSlideHoles;
            DrillSlideHolesOpening1 = baseCab.DrillSlideHolesOpening1;
            DrillSlideHolesOpening2 = baseCab.DrillSlideHolesOpening2;
            DrillSlideHolesOpening3 = baseCab.DrillSlideHolesOpening3;
            DrillSlideHolesOpening4 = baseCab.DrillSlideHolesOpening4;
            OpeningHeight1 = baseCab.OpeningHeight1;
            OpeningHeight2 = baseCab.OpeningHeight2;
            OpeningHeight3 = baseCab.OpeningHeight3;
            OpeningHeight4 = baseCab.OpeningHeight4;
            DrwFrontHeight1 = baseCab.DrwFrontHeight1;
            DrwFrontHeight2 = baseCab.DrwFrontHeight2;
            DrwFrontHeight3 = baseCab.DrwFrontHeight3;
            DrwFrontHeight4 = baseCab.DrwFrontHeight4;
            LeftReveal = baseCab.LeftReveal;
            RightReveal = baseCab.RightReveal;
            TopReveal = baseCab.TopReveal;
            BottomReveal = baseCab.BottomReveal;
            GapWidth = baseCab.GapWidth;

            UpdatePreview();

        }
        else
        {
            //LoadDefaults();
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
            Style = Style,
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

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel selected)
        {
            selected.Width = Width;
            selected.Height = Height;
            selected.Depth = Depth;
            selected.Species = Species;
            selected.EBSpecies = EBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
            selected.TKHeight = TKHeight;  // Subtype-specific
            selected.Style = Style;
            selected.LeftBackWidth = LeftBackWidth;
            selected.RightBackWidth = RightBackWidth;
            selected.LeftFrontWidth = LeftFrontWidth;
            selected.RightFrontWidth = RightFrontWidth;
            selected.LeftDepth = LeftDepth;
            selected.RightDepth = RightDepth;
            selected.HasTK = HasTK;
            selected.TKDepth = TKDepth;
            selected.DoorSpecies = DoorSpecies;
            selected.BackThickness = BackThickness;
            selected.TopType = TopType;
            selected.ShelfCount = ShelfCount;
            selected.ShelfDepth = ShelfDepth;
            selected.DrillShelfHoles = DrillShelfHoles;
            selected.DoorCount = DoorCount;
            selected.DoorGrainDir = DoorGrainDir;
            selected.IncDoorsInList = IncDoorsInList;
            selected.IncDoors = IncDoors;
            selected.DrillHingeHoles = DrillHingeHoles;
            selected.DrwFrontGrainDir = DrwFrontGrainDir;
            selected.IncDrwFrontsInList = IncDrwFrontsInList;
            selected.IncDrwFronts = IncDrwFronts;
            selected.IncDrwBoxesInList = IncDrwBoxesInList;
            selected.IncDrwBoxes = IncDrwBoxes;
            selected.DrillSlideHoles = DrillSlideHoles;
            selected.DrwCount = DrwCount;
            selected.DrwStyle = DrwStyle;
            selected.OpeningHeight1 = OpeningHeight1;
            selected.OpeningHeight2 = OpeningHeight2;
            selected.OpeningHeight3 = OpeningHeight3;
            selected.OpeningHeight4 = OpeningHeight4;
            selected.IncDrwBoxOpening1 = IncDrwBoxOpening1;
            selected.IncDrwBoxOpening2 = IncDrwBoxOpening2;
            selected.IncDrwBoxOpening3 = IncDrwBoxOpening3;
            selected.IncDrwBoxOpening4 = IncDrwBoxOpening4;
            selected.DrillSlideHolesOpening1 = DrillSlideHolesOpening1;
            selected.DrillSlideHolesOpening2 = DrillSlideHolesOpening2;
            selected.DrillSlideHolesOpening3 = DrillSlideHolesOpening3;
            selected.DrillSlideHolesOpening4 = DrillSlideHolesOpening4;
            selected.IncDrwBoxInListOpening1 = IncDrwBoxInListOpening1;
            selected.IncDrwBoxInListOpening2 = IncDrwBoxInListOpening2;
            selected.IncDrwBoxInListOpening3 = IncDrwBoxInListOpening3;
            selected.IncDrwBoxInListOpening4 = IncDrwBoxInListOpening4;
            selected.DrwFrontHeight1 = DrwFrontHeight1;
            selected.DrwFrontHeight2 = DrwFrontHeight2;
            selected.DrwFrontHeight3 = DrwFrontHeight3;
            selected.DrwFrontHeight4 = DrwFrontHeight4;
            selected.IncDrwFront1 = IncDrwFront1;
            selected.IncDrwFront2 = IncDrwFront2;
            selected.IncDrwFront3 = IncDrwFront3;
            selected.IncDrwFront4 = IncDrwFront4;
            selected.IncDrwFrontInList1 = IncDrwFrontInList1;
            selected.IncDrwFrontInList2 = IncDrwFrontInList2;
            selected.IncDrwFrontInList3 = IncDrwFrontInList3;
            selected.IncDrwFrontInList4 = IncDrwFrontInList4;
            selected.LeftReveal = LeftReveal;
            selected.RightReveal = RightReveal;
            selected.TopReveal = TopReveal;
            selected.BottomReveal = BottomReveal;
            selected.GapWidth = GapWidth;

        }

        // Optional: clear selection after update
        //_mainVm.SelectedCabinet = null;
    }

    [RelayCommand]
    private void LoadDefaults()
    {
        if (_defaults is null) return;

        if (Style == Style2)
        {
            // Drawer cabinet selected
            ListDrwCount = [1, 2, 3, 4];
        }
        else if (Style == Style1)
        {
            // Standard or corner cabinet selected
            ListDrwCount = [0, 1];
        }

        Species = _defaults.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        HasTK = _defaults.DefaultHasTK;
        TKHeight = _defaults.DefaultTKHeight;
        TKDepth = _defaults.DefaultTKDepth;
        DoorCount = _defaults.DefaultDoorCount;
        DoorGrainDir = _defaults.DefaultDoorGrainDir;
        IncDoorsInList = _defaults.DefaultIncDoorsInList;
        IncDoors = _defaults.DefaultIncDoors;
        DrillHingeHoles = _defaults.DefaultDrillHingeHoles;
        DoorSpecies = _defaults.DefaultDoorDrwSpecies;
        BackThickness = _defaults.DefaultBaseBackThickness;
        TopType = _defaults.DefaultTopType;
        ShelfCount = _defaults.DefaultShelfCount;
        ShelfDepth = _defaults.DefaultShelfDepth;
        DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
        DrwFrontGrainDir = _defaults.DefaultDrwGrainDir;
        IncDrwFrontsInList = _defaults.DefaultIncDrwFrontsInList;
        IncDrwFronts = _defaults.DefaultIncDrwFronts;
        IncDrwBoxesInList = _defaults.DefaultIncDrwBoxesInList;
        IncDrwBoxes = _defaults.DefaultIncDrwBoxes;
        DrillSlideHoles = _defaults.DefaultDrillSlideHoles;
        if (Style == Style1) { DrwCount = _defaults.DefaultStdDrawerCount; }
        if (Style == Style2) { DrwCount = _defaults.DefaultDrawerStackDrawerCount; }
        DrwStyle = _defaults.DefaultDrwStyle;
        OpeningHeight1 = _defaults.DefaultOpeningHeight1;
        OpeningHeight2 = _defaults.DefaultOpeningHeight2;
        OpeningHeight3 = _defaults.DefaultOpeningHeight3;
        DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
        DrwFrontHeight2 = _defaults.DefaultDrwFrontHeight2;
        DrwFrontHeight3 = _defaults.DefaultDrwFrontHeight3;
        LeftReveal = _defaults.DefaultBaseLeftReveal;
        RightReveal = _defaults.DefaultBaseRightReveal;
        TopReveal = _defaults.DefaultBaseTopReveal;
        BottomReveal = _defaults.DefaultBaseBottomReveal;
        GapWidth = _defaults.DefaultGapWidth;

    }

    // For 3D model:
    private void UpdatePreview() // Update 3D cabinet model preview
    {
        // _mainVm.CurrentPreviewCabinet = new BaseCabinetModel() --- Original before Preview Service
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();

        var model = new BaseCabinetModel
        {
            Style = Style,
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,

            TKHeight = TKHeight,  // Subtype-specific
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
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            IncDoors = IncDoors,
            DrwFrontGrainDir = DrwFrontGrainDir,
            IncDrwFronts = IncDrwFronts,
            IncDrwBoxes = IncDrwBoxes,
            DrwCount = DrwCount,
            OpeningHeight1 = OpeningHeight1,
            OpeningHeight2 = OpeningHeight2,
            OpeningHeight3 = OpeningHeight3,
            OpeningHeight4 = OpeningHeight4,
            IncDrwBoxOpening1 = IncDrwBoxOpening1,
            IncDrwBoxOpening2 = IncDrwBoxOpening2,
            IncDrwBoxOpening3 = IncDrwBoxOpening3,
            IncDrwBoxOpening4 = IncDrwBoxOpening4,
            DrwFrontHeight1 = DrwFrontHeight1,
            DrwFrontHeight2 = DrwFrontHeight2,
            DrwFrontHeight3 = DrwFrontHeight3,
            DrwFrontHeight4 = DrwFrontHeight4,
            IncDrwFront1 = IncDrwFront1,
            IncDrwFront2 = IncDrwFront2,
            IncDrwFront3 = IncDrwFront3,
            IncDrwFront4 = IncDrwFront4,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        // Request preview using the tab index owner token (Base tab = 0)
        previewSvc.RequestPreview(0, model);
    }
}