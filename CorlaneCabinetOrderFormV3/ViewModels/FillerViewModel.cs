using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.Windows;


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
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
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

    private void LoadSelectedIfMine()
    {
        if (_mainVm.SelectedCabinet is FillerModel filler)
        {
            Width = filler.Width;
            Height = filler.Height;
            Depth = filler.Depth;
            Species = filler.Species;
            EBSpecies = filler.EBSpecies;
            Name = filler.Name;
            Qty = filler.Qty;
            Notes = filler.Notes;

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
    private void AddCabinet()
    {
        var newCabinet = new FillerModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type
    }


    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm!.SelectedCabinet is FillerModel selected)
        {
            selected.Width = Width;
            selected.Height = Height;
            selected.Depth = Depth;
            selected.Species = Species;
            selected.EBSpecies = EBSpecies;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
        }

        // Optional: clear selection after update
        //_mainVm.SelectedCabinet = null;
    }


    [RelayCommand]
    private void LoadDefaults()
    {
        Species = _defaults!.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        
        // etc.
    }


    private void UpdatePreview()
    {
        _mainVm.CurrentPreviewCabinet = new FillerModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,

            // ... copy EVERY property from fields to the preview model
            // Yes, it's a few lines, but it's the only place — do it once per ViewModel
        };
    }

}
