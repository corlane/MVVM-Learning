using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using MVVM_Learning.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MVVM_Learning.ViewModels
{
    public partial class PanelViewModel : ObservableValidator
    {
        public PanelViewModel()
        {
            // empty constructor for design-time support
        }

        private readonly ICabinetService? _cabinetService;

        public PanelViewModel(ICabinetService cabinetService)
        {
            _cabinetService = cabinetService;

            ValidateAllProperties();
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

    }
}
