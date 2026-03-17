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
using System.Windows;
using System.Windows.Media;


namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PanelViewModel : ObservableValidator
{
    public PanelViewModel()
    {
        // empty constructor for design-time support
        _lookups = new MaterialLookupService();

    }

    // Example: BaseCabinetViewModel.cs (copy to all input VMs)

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private readonly IPreviewService? _previewService;
    private bool _isMapping;

    private readonly IMaterialLookupService _lookups;
    public ObservableCollection<string> ListCabSpecies => _lookups.CabinetSpecies;
    public ObservableCollection<string> ListEBSpecies => _lookups.EBSpecies;



    public PanelViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults, IMaterialLookupService lookups)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;
        _defaults = defaults;
        _lookups = lookups;
        _previewService = App.ServiceProvider.GetRequiredService<IPreviewService>();

        this.PropertyChanged += (_, e) =>
        {
            // Only rebuild preview when geometry-affecting properties change
            if (e.PropertyName is not null && s_previewProperties.Contains(e.PropertyName))
                UpdatePreview();

            // When material-thickness properties change, update the list so bound ComboBox refreshes
            if (e.PropertyName == nameof(MaterialThickness14) || e.PropertyName == nameof(MaterialThickness34))
                OnPropertyChanged(nameof(ListPanelDepths));
        };

        // React when DefaultDimensionFormat changes so ListPanelDepths updates
        if (_defaults != null)
        {
            PropertyChangedEventManager.AddHandler(
                _defaults,
                Defaults_PropertyChanged,
                nameof(DefaultSettingsService.DefaultDimensionFormat));
        }

        PropertyChangedEventManager.AddHandler(
            _mainVm,
            MainVm_PropertyChanged,
            nameof(MainWindowViewModel.SelectedCabinet));

        Width = "16";
        Height = "32";
        Depth = "0.75";
        LoadDefaults();
        ValidateAllProperties();
    }

    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            OnPropertyChanged(nameof(ListPanelDepths));
        }
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
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(3, 120)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string Depth { get; set; } = "";
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

    [ObservableProperty] public partial string EBSpecies { get; set; } = ""; partial void OnEBSpeciesChanged(string oldValue, string newValue)
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

    // Type-specific properties for PanelModel
    [ObservableProperty] public partial bool PanelEBTop { get; set; }
    [ObservableProperty] public partial bool PanelEBBottom { get; set; }
    [ObservableProperty] public partial bool PanelEBLeft { get; set; }
    [ObservableProperty] public partial bool PanelEBRight { get; set; }


    [ObservableProperty] public partial bool CustomCabSpeciesEnabled { get; set; } = false;
    [ObservableProperty] public partial bool CustomEBSpeciesEnabled { get; set; } = false;

    // ListPanelDepths is now computed from the current default format and the material thickness values.
    public List<string> ListPanelDepths
    {
        get
        {
            var format = _defaults?.DefaultDimensionFormat ?? "Decimal";
            bool useFraction = string.Equals(format, "Fraction", StringComparison.OrdinalIgnoreCase);

            string first = useFraction
                ? ConvertDimension.DoubleToFraction(MaterialThickness14)
                : MaterialThickness14.ToString();

            string second = useFraction
                ? ConvertDimension.DoubleToFraction(MaterialThickness34)
                : MaterialThickness34.ToString();

            return [first, second];
        }
    }

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is PanelModel panel)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(panel, dimFormat);

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
        nameof(PanelEBTop), nameof(PanelEBBottom),
        nameof(PanelEBLeft), nameof(PanelEBRight),
    };


    [RelayCommand]
    private void AddCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies))
            return;

        var newCabinet = new PanelModel
        {
            Width = ConvertDimension.FractionToDouble(Width).ToString(),
            Height = ConvertDimension.FractionToDouble(Height).ToString(),
            Depth = ConvertDimension.FractionToDouble(Depth).ToString(),
            Species = Species,
            CustomSpecies = CustomSpecies,
            EBSpecies = EBSpecies,
            CustomEBSpecies = CustomEBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
            PanelEBTop = PanelEBTop,
            PanelEBBottom = PanelEBBottom,
            PanelEBLeft = PanelEBLeft,
            PanelEBRight = PanelEBRight
        };

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

        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;

    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies))
            return;

        if (_mainVm!.SelectedCabinet is PanelModel selected)
        {
            if (!ViewModelValidationHelper.ValidateUniqueName(Name, selected, _cabinetService, _mainVm))
                return;

            selected.Width = ConvertDimension.FractionToDouble(Width).ToString();
            selected.Height = ConvertDimension.FractionToDouble(Height).ToString();
            selected.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
            selected.Species = Species;
            selected.CustomSpecies = CustomSpecies;
            selected.EBSpecies = EBSpecies;
            selected.CustomEBSpecies = CustomEBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
            selected.PanelEBTop = PanelEBTop;
            selected.PanelEBBottom = PanelEBBottom;
            selected.PanelEBLeft = PanelEBLeft;
            selected.PanelEBRight = PanelEBRight;

            _mainVm?.Notify("Cabinet Updated", Brushes.Green);
            _mainVm?.IsModified = true;

            Notes = ""; // Clear notes field after adding, since it can contain cabinet-specific info that shouldn't be copied to next cabinet
        }

        else
        {
            // No cabinet selected or wrong type
            _mainVm?.Notify("No cabinet selected, or incorrect cabinet tab selected. Nothing updated.", Brushes.Red, 3000);
            return;
        }
        // Optional: clear selection after update
        _mainVm!.SelectedCabinet = null;
    }

    [RelayCommand]
    private void LoadDefaults()
    {
        Species = _defaults!.DefaultPanelSpecies;
        EBSpecies = _defaults.DefaultPanelEBSpecies;
        Depth = _defaults.DefaultPanelThickness;

        // etc.
    }


    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Width","Height","Depth"
    };


    private void MapModelToViewModel(PanelModel model, string dimFormat)
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
        var model = new PanelModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            PanelEBTop = PanelEBTop,
            PanelEBBottom = PanelEBBottom,
            PanelEBLeft = PanelEBLeft,
            PanelEBRight = PanelEBRight
        };

        _previewService?.RequestPreview(3, model);
    }

}