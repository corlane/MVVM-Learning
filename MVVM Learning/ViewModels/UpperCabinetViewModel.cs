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

    [ObservableProperty]
    public partial string Width { get; set; } = "";

    [ObservableProperty]
    public partial string Height { get; set; } = "";

    [ObservableProperty]
    public partial string Depth { get; set; } = "";

    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new BaseCabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth
        };

        _cabinetService?.Add(newCabinet); // Add to shared service
    }
}