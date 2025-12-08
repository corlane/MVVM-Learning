using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

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

        Width = "16";
        Height = "42";
        Depth = "12";

        ValidateAllProperties();
    }


    // Upper cabinet type strings
    public static string Style1 => "Standard";
    public static string Style2 => "90° Corner";
    public static string Style3 => "45° Corner";

    // Common properties from CabinetModel
    [ObservableProperty] public partial string Style { get; set; } = ""; partial void OnStyleChanged(string value)
    {
        StandardDimsVisibility = value == Style1;
        Corner90DimsVisibility = value == Style2;
        Corner45DimsVisibility = value == Style3;
        LoadDefaults();
    }
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; } = "";


    // Corner Cab specific properties
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftBackWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightBackWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftFrontWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightFrontWidth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string LeftDepth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string RightDepth { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string BackThickness { get; set; } = "";



    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }

    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }


    [ObservableProperty] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty] public partial string RightReveal { get; set; } = "";
    [ObservableProperty] public partial string TopReveal { get; set; } = "";
    [ObservableProperty] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty] public partial string GapWidth { get; set; } = "";


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
    public List<string> ListBackThickness { get; } =
        [
            "0.25",
            "0.75"
        ];


    // Visibility properties
    [ObservableProperty] public partial bool StandardDimsVisibility { get; set; } = true;
    [ObservableProperty] public partial bool Corner90DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool Corner45DimsVisibility { get; set; } = false;
    [ObservableProperty] public partial bool ShowRevealSettings { get; set; } = false;

    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new UpperCabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
            Style = Style,
            LeftBackWidth = LeftBackWidth,
            RightBackWidth = RightBackWidth,
            LeftFrontWidth = LeftFrontWidth,
            RightFrontWidth = RightFrontWidth,
            LeftDepth = LeftDepth,
            RightDepth = RightDepth,
            DoorSpecies = DoorSpecies,
            BackThickness = BackThickness,
            ShelfCount = ShelfCount,
            DrillShelfHoles = DrillShelfHoles,
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            IncDoorsInList = IncDoorsInList,
            IncDoors = IncDoors,
            DrillHingeHoles = DrillHingeHoles,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        _cabinetService?.Add(newCabinet); // Add to shared service
    }

    private void LoadSelectedIfMine()
    {
        if (_mainVm.SelectedCabinet is UpperCabinetModel upperCab)
        {
            Width = upperCab.Width;
            Height = upperCab.Height;
            Depth = upperCab.Depth;
            Species = upperCab.Species;
            EBSpecies = upperCab.EBSpecies;
            Name = upperCab.Name;
            Qty = upperCab.Qty;
            Notes = upperCab.Notes;
            Style = upperCab.Style;
            LeftBackWidth = upperCab.LeftBackWidth;
            RightBackWidth = upperCab.RightBackWidth;
            LeftFrontWidth = upperCab.LeftFrontWidth;
            RightFrontWidth = upperCab.RightFrontWidth;
            LeftDepth = upperCab.LeftDepth;
            RightDepth = upperCab.RightDepth;
            DoorSpecies = upperCab.DoorSpecies;
            BackThickness = upperCab.BackThickness;
            ShelfCount = upperCab.ShelfCount;
            DrillShelfHoles = upperCab.DrillShelfHoles;
            DoorCount = upperCab.DoorCount;
            DoorGrainDir = upperCab.DoorGrainDir;   
            IncDoorsInList = upperCab.IncDoorsInList;
            IncDoors = upperCab.IncDoors;
            DrillHingeHoles = upperCab.DrillHingeHoles;
            LeftReveal = upperCab.LeftReveal;
            RightReveal = upperCab.RightReveal;
            TopReveal = upperCab.TopReveal;
            BottomReveal = upperCab.BottomReveal;
            GapWidth = upperCab.GapWidth;

            UpdatePreview();
        }
        else if (_mainVm.SelectedCabinet == null)
        {
            // Optional: clear fields when nothing selected
            //Width = Height = Depth = ToeKickHeight = "";
            // clear all
        }


    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm.SelectedCabinet is UpperCabinetModel selected)
        {
            selected.Width = Width;
            selected.Height = Height;
            selected.Depth = Depth;
            selected.Species = Species;
            selected.EBSpecies = EBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
            selected.Style = Style;
            selected.LeftBackWidth = LeftBackWidth;
            selected.RightBackWidth = RightBackWidth;
            selected.LeftFrontWidth = LeftFrontWidth;
            selected.RightFrontWidth = RightFrontWidth;
            selected.LeftDepth = LeftDepth;
            selected.RightDepth = RightDepth;
            selected.DoorSpecies = DoorSpecies;
            selected.BackThickness = BackThickness;
            selected.ShelfCount = ShelfCount;
            selected.DrillShelfHoles = DrillShelfHoles;
            selected.DoorCount = DoorCount;
            selected.DoorGrainDir = DoorGrainDir;
            selected.IncDoorsInList = IncDoorsInList;
            selected.IncDoors = IncDoors;
            selected.DrillHingeHoles = DrillHingeHoles;
            selected.LeftReveal = LeftReveal;
            selected.RightReveal = RightReveal;
            selected.TopReveal = TopReveal;
            selected.BottomReveal = BottomReveal;
            selected.GapWidth = GapWidth;

            // copy every property back

            // No collection replace needed — bindings update instantly
        }

        // Optional: clear selection after update
        //_mainVm.SelectedCabinet = null;
    }

    [RelayCommand]
    private void LoadDefaults()
    {
        Species = _defaults.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        ShelfCount = _defaults.DefaultShelfCount;
        DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
        BackThickness = _defaults.DefaultUpperBackThickness;
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