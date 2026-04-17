using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    public UpperCabinetViewModel()
    {
        // empty constructor for design-time support
        // Provide a simple concrete lookup service so design-time and parameterless scenarios
        // don't hit a null reference when binding to the collections.
        _lookups = new MaterialLookupService();
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private readonly IPreviewService? _previewService;
    private bool _isMapping;

    private readonly IMaterialLookupService _lookups;
    public ObservableCollection<string> ListCabSpecies => _lookups.CabinetSpecies;
    public ObservableCollection<string> ListEBSpecies => _lookups.EBSpecies;

    public UpperCabinetViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults, IMaterialLookupService lookups, IPreviewService previewService)
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
        Style = Style1; // Default style
        Width = "16";
        Height = "42";
        Depth = "12";
        LeftFrontWidth = "12";
        RightFrontWidth = "12";
        LeftDepth = "12";
        RightDepth = "12";
        LeftBackWidth = "24";
        RightBackWidth = "24";

        LoadDefaults();
        ValidateAllProperties();

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


    // Upper cabinet type strings
    public static string Style1 => CabinetStyles.Upper.Standard;
    public static string Style2 => CabinetStyles.Upper.Corner90;
    public static string Style3 => CabinetStyles.Upper.AngleFront;

    // Common properties from CabinetModel
    [ObservableProperty] public partial string Style { get; set; } = ""; partial void OnStyleChanged(string value)
    {
        if (_isMapping) return;
        ApplyStyleVisibility(value);
        RunValidationVisible();
    }
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = MaterialDefaults.Thickness34;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = MaterialDefaults.Thickness14;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(6, 60)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 120)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = ""; partial void OnSpeciesChanged(string oldValue, string newValue)
    {
        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial string CustomSpecies { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = ""; partial void OnEBSpeciesChanged(string oldValue, string newValue)
    {
        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial string CustomEBSpecies { get; set; } = "";

    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1, 100)] public partial int Qty { get; set; } = 1;
    [ObservableProperty] public partial string Notes { get; set; } = "";


    // Corner Cab specific properties
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

    [ObservableProperty] public partial string DoorSpecies { get; set; } = ""; partial void OnDoorSpeciesChanged(string oldValue, string newValue)
    {
        ApplyStyleVisibility(Style);
    }
    [ObservableProperty] public partial string CustomDoorSpecies { get; set; } = "";

    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BackThickness { get; set; } = ""; partial void OnBackThicknessChanged(string oldValue, string newValue)
    {
        if (_isMapping) return;

        if (newValue != oldValue)
        {
            RunValidationVisible();
        }
    }

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

    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }

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
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial bool EdgebandDoorsAndDrawers { get; set; } = true;

    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string RightReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string TopReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string GapWidth { get; set; } = "";

    // Combobox options
    public IReadOnlyList<int> ComboShelfCount => CabinetOptions.ShelfCounts;
    public static List<string> TypeList => [Style1, Style2, Style3];
    public IReadOnlyList<int> ListDoorCount => CabinetOptions.DoorCounts;
    public IReadOnlyList<string> ListGrainDirection => CabinetOptions.GrainDirections;

    public List<string> ListBackThickness =>
        CabinetOptions.BackThickness.GetList(_defaults?.DefaultDimensionFormat ?? "Decimal");



    // Visibility properties
    [ObservableProperty] public partial bool StandardDimsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool Corner90DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool Corner45DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool ShowRevealSettings { get; set; } = true;
    [ObservableProperty] public partial bool BackThicknessVisible { get; set; } = true;
    [ObservableProperty] public partial bool CustomCabSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool CustomEBSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool CustomDoorSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool DoorGrainDirVisible { get; set; } = true;
    [ObservableProperty] public partial bool IncDoorsInListVisible { get; set; } = true;
    [ObservableProperty] public partial bool DrillHingeHolesVisible { get; set; } = true;
    [ObservableProperty] public partial bool SupplySlabDoorsVisible { get; set; } = true;
}