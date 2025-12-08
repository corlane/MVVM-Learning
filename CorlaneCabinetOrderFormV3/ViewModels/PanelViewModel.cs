using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;


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

    public PanelViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
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
        Height = "32";
        Depth = "0.75";
        LoadDefaults();
        ValidateAllProperties();
    }


    // Common properties from CabinetModel
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
    [ObservableProperty, NotifyDataErrorInfo, Required] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; } = "";

    // Type-specific properties for PanelModel
    [ObservableProperty] public partial bool PanelEBTop { get; set; }
    [ObservableProperty] public partial bool PanelEBBottom { get; set; }
    [ObservableProperty] public partial bool PanelEBLeft { get; set; }
    [ObservableProperty] public partial bool PanelEBRight { get; set; }

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
    public List<string> ListPanelDepths { get; } =
    [
        "0.25",
        "0.75"
    ];


    private void LoadSelectedIfMine()
    {
        if (_mainVm!.SelectedCabinet is PanelModel panel)
        {
            Width = panel.Width;
            Height = panel.Height;
            Depth = panel.Depth;
            Species = panel.Species;
            Name = panel.Name;
            Qty = panel.Qty;
            Notes = panel.Notes;
            PanelEBTop = panel.PanelEBTop;
            PanelEBBottom = panel.PanelEBBottom;
            PanelEBLeft = panel.PanelEBLeft;
            PanelEBRight = panel.PanelEBRight;
            PanelEBBottom = panel.PanelEBBottom;

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
        var newCabinet = new PanelModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            Name = Name,
            Qty = Qty,
            Notes = Notes,
            PanelEBTop = PanelEBTop,
            PanelEBBottom = PanelEBBottom,
            PanelEBLeft = PanelEBLeft,
            PanelEBRight = PanelEBRight
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type

    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (_mainVm!.SelectedCabinet is PanelModel selected)
        {
            selected.Width = Width;
            selected.Height = Height;
            selected.Depth = Depth;
            selected.Species = Species;
            selected.Name = Name;
            selected.Qty = Qty;
            selected.Notes = Notes;
            selected.PanelEBTop = PanelEBTop;
            selected.PanelEBBottom = PanelEBBottom;
            selected.PanelEBLeft = PanelEBLeft;
            selected.PanelEBRight = PanelEBRight;

            // copy every property back

            // No collection replace needed — bindings update instantly
        }

        // Optional: clear selection after update
        _mainVm.SelectedCabinet = null;
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
        //_mainVm.CurrentPreviewCabinet = new PanelModel -- Original before Preview Service
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();

        var model = new PanelModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
        };
        // Request preview using the tab index owner token (Panel tab = 3)
        previewSvc.RequestPreview(3, model);
    }

}
