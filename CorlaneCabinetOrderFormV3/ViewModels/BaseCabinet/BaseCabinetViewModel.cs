using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
{
    public BaseCabinetViewModel()
    {
        // empty constructor for design-time support
        _lookups = new MaterialLookupService();
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private readonly IPreviewService? _previewService;
    private bool _isResizing;
    private bool _isMapping; // true while MapModelToViewModel is running

    private readonly IMaterialLookupService _lookups;
    public ObservableCollection<string> ListCabSpecies => _lookups.CabinetSpecies;
    public ObservableCollection<string> ListEBSpecies => _lookups.EBSpecies;

    private readonly System.Timers.Timer _drwFrontHeight1DebounceTimer = new(90) { AutoReset = false };

    private bool _suppressEditSync;

    private bool _isEditingDrwFrontHeight1;

    public BaseCabinetViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults, IMaterialLookupService lookups, IPreviewService previewService)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;
        _defaults = defaults;
        _lookups = lookups;
        _previewService = previewService;

        // Only rebuild preview when geometry-affecting properties change
        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is not null && s_previewProperties.Contains(e.PropertyName))
                UpdatePreview();
        };

        PropertyChangedEventManager.AddHandler(
            _mainVm,
            MainVm_PropertyChanged,
            nameof(MainWindowViewModel.SelectedCabinet));


        Width = "18";
        Height = "34.5";
        Depth = "24";
        LeftFrontWidth = "12";
        RightFrontWidth = "12";
        LeftDepth = "24";
        RightDepth = "24";
        LeftBackWidth = "36";
        RightBackWidth = "36";
        Style = Style1;
        ListRolloutCount = [0, 1, 2];

        LoadDefaults();

        if (_defaults != null)
        {
            PropertyChangedEventManager.AddHandler(
                _defaults,
                Defaults_PropertyChanged,
                nameof(DefaultSettingsService.DefaultDimensionFormat));
        }
    }

    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
        {
            LoadSelectedIfMine();
        }
    }

    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            OnPropertyChanged(nameof(ListBackThickness));
        }
    }


    // Base cabinet type strings
    public static string Style1 => CabinetStyles.Base.Standard;
    public static string Style2 => CabinetStyles.Base.Drawer;
    public static string Style3 => CabinetStyles.Base.Corner90;
    public static string Style4 => CabinetStyles.Base.AngleFront;

    // Common properties from CabinetModel
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string Style { get; set; } = ""; partial void OnStyleChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        ApplyStyleVisibility(newValue);

        TrashDrawer = (newValue != Style1) ? false : TrashDrawer;

        if (newValue == Style2)
        {
            // Drawer cabinet selected
            RolloutCount = 0;
        }
        else if (newValue == Style1)
        {
            // Standard or corner cabinet selected
            if (DrwCount == 1)
            {
                DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
            }
        }
        RecalculateFrontWidth();
        ResizeOpeningHeights();
        ResizeDrwFrontHeights();
        RunValidationVisible();
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(7, 95)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(6, 120)] public partial string Height { get; set; } = ""; partial void OnHeightChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue == oldValue) return;

        ListRolloutCount = [0];
        RolloutCount = 0;
        IncRollouts = false;

        if (Style == Style1)
        {
            double interiorHeight = ConvertDimension.FractionToDouble(newValue) - (2 * 0.75);
            if (DrwCount == 1)
            {
                interiorHeight -= ConvertDimension.FractionToDouble(OpeningHeight1) - 0.75;
            }

            ListRolloutCount = BuildRolloutCountList(interiorHeight);

            ResizeOpeningHeights();
            return;
        }

        RecalculateDrawerLayout();


    }
    private static ObservableCollection<int> BuildRolloutCountList(double interiorHeight)
    {
        int maxRollouts = Math.Clamp(2 + (int)(interiorHeight / 12), 2, 10);
        return new ObservableCollection<int>(Enumerable.Range(0, maxRollouts + 1));
    }

    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), BaseCabinetDepthRange(48)] public partial string Depth { get; set; } = ""; partial void OnDepthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (ConvertDimension.FractionToDouble(Depth) < 10.625)
        {
            IncDrwBoxOpening1 = false;
            IncDrwBoxOpening2 = false;
            IncDrwBoxOpening3 = false;
            IncDrwBoxOpening4 = false;
            IncRollouts = false;
            IncRolloutsEnabled = false;
            TopType = CabinetOptions.TopType.Full;
            ApplyStyleVisibility(Style);
        }
        else
        {
            bool incBoxes = _defaults.DefaultIncDrwBoxes;
            IncDrwBoxOpening1 = incBoxes;
            IncDrwBoxOpening2 = incBoxes;
            IncDrwBoxOpening3 = incBoxes;
            IncDrwBoxOpening4 = incBoxes;
            TopType = _defaults.DefaultTopType;
            ApplyStyleVisibility(Style);
        }
        if (ConvertDimension.FractionToDouble(Depth) < 8)
        {
            ShelfDepth = CabinetOptions.ShelfDepth.FullDepth;
        }
        else
        {
            ShelfDepth = _defaults.DefaultShelfDepth;
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string Species { get; set; } = ""; partial void OnSpeciesChanged(string oldValue, string newValue)
    {
        if (newValue == "Custom")
        {
            CustomCabSpeciesEnabled = true;
        }
        else
        {
            CustomCabSpeciesEnabled = false;
        }
    }
    [ObservableProperty] public partial string CustomSpecies { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string EBSpecies { get; set; } = ""; partial void OnEBSpeciesChanged(string oldValue, string newValue)
    {
        if (newValue == "Custom")
        {
            CustomEBSpeciesEnabled = true;
        }
        else
        {
            CustomEBSpeciesEnabled = false;
        }
    }
    [ObservableProperty] public partial string CustomEBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1, 100)] public partial int Qty { get; set; } = 1;
    [ObservableProperty] public partial string Notes { get; set; } = "";

    // Type-specific properties for BaseCabinetModel

    // Corner Cab specific properties
    [ObservableProperty] public partial bool SinkCabinet { get; set; } = false; partial void OnSinkCabinetChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        IncDrwBoxInListOpening1 = (!newValue);
        IncDrwBoxOpening1 = (!newValue);
        DrillSlideHolesOpening1 = (!newValue);

        ApplyStyleVisibility(Style);
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftBackWidth { get; set; } = ""; partial void OnLeftBackWidthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightBackWidth { get; set; } = ""; partial void OnRightBackWidthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftFrontWidth { get; set; } = ""; partial void OnLeftFrontWidthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateBackWidths90();
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightFrontWidth { get; set; } = ""; partial void OnRightFrontWidthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateBackWidths90();
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftDepth { get; set; } = ""; partial void OnLeftDepthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateBackWidths90();
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightDepth { get; set; } = ""; partial void OnRightDepthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateBackWidths90();
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty] public partial string FrontWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftBackWidth90 { get; set; } = "";
    [ObservableProperty] public partial string RightBackWidth90 { get; set; } = "";

    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BackThickness { get; set; } = ""; partial void OnBackThicknessChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RunValidationVisible();
        }
    }
    [ObservableProperty] public partial string TopType { get; set; } = "";



    // Toekick-specific properties
    [ObservableProperty] public partial bool HasTK { get; set; }
    partial void OnHasTKChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        // Depth min depends on HasTK/TKDepth, so revalidate Depth when HasTK changes.
        ValidateProperty(Depth, nameof(Depth));

        RecalculateDrawerLayout();
        RunValidationVisible();
    }

    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(2, 8)] public partial string TKHeight { get; set; } = ""; partial void OnTKHeightChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (EqualizeAllDrwFronts || EqualizeBottomDrwFronts)
        {
            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
        }
        else
        {
            ResizeOpeningHeights();
            ResizeDrwFrontHeights();
        }
    }

    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(0, 8)] public partial string TKDepth { get; set; } = "";
    partial void OnTKDepthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        // Depth min depends on TKDepth when HasTK is true, so revalidate Depth when TKDepth changes.
        ValidateProperty(Depth, nameof(Depth));
    }



    // Shelf-specific properties
    [ObservableProperty] public partial int ShelfCount { get; set; } partial void OnShelfCountChanged(int value)
    {
        if (_isMapping) return;

        if (value == 0)
        {
            DrillShelfHoles = false;
        }

        if (value > 0)
        {
            DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
        }
    }
    [ObservableProperty] public partial string ShelfDepth { get; set; } = "";
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }

    // Door-specific properties
    [ObservableProperty] public partial string DoorSpecies { get; set; } = ""; partial void OnDoorSpeciesChanged(string oldValue, string newValue)
    {
        if (newValue == "Custom")
        {
            CustomDoorSpeciesEnabled = true;
        }
        else
        {
            CustomDoorSpeciesEnabled = false;
        }
    }
    [ObservableProperty] public partial string CustomDoorSpecies { get; set; } = "";
    [ObservableProperty] public partial int DoorCount { get; set; } partial void OnDoorCountChanged(int oldValue, int newValue)
    {
        if (_isMapping) return;

        if (newValue == 0)
        {
            IncDoors = false;
            IncDoorsInList = false;
            DrillHingeHoles = false;
        }

        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }

    // Drawer-specific properties
    [ObservableProperty] public partial int DrwCount { get; set; }
    partial void OnDrwCountChanged(int oldValue, int newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            // Reset drawer front heights / equalization to defaults for the new count.
            // LoadDefaultDrwSettings already suppresses intermediate resizes,
            // sets equalization modes, pre-seeds heights, and runs a single
            // ApplyDrawerFrontEqualization + ResizeDrwFrontHeights pass.
            LoadDefaultDrwSettings();
            RunValidationVisible();
        }

        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial string DrwStyle { get; set; } = "";
    [ObservableProperty] public partial string DrwFrontGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDrwFrontInList1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList4 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront4 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening4 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening4 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening1 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening2 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening3 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening4 { get; set; }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight1 { get; set; } = ""; partial void OnOpeningHeight1Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight2 { get; set; } = ""; partial void OnOpeningHeight2Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string OpeningHeight3 { get; set; } = ""; partial void OnOpeningHeight3Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string OpeningHeight4 { get; set; } = ""; partial void OnOpeningHeight4Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = ""; partial void OnDrwFrontHeight1Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;

        // Keep edit buffer synced when not typing; never overwrite while user is mid-edit.
        if (!_suppressEditSync && !_isEditingDrwFrontHeight1)
            DrwFrontHeight1Edit = newValue;

        if (newValue != oldValue)
        {
            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight2 { get; set; } = ""; partial void OnDrwFrontHeight2Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = ""; partial void OnDrwFrontHeight3Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = ""; partial void OnDrwFrontHeight4Changed(string oldValue, string newValue)
    {
        if (_isMapping) return;
        if (newValue != oldValue)
        {
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string RolloutStyle { get; set; } = "";
    [ObservableProperty] public partial bool IncRollouts { get; set; } = false; partial void OnIncRolloutsChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (newValue)
        {
            TrashDrawer = false;
        }
        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial bool IncRolloutsInList { get; set; } = false;
    [ObservableProperty] public partial int RolloutCount { get; set; } = 0; partial void OnRolloutCountChanged(int oldValue, int newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            if (RolloutCount > 0)
            {
                ShelfCount = 0;
                IncRollouts = _defaults.DefaultIncDrwBoxes;
            }
            else
            { 
                IncRollouts = false;
                IncRolloutsInList = false;
            }
        }

        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial bool DrillSlideHolesForRollouts { get; set; } = false;
    [ObservableProperty] public partial bool TrashDrawer { get; set; } = false; partial void OnTrashDrawerChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (newValue)
        {
            ShelfCount = 0;
            DrillShelfHoles = false;
            RolloutCount = 0;
            IncRollouts = false;
            IncRolloutsInList = false;
        }


        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial bool IncTrashDrwBox { get; set; } = true;
    [ObservableProperty] public partial bool EqualizeBottomDrwFronts { get; set; } = false; partial void OnEqualizeBottomDrwFrontsChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (newValue)
        {
            EqualizeAllDrwFronts = false;

            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
        }

        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial bool EqualizeAllDrwFronts { get; set; } = false; partial void OnEqualizeAllDrwFrontsChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (newValue)
        {
            EqualizeBottomDrwFronts = false;

            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
        }

        ApplyStyleVisibility(Style);
    }

    // Reveal and gap properties
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string RightReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string TopReveal { get; set; } = ""; partial void OnTopRevealChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateDrawerLayout();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BottomReveal { get; set; } = ""; partial void OnBottomRevealChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateDrawerLayout();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string GapWidth { get; set; } = ""; partial void OnGapWidthChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RecalculateDrawerLayout();
        }
    }


    // Combobox options
    public IReadOnlyList<int> ComboShelfCount => CabinetOptions.ShelfCounts;
    public static List<string> TypeList => [Style1, Style2, Style3, Style4];

    public IReadOnlyList<string> ListDrawerStyle => CabinetOptions.DrawerStyles;
    public IReadOnlyList<int> ListDoorCount => CabinetOptions.DoorCounts;
    public IReadOnlyList<string> ListGrainDirection => CabinetOptions.GrainDirections;
    public IReadOnlyList<string> ListShelfDepth => CabinetOptions.ShelfDepths;
    public IReadOnlyList<string> ListTopType => CabinetOptions.TopTypes;

    public List<string> ListBackThickness =>
        CabinetOptions.BackThickness.GetList(_defaults?.DefaultDimensionFormat ?? "Decimal");

    [ObservableProperty] public partial ObservableCollection<int> ListDrwCount { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<int> ListRolloutCount { get; set; } = [];



    // Visibility properties
    [ObservableProperty] public partial bool DrawersStackPanelVisible { get; set; } = true;
    [ObservableProperty] public partial bool GroupDoorsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupDrawersVisibility { get; set; } = true;
    [ObservableProperty] public partial bool StdOrDrwBaseVisibility { get; set; } = false;
    [ObservableProperty] public partial bool Corner90Visibility { get; set; } = false;
    [ObservableProperty] public partial bool Corner45Visibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupCabinetTopTypeVisibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupDrawerFrontHeightsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool GroupShelvesVisibility { get; set; } = true;
    [ObservableProperty] public partial bool BaseShowRevealSettings { get; set; }
    [ObservableProperty] public partial bool DrwFront1Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront2Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront3Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront4Visible { get; set; } = true;
    [ObservableProperty] public partial bool DrwFront1Disabled { get; set; }
    [ObservableProperty] public partial bool DrwFront2Disabled { get; set; }
    [ObservableProperty] public partial bool DrwFront3Disabled { get; set; }
    [ObservableProperty] public partial bool DrwFront1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrwFront4PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening1Disabled { get; set; }
    [ObservableProperty] public partial bool Opening2Disabled { get; set; }
    [ObservableProperty] public partial bool Opening3Disabled { get; set; }
    [ObservableProperty] public partial bool Opening1Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening2Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening3Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening4Visible { get; set; } = true;
    [ObservableProperty] public partial bool Opening1PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening2PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening3PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool Opening4PropertiesVisible { get; set; } = false;
    [ObservableProperty] public partial bool BackThicknessVisible { get; set; } = true;
    [ObservableProperty] public partial bool GroupRolloutsVisible { get; set; } = true;
    [ObservableProperty] public partial bool TrashDrawerEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IncRolloutsEnabled { get; set; } = true;
    [ObservableProperty] public partial bool IncRolloutsInListEnabled { get; set; } = true;
    [ObservableProperty] public partial bool CustomCabSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool CustomEBSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool CustomDoorSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool SinkCabinetEnabled { get; set; } = true;
    [ObservableProperty] public partial bool DoorGrainDirVisible { get; set; } = true;
    [ObservableProperty] public partial bool SupplySlabDoorsVisible { get; set; } = true;
    [ObservableProperty] public partial bool IncDoorsInListVisible { get; set; } = true;
    [ObservableProperty] public partial bool DrillHingeHolesVisible { get; set; } = true;
    [ObservableProperty] public partial bool IncRolloutsVisible { get; set; } = false;
    [ObservableProperty] public partial bool IncRolloutsInListVisible { get; set; } = false;
    [ObservableProperty] public partial bool RolloutStyleVisible { get; set; } = false;
    [ObservableProperty] public partial bool DrillSlideHolesForRolloutsVisible { get; set; } = false;
    [ObservableProperty] public partial bool ListDrawerStyleVisible { get; set; } = true;
    [ObservableProperty] public partial bool ComboShelfDepthEnabled { get; set; } = true;
    [ObservableProperty] public partial bool ShelfDepthVisible { get; set; } = true;
    [ObservableProperty] public partial string DrwFrontHeight1Edit { get; set; } = ""; partial void OnDrwFrontHeight1EditChanged(string oldValue, string newValue)
    {
        _isEditingDrwFrontHeight1 = true;

        _drwFrontHeight1DebounceTimer.Stop();
        _drwFrontHeight1DebounceTimer.Elapsed -= DrwFrontHeight1DebounceTimer_Elapsed;
        _drwFrontHeight1DebounceTimer.Elapsed += DrwFrontHeight1DebounceTimer_Elapsed;
        _drwFrontHeight1DebounceTimer.Start();
    }
    private void DrwFrontHeight1DebounceTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!TryParsePositiveDimension(DrwFrontHeight1Edit, out _))
                return; // Option 1: keep last valid preview

            try
            {
                _suppressEditSync = true;
                DrwFrontHeight1 = DrwFrontHeight1Edit; // triggers your existing OnDrwFrontHeight1Changed -> Resize -> UpdatePreview
                _isEditingDrwFrontHeight1 = false;
            }
            finally
            {
                _suppressEditSync = false;
            }
        });
    }

    private static bool TryParsePositiveDimension(string? text, out double value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // ConvertDimension returns 0 on invalid input AND for "0", so gate by allowing 0 explicitly if you want it.
        value = ConvertDimension.FractionToDouble(text);

        if (value > 0)
            return true;

        // if you actually want to allow 0, handle it here; for cabinet dimensions it’s typically invalid:
        return false;
    }
}