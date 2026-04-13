using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class FillerViewModel : ObservableValidator
{
    public FillerViewModel()
    {
        // empty constructor for design-time support
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

    public FillerViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults, IMaterialLookupService lookups, IPreviewService previewService)
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

        Width = "4";
        Height = "34.5";
        Depth = "24";
        LoadDefaults();
        ValidateAllProperties();
    }

    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
        {
            LoadSelectedIfMine();
        }
    }


    // Common properties from CabinetModel
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(3, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 120)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(4, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = ""; partial void OnSpeciesChanged(string oldValue, string newValue)
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
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string CustomEBSpecies { get; set; } = "";

    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1, 100)] public partial int Qty { get; set; } = 1;
    [ObservableProperty] public partial string Notes { get; set; } = "";


    [ObservableProperty] public partial bool CustomCabSpeciesEnabled { get; set; } = false;

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is FillerModel filler)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(filler, dimFormat);

            // Any additional logic that must run after loading (visibility, resize, preview)
            UpdatePreview();
        }
        else
        {

        }
    }


    // Properties that affect 3D preview geometry — only these trigger UpdatePreview
    private static readonly HashSet<string> s_previewProperties = new(StringComparer.Ordinal)
    {
        nameof(Width), nameof(Height), nameof(Depth),
        nameof(Species), nameof(EBSpecies),
    };

    /// <summary>
    /// Copies all current ViewModel property values into the target model,
    /// converting dimension strings to numeric format.
    /// </summary>
    private void ApplyViewModelToModel(FillerModel target)
    {
        target.Width = ConvertDimension.FractionToDouble(Width).ToString();
        target.Height = ConvertDimension.FractionToDouble(Height).ToString();
        target.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
        target.Species = Species;
        target.CustomSpecies = CustomSpecies;
        target.EBSpecies = CabinetBuildHelpers.GetMatchingEdgebandingSpecies(Species);
        target.CustomEBSpecies = CustomSpecies;
        target.Name = Name;
        target.Qty = Qty;
        target.Notes = Notes;
    }

    [RelayCommand]
    private void AddCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, "", null))
            return;

        var newCabinet = new FillerModel();
        ApplyViewModelToModel(newCabinet);

        try
        {
            _cabinetService?.Add(newCabinet);
            _mainVm!.SelectedCabinet = newCabinet;
        }
        catch (InvalidOperationException ex)
        {
            _mainVm?.Notify(ex.Message, Brushes.Red, 3000);
            return;
        }

        Notes = "";

        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;
    }


    [RelayCommand]
    private void UpdateCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, "", null))
            return;

        if (_mainVm!.SelectedCabinet is FillerModel selected)
        {
            if (!ViewModelValidationHelper.ValidateUniqueName(Name, selected, _cabinetService, _mainVm))
                return;

            ApplyViewModelToModel(selected);

            _mainVm?.Notify("Cabinet Updated", Brushes.Green);
            _mainVm.IsModified = true;
        }
        else
        {
            _mainVm?.Notify("No cabinet selected, or incorrect cabinet tab selected. Nothing updated.", Brushes.Red, 3000);
            return;
        }

        _mainVm!.SelectedCabinet = null;

        Notes = "";
    }

    [RelayCommand]
    private void LoadDefaults()
    {
        Species = _defaults!.DefaultFillerSpecies;
        // etc.
    }


    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Width","Height","Depth"
    };


    private void MapModelToViewModel(FillerModel model, string dimFormat)
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
        }
    }

    private void UpdatePreview()
    {
        var model = new FillerModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
        };

        _previewService?.RequestPreview(2, model);
    }
}