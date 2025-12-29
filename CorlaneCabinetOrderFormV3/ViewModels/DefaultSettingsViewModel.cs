using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Themes;
using System.Diagnostics;

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

        // Initialize SelectedTheme from persisted default (falls back to Light Theme)
        if (_defaults != null)
        {
            SelectedTheme = string.IsNullOrWhiteSpace(_defaults.DefaultTheme)
                ? "Light Theme"
                : _defaults.DefaultTheme;
        }

        // Listen for changes on the underlying defaults so computed/display properties update.
        if (_defaults != null)
        {
            _defaults.PropertyChanged += Defaults_PropertyChanged;
        }
    }


    private readonly DefaultSettingsService? _defaults;

    
    // Mirror every default as a bindable property

    // Dimension Format
    public string DefaultDimensionFormat { get => _defaults.DefaultDimensionFormat; set => _defaults.DefaultDimensionFormat = value; }

    // Species
    public string DefaultSpecies { get => _defaults.DefaultSpecies; set => _defaults.DefaultSpecies = value; }
    public string DefaultEBSpecies { get => _defaults.DefaultEBSpecies; set => _defaults.DefaultEBSpecies = value; }

    //Top
    public string DefaultTopType { get => _defaults.DefaultTopType; set => _defaults.DefaultTopType = value; }

    // Back
    // Back - store in defaults as canonical numeric string, but expose formatted strings for UI.
    public string DefaultBaseBackThickness
    {
        get
        {
            if (_defaults == null) return "0.75";
            double numeric = ConvertDimension.FractionToDouble(_defaults.DefaultBaseBackThickness);
            return string.Equals(_defaults.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase) ? ConvertDimension.DoubleToFraction(numeric) : numeric.ToString();
        }
        set
        {
            if (_defaults == null) return;
            double numeric = ConvertDimension.FractionToDouble(value);
            _defaults.DefaultBaseBackThickness = numeric.ToString();
            OnPropertyChanged(nameof(DefaultBaseBackThickness));
        }
    }
    public string DefaultUpperBackThickness
    {
        get
        {
            if (_defaults == null) return "0.75";
            double numeric = ConvertDimension.FractionToDouble(_defaults.DefaultUpperBackThickness);
            return string.Equals(_defaults.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                ? ConvertDimension.DoubleToFraction(numeric)
                : numeric.ToString();
        }
        set
        {
            if (_defaults == null) return;
            double numeric = ConvertDimension.FractionToDouble(value);
            _defaults.DefaultUpperBackThickness = numeric.ToString();
            OnPropertyChanged(nameof(DefaultUpperBackThickness));
        }
    }

    //Toekick
    public bool DefaultHasTK { get => _defaults.DefaultHasTK; set => _defaults.DefaultHasTK = value; }
    public string DefaultTKHeight { get => _defaults.DefaultTKHeight; set => _defaults.DefaultTKHeight = value; }
    public string DefaultTKDepth { get => _defaults.DefaultTKDepth; set => _defaults.DefaultTKDepth = value; }

    // Shelves
    public int DefaultShelfCount { get => _defaults.DefaultShelfCount; set => _defaults.DefaultShelfCount = value; }
    public string DefaultShelfDepth { get => _defaults.DefaultShelfDepth; set => _defaults.DefaultShelfDepth = value; }
    public bool DefaultDrillShelfHoles { get => _defaults.DefaultDrillShelfHoles; set => _defaults.DefaultDrillShelfHoles = value; }

    // Openings
    public string DefaultOpeningHeight1 { get => _defaults.DefaultOpeningHeight1; set => _defaults.DefaultOpeningHeight1 = value; }
    public string DefaultOpeningHeight2 { get => _defaults.DefaultOpeningHeight2; set => _defaults.DefaultOpeningHeight2 = value; }
    public string DefaultOpeningHeight3 { get => _defaults.DefaultOpeningHeight3; set => _defaults.DefaultOpeningHeight3 = value; }


    // Doors
    public string DefaultDoorDrwSpecies { get => _defaults.DefaultDoorDrwSpecies; set => _defaults.DefaultDoorDrwSpecies = value; }
    public int DefaultDoorCount { get => _defaults.DefaultDoorCount; set => _defaults.DefaultDoorCount = value; }
    public bool DefaultDrillHingeHoles { get => _defaults.DefaultDrillHingeHoles; set => _defaults.DefaultDrillHingeHoles = value; }
    public string DefaultDoorGrainDir { get => _defaults.DefaultDoorGrainDir; set => _defaults.DefaultDoorGrainDir = value; }
    public bool DefaultIncDoorsInList { get => _defaults.DefaultIncDoorsInList; set => _defaults.DefaultIncDoorsInList = value; }
    public bool DefaultIncDoors { get => _defaults.DefaultIncDoors; set => _defaults.DefaultIncDoors = value; }

    // Drawers
    public int DefaultStdDrawerCount { get => _defaults.DefaultStdDrawerCount; set => _defaults.DefaultStdDrawerCount = value; }
    public int DefaultDrawerStackDrawerCount { get => _defaults.DefaultDrawerStackDrawerCount; set => _defaults.DefaultDrawerStackDrawerCount = value; }
    public string DefaultDrwStyle { get => _defaults.DefaultDrwStyle; set => _defaults.DefaultDrwStyle = value; }
    public string DefaultDrwGrainDir { get => _defaults.DefaultDrwGrainDir; set => _defaults.DefaultDrwGrainDir = value; }
    public bool DefaultIncDrwFrontsInList { get => _defaults.DefaultIncDrwFrontsInList; set => _defaults.DefaultIncDrwFrontsInList = value; }
    public bool DefaultIncDrwFronts { get => _defaults.DefaultIncDrwFronts; set => _defaults.DefaultIncDrwFronts = value; }
    public bool DefaultIncDrwBoxesInList { get => _defaults.DefaultIncDrwBoxesInList; set => _defaults.DefaultIncDrwBoxesInList = value; }
    public bool DefaultIncDrwBoxes { get => _defaults.DefaultIncDrwBoxes; set => _defaults.DefaultIncDrwBoxes = value; }
    public bool DefaultDrillSlideHoles { get => _defaults.DefaultDrillSlideHoles; set => _defaults.DefaultDrillSlideHoles = value; }
    public string DefaultDrwFrontHeight1 { get => _defaults.DefaultDrwFrontHeight1; set => _defaults.DefaultDrwFrontHeight1 = value; }
    public string DefaultDrwFrontHeight2 { get => _defaults.DefaultDrwFrontHeight2; set => _defaults.DefaultDrwFrontHeight2 = value; }
    public string DefaultDrwFrontHeight3 { get => _defaults.DefaultDrwFrontHeight3; set => _defaults.DefaultDrwFrontHeight3 = value; }

    // Reveals and Gaps
    public string DefaulBasetLeftReveal { get => _defaults.DefaultBaseLeftReveal; set => _defaults.DefaultBaseLeftReveal = value; }
    public string DefaultBaseRightReveal { get => _defaults.DefaultBaseRightReveal; set => _defaults.DefaultBaseRightReveal = value; }
    public string DefaultBaseTopReveal { get => _defaults.DefaultBaseTopReveal; set => _defaults.DefaultBaseTopReveal = value; }
    public string DefaultBaseBottomReveal { get => _defaults.DefaultBaseBottomReveal; set => _defaults.DefaultBaseBottomReveal = value; }

    public string DefaultUpperLeftReveal { get => _defaults.DefaultUpperLeftReveal; set => _defaults.DefaultUpperLeftReveal = value; }
    public string DefaultUpperRightReveal { get => _defaults.DefaultUpperRightReveal; set => _defaults.DefaultUpperRightReveal = value; }
    public string DefaultUpperTopReveal { get => _defaults.DefaultUpperTopReveal; set => _defaults.DefaultUpperTopReveal = value; }
    public string DefaultUpperBottomReveal { get => _defaults.DefaultUpperBottomReveal; set => _defaults.DefaultUpperBottomReveal = value; }

    public string DefaultGapWidth { get => _defaults.DefaultGapWidth; set => _defaults.DefaultGapWidth = value; }

    public string DefaultTheme { get => _defaults.DefaultTheme; set => _defaults.DefaultTheme = value; }
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
    public List<string> ListBackThickness
    {
        get
        {
            var format = _defaults?.DefaultDimensionFormat ?? "Decimal";
            bool useFraction = string.Equals(format, "Fraction", StringComparison.OrdinalIgnoreCase);

            string thin = useFraction
                ? ConvertDimension.DoubleToFraction(0.25)
                : 0.25.ToString();

            string thick = useFraction
                ? ConvertDimension.DoubleToFraction(0.75)
                : 0.75.ToString();
            
            return new List<string> { thin, thick };
        }
    }
    public List<string> ListTopType { get; } =
        [
            "Stretcher",
            "Full"
        ];
    public List<string> ThemeOptions { get; } = new()
    {
        "Deep Dark",
        "Soft Dark",
        "Dark Grey Theme",
        "Grey Theme",
        "Light Theme",
        "Red Black Theme"
    };

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = "Light Theme";
    partial void OnSelectedThemeChanged(string value)
    {
        ThemeType selectedTheme = value switch
        {
            "Soft Dark" => ThemeType.SoftDark,
            "Red Black Theme" => ThemeType.RedBlackTheme,
            "Deep Dark" => ThemeType.DeepDark,
            "Grey Theme" => ThemeType.GreyTheme,
            "Dark Grey Theme" => ThemeType.DarkGreyTheme,
            "Light Theme" => ThemeType.LightTheme,
            _ => ThemeType.LightTheme
        };
        ThemesController.SetTheme(selectedTheme);

        // Persist selection to defaults and save
        if (_defaults != null)
        {
            _defaults.DefaultTheme = value;
            _ = _defaults.SaveAsync(); // fire-and-forget; best-effort persist
        }
    }


    // Command to save defaults
    [RelayCommand]
    private async Task SaveDefaults()
    {
        await _defaults.SaveAsync();
        // Optional: show toast/message
    }

    private void Defaults_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // When dimension format changes, recompute display lists and formatted properties
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            OnPropertyChanged(nameof(ListBackThickness));
            OnPropertyChanged(nameof(DefaultBaseBackThickness));
            OnPropertyChanged(nameof(DefaultUpperBackThickness));
        }
        else if (e.PropertyName == nameof(DefaultSettingsService.DefaultBaseBackThickness))
        {
            OnPropertyChanged(nameof(DefaultBaseBackThickness));
        }
        else if (e.PropertyName == nameof(DefaultSettingsService.DefaultUpperBackThickness))
        {
            OnPropertyChanged(nameof(DefaultUpperBackThickness));
        }
    }
}