using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;

namespace MVVM_Learning.ViewModels;

public partial class BaseCabinetViewModel : ObservableObject
{
    public BaseCabinetViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;

    public BaseCabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
    }

    [ObservableProperty] public partial string Width { get; set; } = "";
    [ObservableProperty] public partial string Height { get; set; } = "";
    [ObservableProperty] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string ToeKickHeight { get; set; } = "4";  // Subtype-specific

    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new BaseCabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth,
            ToeKickHeight = ToeKickHeight  // Subtype-specific
        };

        _cabinetService?.Add(newCabinet);  // Adds to shared list as base type

        // Clear fields (add clears for new props too)
        Width = Height = Depth = ToeKickHeight = string.Empty;
    }
}