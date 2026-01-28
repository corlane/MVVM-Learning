using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;


namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PanelViewModel : ObservableValidator
{
    public PanelViewModel()
    {
        // empty constructor for design-time support
    }

    // Example: BaseCabinetViewModel.cs (copy to all input VMs)

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private bool _isMapping;

    public PanelViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;
        _defaults = defaults;

        // Subscribe to ALL property changes in this ViewModel
        this.PropertyChanged += (_, e) =>
        {
            // keep preview updated
            UpdatePreview();

            // when material-thickness properties change, update the list so bound ComboBox refreshes
            if (e.PropertyName == nameof(MaterialThickness14) || e.PropertyName == nameof(MaterialThickness34))
            {
                OnPropertyChanged(nameof(ListPanelDepths));
            }
        };

        // react when DefaultDimensionFormat changes so ListPanelDepths updates
        if (_defaults != null)
        {
            _defaults.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
                {
                    OnPropertyChanged(nameof(ListPanelDepths));
                }
            };
        }

        _mainVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
                LoadSelectedIfMine();
        };

        Width = "16";
        Height = "32";
        Depth = "0.75";
        LoadDefaults();
        ValidateAllProperties();
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


    // Combo box lists
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
            //LoadDefaults();
        }
    }


    [RelayCommand]
    private void AddCabinet()
    {
        if (Species == "Custom" && string.IsNullOrWhiteSpace(CustomSpecies))
        {
            // Prompt for custom species
            MessageBox.Show("Please enter a custom species name.", "Custom Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (EBSpecies == "Custom" && string.IsNullOrWhiteSpace(CustomEBSpecies))
        {
            // Prompt for custom edge band species
            MessageBox.Show("Please enter a custom edgebanding species name.", "Custom Edge Band Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }


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

        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;

    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (Species == "Custom" && string.IsNullOrWhiteSpace(CustomSpecies))
        {
            // Prompt for custom species
            MessageBox.Show("Please enter a custom species name.", "Custom Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (EBSpecies == "Custom" && string.IsNullOrWhiteSpace(CustomEBSpecies))
        {
            // Prompt for custom edge band species
            MessageBox.Show("Please enter a custom edgebanding species name.", "Custom Edge Band Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }


        if (_mainVm!.SelectedCabinet is PanelModel selected)
        {
            var newName = Name;

            if (!string.IsNullOrWhiteSpace(newName))
            {
                var normalized = newName.Trim();

                bool dup = _cabinetService?.Cabinets.Any(c =>
                    !ReferenceEquals(c, selected) &&
                    !string.IsNullOrWhiteSpace(c.Name) &&
                    string.Equals(c.Name.Trim(), normalized, StringComparison.OrdinalIgnoreCase)) == true;

                if (dup)
                {
                    _mainVm?.Notify("Duplicate cabinet names are not allowed.", Brushes.Red, 3000);
                    return;
                }
            }

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
            var vmType = GetType();
            var modelType = model.GetType();

            // iterate model public instance properties and copy to VM where names match
            foreach (var modelProp in modelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var vmProp = vmType.GetProperty(modelProp.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (vmProp is null || !vmProp.CanWrite) continue;

                var modelValue = modelProp.GetValue(model);
                if (modelValue is null)
                {
                    vmProp.SetValue(this, null);
                    continue;
                }

                // string properties: either dimension-formatted or direct copy
                if (vmProp.PropertyType == typeof(string))
                {
                    var raw = modelValue.ToString() ?? "";

                    if (s_dimensionProperties.Contains(modelProp.Name))
                    {
                        double numeric = ConvertDimension.FractionToDouble(raw);

                        if (string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase))
                        {
                            vmProp.SetValue(this, ConvertDimension.DoubleToFraction(numeric));
                        }
                        else
                        {
                            vmProp.SetValue(this, numeric.ToString());
                        }
                    }
                    else
                    {
                        vmProp.SetValue(this, raw);
                    }
                }
                else if (vmProp.PropertyType == typeof(int))
                {
                    if (modelValue is int i) vmProp.SetValue(this, i);
                    else if (int.TryParse(modelValue.ToString(), out var v)) vmProp.SetValue(this, v);
                }
                else if (vmProp.PropertyType == typeof(bool))
                {
                    if (modelValue is bool b) vmProp.SetValue(this, b);
                    else if (bool.TryParse(modelValue.ToString(), out var vb)) vmProp.SetValue(this, vb);
                }
                else
                {
                    vmProp.SetValue(this, modelValue);
                }
            }
        }
        finally
        {
            _isMapping = false;
        }
    }

    private void UpdatePreview()
    {
        //_mainVm.CurrentPreviewCabinet = new PanelModel -- Original before Preview Service
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();

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
        // Request preview using the tab index owner token (Panel tab = 3)
        previewSvc.RequestPreview(3, model);
    }

}