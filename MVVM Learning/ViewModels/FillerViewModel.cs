using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Converters;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using MVVM_Learning.ValidationAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;


namespace MVVM_Learning.ViewModels;

public partial class FillerViewModel : ObservableValidator
{
    public FillerViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;

    public FillerViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;

        ValidateAllProperties();
    }


    // Common properties from CabinetModel
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty] public partial string Width { get; set; } = "";
    [ObservableProperty] public partial string Height { get; set; } = "";
    [ObservableProperty] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; } = "";

    private void AddCabinet()
    {
        var newCabinet = new BaseCabinetModel
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

}
