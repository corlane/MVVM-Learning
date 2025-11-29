using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class PanelViewModel : ObservableValidator
    {
        public PanelViewModel()
        {
            // empty constructor for design-time support
        }

        // Example: BaseCabinetViewModel.cs (copy to all input VMs)

        private readonly ICabinetService? _cabinetService;
        private readonly MainWindowViewModel? _mainVm;

        public PanelViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm)
        {
            _cabinetService = cabinetService;
            _mainVm = mainVm;

            _mainVm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
                    LoadSelectedIfMine();
            };

            LoadSelectedIfMine(); // initial
        }


        // Common properties from CabinetModel
        [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
        [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
        [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Width { get; set; } = "";
        [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Height { get; set; } = "";
        [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Enter a value"), DimensionRange(8, 48)] public partial string Depth { get; set; } = "";
        [ObservableProperty] public partial string Species { get; set; } = "";
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

        private void LoadSelectedIfMine()
        {
            if (_mainVm.SelectedCabinet is PanelModel panel)
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

                // copy every property
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
            if (_mainVm.SelectedCabinet is PanelModel selected)
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
    }
}
