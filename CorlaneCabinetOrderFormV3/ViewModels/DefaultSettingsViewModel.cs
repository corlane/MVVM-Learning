using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Themes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class DefaultSettingsViewModel : ObservableObject
{

    public DefaultSettingsViewModel()
    {
        // empty constructor for design-time support
        _lookups = new MaterialLookupService();
    }

    public DefaultSettingsViewModel(DefaultSettingsService defaults, IMaterialLookupService lookups)
    {
        _defaults = defaults;
        _lookups = lookups;

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
            PropertyChangedEventManager.AddHandler(_defaults, Defaults_PropertyChanged, string.Empty);
        }

        // Initialize VM-level formatted values from the backing defaults so ComboBoxes
        // have matching SelectedItem when the view appears.
        ApplyDefaultsToViewModel();
    }


    private readonly DefaultSettingsService? _defaults;
    private readonly IMaterialLookupService _lookups;

    // Flag used to avoid writing back to _defaults while we are applying values that came from _defaults.
    // This prevents re-entrant PropertyChanged loops and transient mismatches between SelectedItem and ItemsSource.
    private bool _isApplyingDefaults;


    // Mirror every default as a bindable property

    // Dimension Format
    public string DefaultDimensionFormat { get => _defaults.DefaultDimensionFormat; set => _defaults.DefaultDimensionFormat = value; }

    // Species
    public string DefaultSpecies { get => _defaults.DefaultSpecies; set => _defaults.DefaultSpecies = value; }
    public string DefaultEBSpecies { get => _defaults.DefaultEBSpecies; set => _defaults.DefaultEBSpecies = value; }
    public string DefaultFillerSpecies { get => _defaults.DefaultFillerSpecies; set => _defaults.DefaultFillerSpecies = value; }
    public string DefaultPanelSpecies { get => _defaults.DefaultPanelSpecies; set => _defaults.DefaultPanelSpecies = value; }
    public string DefaultPanelEBSpecies { get => _defaults.DefaultPanelEBSpecies; set => _defaults.DefaultPanelEBSpecies     = value; }

    // Panel Thickness - store in defaults as canonical numeric string, but expose formatted strings for UI.

    [ObservableProperty] public partial string DefaultPanelThickness { get; set; } = "0.75"; partial void OnDefaultPanelThicknessChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (_isApplyingDefaults) return;

        if (_defaults != null)
        {
            _defaults.DefaultPanelThickness = ConvertDimension.FractionToDouble(value).ToString();
            _ = _defaults.SaveAsync();
        }
    }

    //Top
    public string DefaultTopType { get => _defaults.DefaultTopType; set => _defaults.DefaultTopType = value; }

    // Back
    // Back - store in defaults as canonical numeric string, but expose formatted strings for UI.

    [ObservableProperty] public partial string DefaultBaseBackThickness { get; set; } = "0.75"; partial void OnDefaultBaseBackThicknessChanged(string value)
    {
        //Debug.WriteLine($"OnDefaultBaseBackThicknessChanged called. _isApplyingDefaults={_isApplyingDefaults}, value='{value}', _defaults.DefaultBaseBackThickness='{_defaults?.DefaultBaseBackThickness}'");

        // If the ComboBox temporarily clears SelectedItem because its ItemsSource changed,
        // the VM setter can receive an empty string. Don't treat that as a user intent to write back.
        if (string.IsNullOrWhiteSpace(value))
        {
            //Debug.WriteLine("Ignoring empty/whitespace DefaultBaseBackThickness change (likely ItemsSource update).");
            return;
        }

        // When we're applying defaults we must not write back into the defaults service
        // otherwise we create re-entrant PropertyChanged loops and transient mismatches.
        if (_isApplyingDefaults) return;

        if (_defaults != null)
        {
            _defaults.DefaultBaseBackThickness = ConvertDimension.FractionToDouble(value).ToString();
            // Persist directly on the service (fire-and-forget) rather than invoking the VM command
            _ = _defaults.SaveAsync();
            //Debug.WriteLine($"Wrote to _defaults.DefaultBaseBackThickness='{_defaults.DefaultBaseBackThickness}'");
        }
    }

    [ObservableProperty] public partial string DefaultUpperBackThickness { get; set; } = "0.75"; partial void OnDefaultUpperBackThicknessChanged(string value)
    {
        //Debug.WriteLine($"OnDefaultUpperBackThicknessChanged called. _isApplyingDefaults={_isApplyingDefaults}, value='{value}', _defaults.DefaultUpperBackThickness='{_defaults?.DefaultUpperBackThickness}'");

        // Same guard for the upper thickness: ignore transient empty changes from ItemsSource swap.
        if (string.IsNullOrWhiteSpace(value))
        {
            //Debug.WriteLine("Ignoring empty/whitespace DefaultUpperBackThickness change (likely ItemsSource update).");
            return;
        }

        if (_isApplyingDefaults) return;

        if (_defaults != null)
        {
            _defaults.DefaultUpperBackThickness = ConvertDimension.FractionToDouble(value).ToString();
            _ = _defaults.SaveAsync();
            //Debug.WriteLine($"Wrote to _defaults.DefaultUpperBackThickness='{_defaults.DefaultUpperBackThickness}'");
        }
    }

    //Toekick
    public bool DefaultHasTK { get => _defaults.DefaultHasTK; set => _defaults.DefaultHasTK = value; }
    public string DefaultTKHeight { get => _defaults.DefaultTKHeight; set => _defaults.DefaultTKHeight = value; }
    public string DefaultTKDepth { get => _defaults.DefaultTKDepth; set => _defaults.DefaultTKDepth = value; }

    // Shelves
    public int DefaultShelfCount { get => _defaults.DefaultShelfCount; set => _defaults.DefaultShelfCount = value; }
    public int DefaultUpperShelfCount { get => _defaults.DefaultUpperShelfCount; set => _defaults.DefaultUpperShelfCount     = value; }

    public string DefaultShelfDepth { get => _defaults.DefaultShelfDepth; set => _defaults.DefaultShelfDepth = value; }
    public bool DefaultDrillShelfHoles { get => _defaults.DefaultDrillShelfHoles; set => _defaults.DefaultDrillShelfHoles = value; }

    // Doors
    public string DefaultDoorDrwSpecies { get => _defaults.DefaultDoorDrwSpecies; set => _defaults.DefaultDoorDrwSpecies = value; }
    public int DefaultDoorCount { get => _defaults.DefaultDoorCount; set => _defaults.DefaultDoorCount = value; }
    public bool DefaultDrillHingeHoles { get => _defaults.DefaultDrillHingeHoles; set => _defaults.DefaultDrillHingeHoles = value; }
    public string DefaultDoorGrainDir { get => _defaults.DefaultDoorGrainDir; set => _defaults.DefaultDoorGrainDir = value; }
    public bool DefaultIncDoorsInList { get => _defaults.DefaultIncDoorsInList; set => _defaults.DefaultIncDoorsInList = value; }
    public bool DefaultIncDoors { get => _defaults.DefaultIncDoors; set => _defaults.DefaultIncDoors = value; }
    public bool DefaultEdgebandDoorsAndDrawers { get => _defaults.DefaultEdgebandDoorsAndDrawers; set => _defaults.DefaultEdgebandDoorsAndDrawers = value; }


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


    // Replace the two existing pass-through properties with these:

    public bool DefaultEqualizeBottomDrwFronts
    {
        get => _defaults.DefaultEqualizeBottomDrwFronts;
        set
        {
            if (_defaults.DefaultEqualizeBottomDrwFronts == value) return;

            _defaults.DefaultEqualizeBottomDrwFronts = value;

            if (!_isApplyingDefaults && value)
            {
                // mutually exclusive
                _defaults.DefaultEqualizeAllDrwFronts = false;

                // clear + persist (these are disabled in the UI anyway)
                //_defaults.DefaultDrwFrontHeight2 = string.Empty;
                //_defaults.DefaultDrwFrontHeight3 = string.Empty;

                _ = _defaults.SaveAsync();
            }

            OnPropertyChanged(nameof(DefaultEqualizeBottomDrwFronts));
            OnPropertyChanged(nameof(DefaultEqualizeAllDrwFronts));
            OnPropertyChanged(nameof(DefaultDrwFrontHeight2));
            OnPropertyChanged(nameof(DefaultDrwFrontHeight3));
        }
    }

    public bool DefaultEqualizeAllDrwFronts
    {
        get => _defaults.DefaultEqualizeAllDrwFronts;
        set
        {
            if (_defaults.DefaultEqualizeAllDrwFronts == value) return;

            _defaults.DefaultEqualizeAllDrwFronts = value;

            if (!_isApplyingDefaults && value)
            {
                // mutually exclusive
                _defaults.DefaultEqualizeBottomDrwFronts = false;

                // clear + persist (these are disabled in the UI anyway)
                //_defaults.DefaultDrwFrontHeight1 = string.Empty;
                //_defaults.DefaultDrwFrontHeight2 = string.Empty;
                //_defaults.DefaultDrwFrontHeight3 = string.Empty;

                _ = _defaults.SaveAsync();
            }

            OnPropertyChanged(nameof(DefaultEqualizeAllDrwFronts));
            OnPropertyChanged(nameof(DefaultEqualizeBottomDrwFronts));
            OnPropertyChanged(nameof(DefaultDrwFrontHeight1));
            OnPropertyChanged(nameof(DefaultDrwFrontHeight2));
            OnPropertyChanged(nameof(DefaultDrwFrontHeight3));
        }
    }

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
    public IReadOnlyList<string> ListDrawerStyle => CabinetOptions.DrawerStyles;
    public IReadOnlyList<int> ListDoorCount => CabinetOptions.DoorCounts;
    public IReadOnlyList<string> ListGrainDirection => CabinetOptions.GrainDirections;
    public ObservableCollection<string> ListCabSpecies => _lookups.CabinetSpecies;
    public ObservableCollection<string> ListEBSpecies => _lookups.EBSpecies;
    public IReadOnlyList<string> ListShelfDepth => CabinetOptions.ShelfDepths;

    public List<string> ListBackThickness =>
        CabinetOptions.BackThickness.GetList(_defaults?.DefaultDimensionFormat ?? "Decimal");

    public IReadOnlyList<string> ListTopType => CabinetOptions.TopTypes;




    public List<string> ThemeOptions { get; } = new()
    {
        "Deep Dark",
        "Soft Dark",
        "Dark Grey Theme",
        "Grey Theme",
        "Light Theme",
        //"Red Black Theme"
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
        // Only reformat when the dimension format actually changes
        if (e.PropertyName != nameof(DefaultSettingsService.DefaultDimensionFormat))
            return;

        if (_defaults == null) return;

        try
        {
            _isApplyingDefaults = true;

            // 1. Set the formatted values FIRST so SelectedItem already matches the new list
            if (_defaults.DefaultDimensionFormat == "Fraction")
            {
                try
                {
                    var baseConv = Convert.ToDouble(_defaults.DefaultBaseBackThickness);
                    DefaultBaseBackThickness = ConvertDimension.DoubleToFraction(baseConv);
                }
                catch
                {
                    DefaultBaseBackThickness = _defaults.DefaultBaseBackThickness;
                }

                try
                {
                    var upperConv = Convert.ToDouble(_defaults.DefaultUpperBackThickness);
                    DefaultUpperBackThickness = ConvertDimension.DoubleToFraction(upperConv);
                }
                catch
                {
                    DefaultUpperBackThickness = _defaults.DefaultUpperBackThickness;
                }

                try
                {
                    var panelConv = Convert.ToDouble(_defaults.DefaultPanelThickness);
                    DefaultPanelThickness = ConvertDimension.DoubleToFraction(panelConv);
                }
                catch
                {
                    DefaultPanelThickness = _defaults.DefaultPanelThickness;
                }
            }
            else
            {
                DefaultBaseBackThickness = _defaults.DefaultBaseBackThickness;
                DefaultUpperBackThickness = _defaults.DefaultUpperBackThickness;
                DefaultPanelThickness = _defaults.DefaultPanelThickness;
            }

            // 2. THEN notify the list changed — ComboBox SelectedItems already match the new list items
            OnPropertyChanged(nameof(ListBackThickness));
        }
        finally
        {
            _isApplyingDefaults = false;
        }
    }

    // Helper: initialize VM fields from _defaults so ItemsSource and SelectedItem match on startup.
    private void ApplyDefaultsToViewModel()
    {
        if (_defaults == null) return;

        try
        {
            _isApplyingDefaults = true;

            if (string.Equals(_defaults.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(_defaults.DefaultBaseBackThickness, out var b))
                    DefaultBaseBackThickness = ConvertDimension.DoubleToFraction(b);
                else
                    DefaultBaseBackThickness = _defaults.DefaultBaseBackThickness;

                if (double.TryParse(_defaults.DefaultUpperBackThickness, out var u))
                    DefaultUpperBackThickness = ConvertDimension.DoubleToFraction(u);
                else
                    DefaultUpperBackThickness = _defaults.DefaultUpperBackThickness;

                if (double.TryParse(_defaults.DefaultPanelThickness, out var p))
                    DefaultPanelThickness = ConvertDimension.DoubleToFraction(p);
                else
                    DefaultPanelThickness = _defaults.DefaultPanelThickness;
            }
            else
            {
                DefaultBaseBackThickness = _defaults.DefaultBaseBackThickness;
                DefaultUpperBackThickness = _defaults.DefaultUpperBackThickness;
                DefaultPanelThickness = _defaults.DefaultPanelThickness;
            }
        }
        finally
        {
            _isApplyingDefaults = false;
        }
    }

}