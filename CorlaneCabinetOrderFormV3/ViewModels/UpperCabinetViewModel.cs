using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    public UpperCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;
    private bool _isMapping;

    public UpperCabinetViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
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
            _defaults.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
                    OnPropertyChanged(nameof(ListBackThickness));
            };
        }

        LoadDefaults();

    }


    // Upper cabinet type strings
    public static string Style1 => "Standard";
    public static string Style2 => "90° Corner";
    public static string Style3 => "Angle Front";

    // Common properties from CabinetModel
    [ObservableProperty] public partial string Style { get; set; } = ""; partial void OnStyleChanged(string value)
    {
        if (_isMapping) return;
        StandardDimsVisibility = value == Style1;
        Corner90DimsVisibility = value == Style2;
        Corner45DimsVisibility = value == Style3;
        BackThicknessVisible = value == Style1;

        //LoadDefaults();
        RunValidationVisible();
    }
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required, Range(1, 100)] public partial int Qty { get; set; } = 1;
    [ObservableProperty] public partial string Notes { get; set; } = "";


    // Corner Cab specific properties
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftBackWidth { get; set; } = ""; partial void OnLeftBackWidthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightBackWidth { get; set; } = ""; partial void OnRightBackWidthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftFrontWidth { get; set; } = ""; partial void OnLeftFrontWidthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            LeftBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth));
            RightBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth));
            LeftBackWidth90 = new DimensionFormatConverter().Convert(LeftBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RightBackWidth90 = new DimensionFormatConverter().Convert(RightBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightFrontWidth { get; set; } = ""; partial void OnRightFrontWidthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            LeftBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth));
            RightBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth));
            LeftBackWidth90 = new DimensionFormatConverter().Convert(LeftBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RightBackWidth90 = new DimensionFormatConverter().Convert(RightBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftDepth { get; set; } = ""; partial void OnLeftDepthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            LeftBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth));
            RightBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth));
            LeftBackWidth90 = new DimensionFormatConverter().Convert(LeftBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RightBackWidth90 = new DimensionFormatConverter().Convert(RightBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightDepth { get; set; } = ""; partial void OnRightDepthChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            LeftBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth));
            RightBackWidth90 = Convert.ToString(ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth));
            LeftBackWidth90 = new DimensionFormatConverter().Convert(LeftBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RightBackWidth90 = new DimensionFormatConverter().Convert(RightBackWidth90, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString()!;
            RecalculateFrontWidth();
            RunValidationVisible();
        }
    }
    [ObservableProperty] public partial string FrontWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftBackWidth90 { get; set; } = "";
    [ObservableProperty] public partial string RightBackWidth90 { get; set; } = "";

    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BackThickness { get; set; } = ""; partial void OnBackThicknessChanged(string oldValue, string newValue)
    {
        if (newValue != oldValue)
        {
            RunValidationVisible();
        }
    }


    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }

    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }


    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string RightReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string TopReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string GapWidth { get; set; } = "";


    // Combobox options
    public List<int> ComboShelfCount { get; } = [0, 1, 2, 3, 4, 5];
    public static List<string> TypeList => [Style1, Style2, Style3];
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

            return [thin, thick];
        }
    }

    // Visibility properties
    [ObservableProperty] public partial bool StandardDimsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool Corner90DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool Corner45DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool ShowRevealSettings { get; set; } = true;
    [ObservableProperty] public partial bool BackThicknessVisible { get; set; } = true;


    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new UpperCabinetModel
        {
            Width = ConvertDimension.FractionToDouble(Width).ToString(),
            Height = ConvertDimension.FractionToDouble(Height).ToString(),
            Depth = ConvertDimension.FractionToDouble(Depth).ToString(),
            Species = Species,
            EBSpecies = EBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
            Style = Style,
            LeftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth).ToString(),
            RightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth).ToString(),
            LeftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidth).ToString(),
            RightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidth).ToString(),
            LeftDepth = ConvertDimension.FractionToDouble(LeftDepth).ToString(),
            RightDepth = ConvertDimension.FractionToDouble(RightDepth).ToString(),
            DoorSpecies = DoorSpecies,
            BackThickness = ConvertDimension.FractionToDouble(BackThickness).ToString(),
            ShelfCount = ShelfCount,
            DrillShelfHoles = DrillShelfHoles,
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            IncDoorsInList = IncDoorsInList,
            IncDoors = IncDoors,
            DrillHingeHoles = DrillHingeHoles,
            LeftReveal = ConvertDimension.FractionToDouble(LeftReveal).ToString(),
            RightReveal = ConvertDimension.FractionToDouble(RightReveal).ToString(),
            TopReveal = ConvertDimension.FractionToDouble(TopReveal).ToString(),
            BottomReveal = ConvertDimension.FractionToDouble(BottomReveal).ToString(),
            GapWidth = ConvertDimension.FractionToDouble(GapWidth).ToString()
        };

        _cabinetService?.Add(newCabinet); // Add to shared service
        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;
    }

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is UpperCabinetModel upperCab)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(upperCab, dimFormat);

            // Any additional logic that must run after loading (visibility, resize, preview)
            UpdatePreview();
        }
        else
        {
            //LoadDefaults();
        }
    }

    private void RecalculateFrontWidth()
    {

        // Only relevant for "Angle Front" (Style4). Clear it otherwise.
        if (!string.Equals(Style, Style3, StringComparison.Ordinal))
        {
            FrontWidth = string.Empty;
            return;
        }

        // For the angle-front cabinet, the polygon edge used in Cabinet3DViewModel is:
        // p0 = (LeftDepth, 0)
        // p1 = (RightBackWidth - 3/4, LeftBackWidth - RightDepth)
        // frontWidth = distance(p0, p1)
        try
        {
            double leftDepth = ConvertDimension.FractionToDouble(LeftDepth);
            double rightDepth = ConvertDimension.FractionToDouble(RightDepth);
            double leftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth);
            double rightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth);

            const double materialThickness34 = 0.75;

            double p0x = leftDepth;
            double p0y = materialThickness34;

            double p1x = rightBackWidth - materialThickness34;
            double p1y = leftBackWidth - rightDepth;

            double vx = p1x - p0x;
            double vy = p1y - p0y;

            double frontWidth = Math.Sqrt((vx * vx) + (vy * vy));

            // Format per default settings
            string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
            FrontWidth = string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                ? ConvertDimension.DoubleToFraction(frontWidth)
                : frontWidth.ToString("0.####");
        }
        catch
        {
            // If any inputs are invalid/empty, just clear output.
            FrontWidth = string.Empty;
        }
    }


    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm!.SelectedCabinet is UpperCabinetModel selected)
        {
            selected.Width = ConvertDimension.FractionToDouble(Width).ToString();
            selected.Height = ConvertDimension.FractionToDouble(Height).ToString();
            selected.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
            selected.Species = Species;
            selected.EBSpecies = EBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
            selected.Style = Style;
            selected.LeftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth).ToString();
            selected.RightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth).ToString();
            selected.LeftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidth).ToString();
            selected.RightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidth).ToString();
            selected.LeftDepth = ConvertDimension.FractionToDouble(LeftDepth).ToString();
            selected.RightDepth = ConvertDimension.FractionToDouble(RightDepth).ToString();
            selected.DoorSpecies = DoorSpecies;
            selected.BackThickness = BackThickness;
            selected.ShelfCount = ShelfCount;
            selected.DrillShelfHoles = DrillShelfHoles;
            selected.DoorCount = DoorCount;
            selected.DoorGrainDir = DoorGrainDir;
            selected.IncDoorsInList = IncDoorsInList;
            selected.IncDoors = IncDoors;
            selected.DrillHingeHoles = DrillHingeHoles;
            selected.LeftReveal = ConvertDimension.FractionToDouble(LeftReveal).ToString();
            selected.RightReveal = ConvertDimension.FractionToDouble(RightReveal).ToString();
            selected.TopReveal = ConvertDimension.FractionToDouble(TopReveal).ToString();
            selected.BottomReveal = ConvertDimension.FractionToDouble(BottomReveal).ToString();
            selected.GapWidth = ConvertDimension.FractionToDouble(GapWidth).ToString();

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
        if (_defaults is null) return;
        Species = _defaults.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        ShelfCount = _defaults.DefaultShelfCount;
        DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
        if (_defaults.DefaultDimensionFormat == "Decimal") { BackThickness = _defaults.DefaultBaseBackThickness; }
        else { BackThickness = ConvertDimension.DoubleToFraction(Convert.ToDouble(_defaults.DefaultBaseBackThickness)); }
        //BackThickness = _defaults.DefaultUpperBackThickness;
        DoorCount = _defaults.DefaultDoorCount;
        IncDoors = _defaults.DefaultIncDoors;
        IncDoorsInList = _defaults.DefaultIncDoorsInList;
        DoorSpecies = _defaults.DefaultDoorDrwSpecies;
        DrillHingeHoles = _defaults.DefaultDrillHingeHoles;
        DoorGrainDir = _defaults.DefaultDoorGrainDir;
        LeftReveal = _defaults.DefaultUpperLeftReveal;
        RightReveal = _defaults.DefaultUpperRightReveal;
        TopReveal = _defaults.DefaultUpperTopReveal;
        BottomReveal = _defaults.DefaultUpperBottomReveal;
        GapWidth = _defaults.DefaultGapWidth;
        // etc.
    }


    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
{
    "Width","Height","Depth","TKHeight","TKDepth",
    "LeftBackWidth","RightBackWidth","LeftFrontWidth","RightFrontWidth",
    "LeftDepth","RightDepth","BackThickness", "FrontWidth",
    "LeftReveal","RightReveal","TopReveal","BottomReveal","GapWidth"
};

    private void MapModelToViewModel(UpperCabinetModel model, string dimFormat)
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


    // For 3D model:

    private void UpdatePreview()
    {
        //_mainVm.CurrentPreviewCabinet = new UpperCabinetModel --- Original before Preview Service
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();

        var model = new UpperCabinetModel
        {
            Style = Style,
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            LeftBackWidth = LeftBackWidth,
            RightBackWidth = RightBackWidth,
            LeftFrontWidth = LeftFrontWidth,
            RightFrontWidth = RightFrontWidth,
            LeftDepth = LeftDepth,
            RightDepth = RightDepth,
            DoorSpecies = DoorSpecies,
            BackThickness = BackThickness,
            ShelfCount = ShelfCount,
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            IncDoors = IncDoors,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        // Request preview using the tab index owner token (Upper tab = 1)
        previewSvc.RequestPreview(1, model);
    }
}