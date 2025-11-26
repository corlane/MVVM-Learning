using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;

namespace MVVM_Learning.ViewModels;

public partial class UpperCabinetViewModel : ObservableObject
{
    private readonly ICabinetService? _cabinetService;

    public UpperCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    public UpperCabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
    }

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

    // Type-specific properties for UpperCabinetModel
    [ObservableProperty] public partial string Type { get; set; } = "";
    [ObservableProperty] public partial string LeftBackWidth { get; set; } = "";
    [ObservableProperty] public partial string RightBackWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string RightFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftDepth { get; set; } = "";
    [ObservableProperty] public partial string RightDepth { get; set; } = "";
    [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty] public partial string BackThickness { get; set; } = "";
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


    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new UpperCabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth
        };

        _cabinetService?.Add(newCabinet); // Add to shared service
    }
}