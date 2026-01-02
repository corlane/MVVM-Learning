using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;


namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class FillerViewModel : ObservableValidator
{
    public FillerViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private bool _isMapping;
    public FillerViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
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

        Width = "4";
        Height = "34.5";
        Depth = "24";
        LoadDefaults();
        ValidateAllProperties();
    }


    // Common properties from CabinetModel
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(3, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1, 100)] public partial int Qty { get; set; } = 1;
    [ObservableProperty] public partial string Notes { get; set; } = "";

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
            //LoadDefaults();
        }
    }


    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new FillerModel
        {
            Width = ConvertDimension.FractionToDouble(Width).ToString(),
            Height = ConvertDimension.FractionToDouble(Height).ToString(),
            Depth = ConvertDimension.FractionToDouble(Depth).ToString(),
            Species = Species,
            EBSpecies = EBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type
        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;
    }


    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm!.SelectedCabinet is FillerModel selected)
        {
            selected.Width = ConvertDimension.FractionToDouble(Width).ToString();
            selected.Height = ConvertDimension.FractionToDouble(Height).ToString();
            selected.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
            selected.Species = Species;
            selected.EBSpecies = EBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;

            _mainVm?.Notify("Cabinet Updated", Brushes.Green);
            _mainVm.IsModified = true;
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
        Species = _defaults!.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        
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
        //_mainVm.CurrentPreviewCabinet = new FillerModel --- Original before Preview Service
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();

        var model = new FillerModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,

            // ... copy EVERY property from fields to the preview model
            // Yes, it's a few lines, but it's the only place — do it once per ViewModel
        };
        // Request preview using the tab index owner token (Filler tab = 2)
        previewSvc.RequestPreview(2, model);
    }

}
