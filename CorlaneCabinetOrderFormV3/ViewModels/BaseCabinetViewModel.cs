using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using System.Windows;
using System.Windows.Media;

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

    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), BaseCabinetDepthRange(48)] public partial string Depth { get; set; } = "";
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
        IncDrwBoxesVisible = !newValue;
        IncDrwBoxesInListVisible = !newValue;
        DrillSlideHolesVisible = !newValue;
        ListDrawerStyleVisible = !newValue;
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


    private void EnforceTopTypeForShallowDepth()
    {
        // If the user chose "Stretcher" but the cabinet is very shallow, force "Full".

        double depth = ConvertDimension.FractionToDouble(Depth);

        if (depth > 0 && depth < 10)
        { 
            TopType = CabinetOptions.TopType.Full;
        }
    }

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
    [ObservableProperty] public partial bool IncDrwFrontsInList { get; set; }
    partial void OnIncDrwFrontsInListChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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
    [ObservableProperty] public partial bool IncDrwFrontInList1 { get; set; }
    partial void OnIncDrwFrontInList1Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList2 { get; set; }
    partial void OnIncDrwFrontInList2Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList3 { get; set; }
    partial void OnIncDrwFrontInList3Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFrontInList4 { get; set; }
    partial void OnIncDrwFrontInList4Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFrontInList1 && !IncDrwFrontInList2 && !IncDrwFrontInList3 && !IncDrwFrontInList4)
        { IncDrwFrontsInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwFronts { get; set; }
    partial void OnIncDrwFrontsChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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
    [ObservableProperty] public partial bool IncDrwFront1 { get; set; }
    partial void OnIncDrwFront1Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront2 { get; set; }
    partial void OnIncDrwFront2Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront3 { get; set; }
    partial void OnIncDrwFront3Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwFront4 { get; set; }
    partial void OnIncDrwFront4Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwFront1 && !IncDrwFront2 && !IncDrwFront3 && !IncDrwFront4)
        { IncDrwFronts = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxesInList { get; set; }
    partial void OnIncDrwBoxesInListChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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
    [ObservableProperty] public partial bool IncDrwBoxInListOpening1 { get; set; }
    partial void OnIncDrwBoxInListOpening1Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening2 { get; set; }
    partial void OnIncDrwBoxInListOpening2Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening3 { get; set; }
    partial void OnIncDrwBoxInListOpening3Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening4 { get; set; }
    partial void OnIncDrwBoxInListOpening4Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxInListOpening1 && !IncDrwBoxInListOpening2 && !IncDrwBoxInListOpening3 && !IncDrwBoxInListOpening4)
        { IncDrwBoxesInList = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxes { get; set; }
    partial void OnIncDrwBoxesChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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

            if (SinkCabinet)
            {
                IncDrwBoxOpening1 = false;
                DrillSlideHolesOpening1 = false;
                IncDrwBoxInListOpening1 = false;
            }
        }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening1 { get; set; }
    partial void OnIncDrwBoxOpening1Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening2 { get; set; }
    partial void OnIncDrwBoxOpening2Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening3 { get; set; }
    partial void OnIncDrwBoxOpening3Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool IncDrwBoxOpening4 { get; set; }
    partial void OnIncDrwBoxOpening4Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;
        if (!IncDrwBoxOpening1 && !IncDrwBoxOpening2 && !IncDrwBoxOpening3 && !IncDrwBoxOpening4)
        { IncDrwBoxes = false; }
    }
    [ObservableProperty] public partial bool DrillSlideHoles { get; set; }
    partial void OnDrillSlideHolesChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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
    [ObservableProperty] public partial bool DrillSlideHolesOpening1 { get; set; }
    partial void OnDrillSlideHolesOpening1Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening2 { get; set; }
    partial void OnDrillSlideHolesOpening2Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening3 { get; set; }
    partial void OnDrillSlideHolesOpening3Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (!DrillSlideHolesOpening1 && !DrillSlideHolesOpening2 && !DrillSlideHolesOpening3 && !DrillSlideHolesOpening4)
        {
            DrillSlideHoles = false;
        }
    }
    [ObservableProperty] public partial bool DrillSlideHolesOpening4 { get; set; }
    partial void OnDrillSlideHolesOpening4Changed(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

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
    [ObservableProperty] public partial string OpeningHeight4 { get; set; } = ""; partial void OnOpeningHeight4Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeOpeningHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = "";
    partial void OnDrwFrontHeight1Changed(string oldValue, string newValue)
    {
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
        if (newValue != oldValue)
        {
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = ""; partial void OnDrwFrontHeight3Changed(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            ResizeDrwFrontHeights();
        }
    }
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = ""; partial void OnDrwFrontHeight4Changed(string oldValue, string newValue)
    {
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
            TrashDrawerEnabled = false;
        }
        else
        {
            TrashDrawerEnabled = true;
        }
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
            IncRolloutsEnabled = false;
            IncRolloutsInListEnabled = false;
            GroupRolloutsVisible = false;
        }

        if (!newValue)
        {
            IncRolloutsEnabled = true;
            IncRolloutsInListEnabled = true;
            GroupRolloutsVisible = true;
        }
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

            DrwFront1Disabled = false;
            DrwFront2Disabled = true;
            DrwFront3Disabled = true;

            Opening1Disabled = false;
            Opening2Disabled = true;
            Opening3Disabled = true;
        }
        else
        {
            DrwFront1Disabled = false;
            DrwFront2Disabled = false;
            DrwFront3Disabled = false;

            Opening1Disabled = false;
            Opening2Disabled = false;
            Opening3Disabled = false;
        }
    }
    [ObservableProperty] public partial bool EqualizeAllDrwFronts { get; set; } = false; partial void OnEqualizeAllDrwFrontsChanged(bool oldValue, bool newValue)
    {
        if (_isMapping) return;

        if (newValue)
        {
            EqualizeBottomDrwFronts = false;

            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();

            DrwFront1Disabled = true;
            DrwFront2Disabled = true;
            DrwFront3Disabled = true;

            Opening1Disabled = true;
            Opening2Disabled = true;
            Opening3Disabled = true;
        }
        else
        {
            DrwFront1Disabled = false;
            DrwFront2Disabled = false;
            DrwFront3Disabled = false;

            Opening1Disabled = false;
            Opening2Disabled = false;
            Opening3Disabled = false;
        }
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
    [ObservableProperty] public partial bool IncDrwBoxesVisible { get; set; } = true;
    [ObservableProperty] public partial bool IncDrwBoxesInListVisible { get; set; } = true;
    [ObservableProperty] public partial bool DrillSlideHolesVisible { get; set; } = true;
    [ObservableProperty] public partial bool ListDrawerStyleVisible { get; set; } = true;
    [ObservableProperty] public partial bool ComboShelfDepthEnabled { get; set; } = true;

    

    private void ResizeOpeningHeights()
    {
        if (_isResizing || _isMapping) return;

        try
        {
            _isResizing = true;

            var input = BuildLayoutInputs();
            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);
            ApplyLayoutResult(result);

            // Style-specific disable flags
            if (Style == Style1)
                Opening1Disabled = DrwCount == 0;
            else if (Style == Style2)
            {
                Opening1Disabled = DrwCount == 1;
                if (DrwCount >= 2) { Opening1Disabled = false; Opening2Disabled = true; Opening3Disabled = true; }
                if (DrwCount >= 3) Opening2Disabled = false;
                if (DrwCount >= 4) Opening3Disabled = false;
            }

            UpdatePreview();
        }
        finally { _isResizing = false; }
    }

    private CabinetLayoutCalculator.LayoutInputs BuildLayoutInputs() => new(
        Style, DrwCount,
        ConvertDimension.FractionToDouble(Height),
        ConvertDimension.FractionToDouble(TKHeight),
        HasTK,
        ConvertDimension.FractionToDouble(TopReveal),
        ConvertDimension.FractionToDouble(BottomReveal),
        ConvertDimension.FractionToDouble(GapWidth),
        ConvertDimension.FractionToDouble(OpeningHeight1),
        ConvertDimension.FractionToDouble(OpeningHeight2),
        ConvertDimension.FractionToDouble(OpeningHeight3),
        ConvertDimension.FractionToDouble(OpeningHeight4),
        ConvertDimension.FractionToDouble(DrwFrontHeight1),
        ConvertDimension.FractionToDouble(DrwFrontHeight2),
        ConvertDimension.FractionToDouble(DrwFrontHeight3),
        ConvertDimension.FractionToDouble(DrwFrontHeight4));

    private void ApplyLayoutResult(CabinetLayoutCalculator.LayoutResult r)
    {
        OpeningHeight1 = r.Opening1.ToString();
        OpeningHeight2 = r.Opening2.ToString();
        OpeningHeight3 = r.Opening3.ToString();
        OpeningHeight4 = r.Opening4.ToString();
        DrwFrontHeight1 = r.DrwFront1.ToString();
        DrwFrontHeight2 = r.DrwFront2.ToString();
        DrwFrontHeight3 = r.DrwFront3.ToString();
        DrwFrontHeight4 = r.DrwFront4.ToString();
    }

    private void ResizeDrwFrontHeights()
    {
        if (_isResizing || _isMapping) return;

        try
        {
            _isResizing = true;

            var input = BuildLayoutInputs();
            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);
            ApplyLayoutResult(result);

            // Style-specific disable flags
            if (Style == Style1 && DrwCount == 1)
            {
                Opening1Disabled = false;
                DrwFront1Disabled = false;
            }
            else if (Style == Style2)
            {
                if (DrwCount == 1) DrwFront1Disabled = true;
                if (DrwCount >= 2) { DrwFront1Disabled = false; DrwFront2Disabled = true; }
                if (DrwCount >= 3) { DrwFront2Disabled = false; DrwFront3Disabled = true; }
                if (DrwCount >= 4) DrwFront3Disabled = false;
            }

            if (EqualizeBottomDrwFronts)
            {
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
            }
            if (EqualizeAllDrwFronts)
            {
                DrwFront1Disabled = true;
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
            }

            UpdatePreview();
        }
        finally { _isResizing = false; }
    }

    private void ApplyDrawerFrontEqualization()
    {
        if (_isResizing || _isMapping) return;
        if (Style != Style2) return;
        if (DrwCount <= 0) return;
        if (!EqualizeAllDrwFronts && !EqualizeBottomDrwFronts) return;

        double tkHeight = ConvertDimension.FractionToDouble(TKHeight);
        if (!HasTK) tkHeight = 0;
        double height = ConvertDimension.FractionToDouble(Height) - tkHeight;

        double topReveal = ConvertDimension.FractionToDouble(TopReveal);
        double bottomReveal = ConvertDimension.FractionToDouble(BottomReveal);
        double gapWidth = ConvertDimension.FractionToDouble(GapWidth);

        try
        {
            _isResizing = false; // _isResizing = true breaks this, because the openings won't resize

            if (EqualizeAllDrwFronts)
            {
                if (DrwCount <= 0) return;

                double each = CabinetLayoutCalculator.EqualizeAll(height, topReveal, bottomReveal, gapWidth, DrwCount);

                DrwFrontHeight1 = each.ToString();
                DrwFrontHeight2 = each.ToString();
                DrwFrontHeight3 = each.ToString();
                if (DrwCount >= 4) DrwFrontHeight4 = each.ToString();
            }
            else if (EqualizeBottomDrwFronts)
            {
                if (DrwCount <= 1) return;

                double top = ConvertDimension.FractionToDouble(DrwFrontHeight1);
                double eachBottom = CabinetLayoutCalculator.EqualizeBottom(height, topReveal, bottomReveal, gapWidth, DrwCount, top);

                if (DrwCount >= 2) DrwFrontHeight2 = eachBottom.ToString();
                if (DrwCount >= 3) DrwFrontHeight3 = eachBottom.ToString();
                if (DrwCount >= 4) DrwFrontHeight4 = eachBottom.ToString();
            }
        }
        finally
        {
            _isResizing = false;
        }
    }


    private void RecalculateDrawerLayout()
    {
        if (EqualizeAllDrwFronts || EqualizeBottomDrwFronts)
        {
            ApplyDrawerFrontEqualization();
        }
        else
        {
            ResizeOpeningHeights();
        }
        ResizeDrwFrontHeights();
    }

    private void RecalculateFrontWidth()
    {
        if (_isResizing || _isMapping)
            return;

        if (!string.Equals(Style, Style4, StringComparison.Ordinal))
        {
            FrontWidth = string.Empty;
            return;
        }

        try
        {
            double frontWidth = CabinetLayoutCalculator.ComputeAngleFrontWidth(
                ConvertDimension.FractionToDouble(LeftDepth),
                ConvertDimension.FractionToDouble(RightDepth),
                ConvertDimension.FractionToDouble(LeftBackWidth),
                ConvertDimension.FractionToDouble(RightBackWidth));

            string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
            FrontWidth = string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                ? ConvertDimension.DoubleToFraction(frontWidth)
                : frontWidth.ToString("0.####");
        }
        catch
        {
            FrontWidth = string.Empty;
        }
    }

    private void RecalculateBackWidths90()
    {
        double leftBack = ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth);
        double rightBack = ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth);

        bool useFraction = string.Equals(_defaults?.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase);

        LeftBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(leftBack)
            : leftBack.ToString();

        RightBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(rightBack)
            : rightBack.ToString();
    }

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel baseCab)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(baseCab, dimFormat);

            // Kill any stale debounce timer that fired during mapping
            // and resync the edit buffer to the final correct value.
            _drwFrontHeight1DebounceTimer.Stop();
            _isEditingDrwFrontHeight1 = false;
            DrwFrontHeight1Edit = DrwFrontHeight1; // restarts timer via OnDrwFrontHeight1EditChanged
            _drwFrontHeight1DebounceTimer.Stop();  // kill it again immediately
            _isEditingDrwFrontHeight1 = false;

            UpdatePreview();
        }
        else
        {
            //LoadDefaults();
        }
    }

    /// <summary>
    /// Applies style-specific constraints before saving to a model
    /// (e.g. drawer cabs have 0 doors, corner cabs force 3/4" back).
    /// </summary>
    private void EnforceStyleConstraints()
    {
        if (Style == Style2)
        {
            DoorCount = 0;
            DrillHingeHoles = false;
            DrillShelfHoles = false;
            RolloutCount = 0;
            ShelfCount = 0;
        }

        if (Style == Style3)
        {
            if (DoorCount == 1)
            { DoorCount = 2; }
            DrwCount = 0;
            TopType = CabinetOptions.TopType.Full;
            BackThickness = "0.75"; // Force 3/4" back
        }

        if (Style == Style4)
        {
            DrwCount = 0;
            RolloutCount = 0;
            TopType = CabinetOptions.TopType.Full;
            BackThickness = "0.75"; // Force 3/4" back
        }
    }

    /// <summary>
    /// Copies all current ViewModel property values into the target model,
    /// converting dimension strings to numeric format.
    /// </summary>
    private void ApplyViewModelToModel(BaseCabinetModel target)
    {
        target.Width = ConvertDimension.FractionToDouble(Width).ToString();
        target.Height = ConvertDimension.FractionToDouble(Height).ToString();
        target.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
        target.Species = Species;
        target.CustomSpecies = CustomSpecies;
        target.EBSpecies = EBSpecies;
        target.CustomEBSpecies = CustomEBSpecies;
        target.Name = Name;
        target.Qty = Qty;
        target.Notes = Notes;
        target.TKHeight = ConvertDimension.FractionToDouble(TKHeight).ToString();
        target.Style = Style;
        target.LeftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth).ToString();
        target.RightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth).ToString();
        target.LeftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidth).ToString();
        target.RightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidth).ToString();
        target.LeftDepth = ConvertDimension.FractionToDouble(LeftDepth).ToString();
        target.RightDepth = ConvertDimension.FractionToDouble(RightDepth).ToString();
        target.HasTK = HasTK;
        target.TKDepth = ConvertDimension.FractionToDouble(TKDepth).ToString();
        target.DoorSpecies = DoorSpecies;
        target.CustomDoorSpecies = CustomDoorSpecies;
        target.BackThickness = ConvertDimension.FractionToDouble(BackThickness).ToString();
        target.TopType = TopType;
        target.ShelfCount = ShelfCount;
        target.ShelfDepth = ShelfDepth;
        target.DrillShelfHoles = DrillShelfHoles;
        target.DoorCount = DoorCount;
        target.DoorGrainDir = DoorGrainDir;
        target.IncDoorsInList = IncDoorsInList;
        target.IncDoors = IncDoors;
        target.DrillHingeHoles = DrillHingeHoles;
        target.DrwFrontGrainDir = DrwFrontGrainDir;
        target.IncDrwFrontsInList = IncDrwFrontsInList;
        target.IncDrwFronts = IncDrwFronts;
        target.IncDrwBoxesInList = IncDrwBoxesInList;
        target.IncDrwBoxes = IncDrwBoxes;
        target.DrillSlideHoles = DrillSlideHoles;
        target.DrwCount = DrwCount;
        target.DrwStyle = DrwStyle;
        target.OpeningHeight1 = ConvertDimension.FractionToDouble(OpeningHeight1).ToString();
        target.OpeningHeight2 = ConvertDimension.FractionToDouble(OpeningHeight2).ToString();
        target.OpeningHeight3 = ConvertDimension.FractionToDouble(OpeningHeight3).ToString();
        target.OpeningHeight4 = ConvertDimension.FractionToDouble(OpeningHeight4).ToString();
        target.IncDrwBoxOpening1 = IncDrwBoxOpening1;
        target.IncDrwBoxOpening2 = IncDrwBoxOpening2;
        target.IncDrwBoxOpening3 = IncDrwBoxOpening3;
        target.IncDrwBoxOpening4 = IncDrwBoxOpening4;
        target.DrillSlideHolesOpening1 = DrillSlideHolesOpening1;
        target.DrillSlideHolesOpening2 = DrillSlideHolesOpening2;
        target.DrillSlideHolesOpening3 = DrillSlideHolesOpening3;
        target.DrillSlideHolesOpening4 = DrillSlideHolesOpening4;
        target.IncDrwBoxInListOpening1 = IncDrwBoxInListOpening1;
        target.IncDrwBoxInListOpening2 = IncDrwBoxInListOpening2;
        target.IncDrwBoxInListOpening3 = IncDrwBoxInListOpening3;
        target.IncDrwBoxInListOpening4 = IncDrwBoxInListOpening4;
        target.DrwFrontHeight1 = ConvertDimension.FractionToDouble(DrwFrontHeight1).ToString();
        target.DrwFrontHeight2 = ConvertDimension.FractionToDouble(DrwFrontHeight2).ToString();
        target.DrwFrontHeight3 = ConvertDimension.FractionToDouble(DrwFrontHeight3).ToString();
        target.DrwFrontHeight4 = ConvertDimension.FractionToDouble(DrwFrontHeight4).ToString();
        target.IncDrwFront1 = IncDrwFront1;
        target.IncDrwFront2 = IncDrwFront2;
        target.IncDrwFront3 = IncDrwFront3;
        target.IncDrwFront4 = IncDrwFront4;
        target.IncDrwFrontInList1 = IncDrwFrontInList1;
        target.IncDrwFrontInList2 = IncDrwFrontInList2;
        target.IncDrwFrontInList3 = IncDrwFrontInList3;
        target.IncDrwFrontInList4 = IncDrwFrontInList4;
        target.LeftReveal = ConvertDimension.FractionToDouble(LeftReveal).ToString();
        target.RightReveal = ConvertDimension.FractionToDouble(RightReveal).ToString();
        target.TopReveal = ConvertDimension.FractionToDouble(TopReveal).ToString();
        target.BottomReveal = ConvertDimension.FractionToDouble(BottomReveal).ToString();
        target.GapWidth = ConvertDimension.FractionToDouble(GapWidth).ToString();
        target.IncRollouts = IncRollouts;
        target.IncRolloutsInList = IncRolloutsInList;
        target.RolloutCount = RolloutCount;
        target.RolloutStyle = RolloutStyle;
        target.DrillSlideHolesForRollouts = DrillSlideHolesForRollouts;
        target.SinkCabinet = SinkCabinet;
        target.TrashDrawer = TrashDrawer;
        target.EqualizeAllDrwFronts = EqualizeAllDrwFronts;
        target.EqualizeBottomDrwFronts = EqualizeBottomDrwFronts;
    }

    [RelayCommand]
    private void AddCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
            return;

        EnforceStyleConstraints();

        string tempTopType = TopType; // User-selected top type, which may be overridden by depth-specific rules below
        EnforceTopTypeForShallowDepth();

        var newCabinet = new BaseCabinetModel();
        ApplyViewModelToModel(newCabinet);

        try
        {
            _cabinetService?.Add(newCabinet);  // Adds to shared list as base type
            _mainVm!.SelectedCabinet = newCabinet;
        }
        catch (InvalidOperationException ex)
        {
            _mainVm?.Notify(ex.Message, Brushes.Red, 3000);
            return;
        }

        Notes = ""; // Clear notes field after adding, since it can contain cabinet-specific info that shouldn't be copied to next cabinet

        TopType = tempTopType; // Restore user's top type choice after forcing a full top for shallow depths

        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;
    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel selected)
        {
            if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
                return;

            if (!ViewModelValidationHelper.ValidateUniqueName(Name, selected, _cabinetService, _mainVm))
                return;

            EnforceStyleConstraints();

            string tempTopType = TopType; // User-selected top type, which may be overridden by depth-specific rules below
            EnforceTopTypeForShallowDepth();

            ApplyViewModelToModel(selected);

            _mainVm?.Notify("Cabinet Updated", Brushes.Green);
            _mainVm?.IsModified = true;

            TopType = tempTopType; // Restore user's top type choice after enforcing depth-specific rules
        }

        else
        {
            // No cabinet selected or wrong type
            _mainVm?.Notify("No cabinet selected, or incorrect cabinet tab selected. Nothing updated.", Brushes.Red, 3000);
            return;
        }

        // Optional: clear selection after update
        _mainVm!.SelectedCabinet = null;

        Notes = ""; // Clear notes field after adding, since it can contain cabinet-specific info that shouldn't be copied to next cabinet

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

        // Suppress intermediate resize calls while batch-setting defaults.
        // Many of these properties (HasTK, TKHeight, DrwCount, EqualizeAll/Bottom,
        // DrwFrontHeight*, reveals, GapWidth) fire changed handlers that call
        // ResizeOpeningHeights / ResizeDrwFrontHeights against partially-updated state.
        _isResizing = true;
        try
        {
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
            if (_defaults.DefaultDimensionFormat == "Decimal") { BackThickness = _defaults.DefaultBaseBackThickness; }
            else { BackThickness = ConvertDimension.DoubleToFraction(Convert.ToDouble(_defaults.DefaultBaseBackThickness)); }
            //BackThickness = _defaults.DefaultBaseBackThickness;
            TopType = _defaults.DefaultTopType;
            ShelfCount = _defaults.DefaultShelfCount;
            ShelfDepth = _defaults.DefaultShelfDepth;
            DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
            DrwFrontGrainDir = _defaults.DefaultDrwGrainDir;
            IncDrwFrontsInList = _defaults.DefaultIncDrwFrontsInList;
            IncDrwFronts = _defaults.DefaultIncDrwFronts;
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

            IncDrwBoxesInList = _defaults.DefaultIncDrwBoxesInList;
            IncDrwBoxes = _defaults.DefaultIncDrwBoxes;
            IncRollouts = _defaults.DefaultIncDrwBoxes;
            DrillSlideHoles = _defaults.DefaultDrillSlideHoles;
            if (Style == Style1) { DrwCount = _defaults.DefaultStdDrawerCount; }
            if (Style == Style2) { DrwCount = _defaults.DefaultDrawerStackDrawerCount; }
            DrwStyle = _defaults.DefaultDrwStyle;
            RolloutStyle = _defaults.DefaultDrwStyle;
            EqualizeAllDrwFronts = _defaults.DefaultEqualizeAllDrwFronts;
            EqualizeBottomDrwFronts = _defaults.DefaultEqualizeBottomDrwFronts;

            DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
            DrwFrontHeight2 = _defaults.DefaultDrwFrontHeight2;
            DrwFrontHeight3 = _defaults.DefaultDrwFrontHeight3;


            if (_defaults.DefaultEqualizeBottomDrwFronts && Style == Style2)
            {
                DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                DrwFrontHeight2 = "7"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                DrwFrontHeight3 = "7";
            }
            if (_defaults.DefaultEqualizeAllDrwFronts && Style == Style2)
            {
                DrwFrontHeight1 = "3"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                DrwFrontHeight2 = "3";
                DrwFrontHeight3 = "3";
                DrwFrontHeight4 = "3";
            }

            LeftReveal = _defaults.DefaultBaseLeftReveal;
            RightReveal = _defaults.DefaultBaseRightReveal;
            TopReveal = _defaults.DefaultBaseTopReveal;
            BottomReveal = _defaults.DefaultBaseBottomReveal;
            GapWidth = _defaults.DefaultGapWidth;
            SinkCabinet = false;
            TrashDrawer = false;
        }
        finally
        {
            _isResizing = false;
        }

        ApplyDrawerFrontEqualization();
        ResizeDrwFrontHeights();
        DrwFrontHeight1Edit = DrwFrontHeight1;
    }

    [RelayCommand]
    private void LoadDefaultDrwSettings()
    {
        if (_defaults is null) return;

        // Suppress intermediate resize calls while batch-setting defaults.
        // Preserve the outer _isResizing state so we don't break a parent
        // batch (e.g. LoadDefaults) that also set _isResizing = true.
        bool wasResizing = _isResizing;
        _isResizing = true;
        try
        {
            EqualizeAllDrwFronts = _defaults.DefaultEqualizeAllDrwFronts;
            EqualizeBottomDrwFronts = _defaults.DefaultEqualizeBottomDrwFronts;

            DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
            DrwFrontHeight2 = _defaults.DefaultDrwFrontHeight2;
            DrwFrontHeight3 = _defaults.DefaultDrwFrontHeight3;


            if (_defaults.DefaultEqualizeBottomDrwFronts && Style == Style2)
            {
                DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                DrwFrontHeight2 = "7"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                DrwFrontHeight3 = "7";
            }
            if (_defaults.DefaultEqualizeAllDrwFronts && Style == Style2)
            {
                DrwFrontHeight1 = "3"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                DrwFrontHeight2 = "3";
                DrwFrontHeight3 = "3";
                DrwFrontHeight4 = "3";
            }
        }
        finally
        {
            _isResizing = wasResizing;
        }

        ApplyDrawerFrontEqualization();
        ResizeDrwFrontHeights();
    }

    // For 3D model:
    private void UpdatePreview() // Update 3D cabinet model preview
    {
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
            GapWidth = GapWidth,
            IncRollouts = IncRollouts,
            RolloutCount = RolloutCount,
            RolloutStyle = RolloutStyle,
            DrwStyle = DrwStyle,
            SinkCabinet = SinkCabinet,
            TrashDrawer = TrashDrawer,
            DrillHingeHoles = DrillHingeHoles,
            DrillShelfHoles = DrillShelfHoles,
            DrillSlideHolesOpening1 = DrillSlideHolesOpening1,
            DrillSlideHolesOpening2 = DrillSlideHolesOpening2,
            DrillSlideHolesOpening3 = DrillSlideHolesOpening3,
            DrillSlideHolesOpening4 = DrillSlideHolesOpening4
        };

        // Request preview using the tab index owner token (Base tab = 0)
        _previewService?.RequestPreview(0, model);
    }


    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Width","Height","Depth","TKHeight","TKDepth",
        "LeftBackWidth","RightBackWidth","LeftFrontWidth","RightFrontWidth",
        "LeftDepth","RightDepth","BackThickness", "FrontWidth",
        "OpeningHeight1","OpeningHeight2","OpeningHeight3","OpeningHeight4",
        "DrwFrontHeight1","DrwFrontHeight2","DrwFrontHeight3","DrwFrontHeight4",
        "LeftReveal","RightReveal","TopReveal","BottomReveal","GapWidth"
    };

    // Properties that affect 3D preview geometry — only these trigger UpdatePreview
    private static readonly HashSet<string> s_previewProperties = new(StringComparer.Ordinal)
    {
        nameof(Style), nameof(Width), nameof(Height), nameof(Depth),
        nameof(Species), nameof(EBSpecies),
        nameof(TKHeight), nameof(TKDepth), nameof(HasTK),
        nameof(LeftBackWidth), nameof(RightBackWidth),
        nameof(LeftFrontWidth), nameof(RightFrontWidth),
        nameof(LeftDepth), nameof(RightDepth),
        nameof(DoorSpecies), nameof(BackThickness), nameof(TopType),
        nameof(ShelfCount), nameof(ShelfDepth),
        nameof(DoorCount), nameof(DoorGrainDir),
        nameof(IncDoors), nameof(IncDrwFronts), nameof(IncDrwBoxes),
        nameof(DrwCount), nameof(DrwFrontGrainDir), nameof(DrwStyle),
        nameof(OpeningHeight1), nameof(OpeningHeight2), nameof(OpeningHeight3), nameof(OpeningHeight4),
        nameof(IncDrwBoxOpening1), nameof(IncDrwBoxOpening2), nameof(IncDrwBoxOpening3), nameof(IncDrwBoxOpening4),
        nameof(DrwFrontHeight1), nameof(DrwFrontHeight2), nameof(DrwFrontHeight3), nameof(DrwFrontHeight4),
        nameof(IncDrwFront1), nameof(IncDrwFront2), nameof(IncDrwFront3), nameof(IncDrwFront4),
        nameof(LeftReveal), nameof(RightReveal), nameof(TopReveal), nameof(BottomReveal), nameof(GapWidth),
        nameof(IncRollouts), nameof(RolloutCount), nameof(RolloutStyle),
        nameof(SinkCabinet), nameof(TrashDrawer),
        nameof(DrillHingeHoles), nameof(DrillShelfHoles),
        nameof(DrillSlideHolesOpening1), nameof(DrillSlideHolesOpening2),
        nameof(DrillSlideHolesOpening3), nameof(DrillSlideHolesOpening4),
    };


    private void MapModelToViewModel(BaseCabinetModel model, string dimFormat)
    {
        if (model is null) return;

        _isMapping = true;
        try
        {
            ViewModelMappingHelper.MapModelToViewModel(this, model, dimFormat, s_dimensionProperties);
        }
        finally
        {
            _isMapping = false;

            // Only update visibility/state — do NOT recalculate values.
            // The model's values are authoritative after mapping.
            ApplyStyleVisibility(model.Style);
            RunValidationVisible();
        }
    }

    private readonly System.Timers.Timer _drwFrontHeight1DebounceTimer = new(90) { AutoReset = false };
    private bool _suppressEditSync;
    private bool _isEditingDrwFrontHeight1;

    [ObservableProperty]
    public partial string DrwFrontHeight1Edit { get; set; } = "";

    partial void OnDrwFrontHeight1EditChanged(string oldValue, string newValue)
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

    /// <summary>
    /// Sets visibility/state/list properties that depend on the cabinet style.
    /// Does NOT modify any dimension values or trigger recalculation.
    /// </summary>
    private void ApplyStyleVisibility(string style)
    {
        StdOrDrwBaseVisibility = (style == Style1 || style == Style2);
        Corner90Visibility = (style == Style3);
        Corner45Visibility = (style == Style4);
        GroupShelvesVisibility = (style == Style1 || style == Style3 || style == Style4);
        ComboShelfDepthEnabled = (style == Style1 || style == Style3);
        GroupDrawersVisibility = (style == Style1 || style == Style2);
        GroupCabinetTopTypeVisibility = (style == Style1 || style == Style2);
        GroupDrawerFrontHeightsVisibility = (style == Style1 || style == Style2);
        GroupDoorsVisibility = (style == Style1 || style == Style3 || style == Style4);
        BackThicknessVisible = (style == Style1 || style == Style2);
        GroupRolloutsVisible = (style == Style1 && !TrashDrawer);
        TrashDrawerEnabled = (style == Style1);
        SinkCabinetEnabled = (style == Style1 || style == Style3 || style == Style4);
        if (style == Style2)
            ListDrwCount = [1, 2, 3, 4];
        else if (style == Style1)
            ListDrwCount = [0, 1];

        // DrwCount-dependent visibility
        DrawersStackPanelVisible = DrwCount > 0;
        DrwFront1Visible = DrwCount >= 1;
        DrwFront2Visible = DrwCount >= 2;
        DrwFront3Visible = DrwCount >= 3;
        DrwFront4Visible = DrwCount == 4;
        Opening1Visible = DrwCount >= 1;
        Opening2Visible = DrwCount >= 2;
        Opening3Visible = DrwCount >= 3;
        Opening4Visible = DrwCount == 4;

        // RolloutCount-dependent visibility
        IncRolloutsVisible = RolloutCount > 0;
        IncRolloutsInListVisible = RolloutCount > 0;
        RolloutStyleVisible = RolloutCount > 0;
        DrillSlideHolesForRolloutsVisible = RolloutCount > 0;

        // DoorCount-dependent visibility
        DoorGrainDirVisible = DoorCount > 0;
        IncDoorsInListVisible = DoorCount > 0;
        DrillHingeHolesVisible = DoorCount > 0;
        SupplySlabDoorsVisible = DoorCount > 0;

        // Trash Drawer dependent visibility
        GroupRolloutsVisible = (style == Style1);
    }
}









