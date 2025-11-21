using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;

namespace MVVM_Learning.ViewModels;

public partial class BaseCabinetViewModel : ObservableObject
{
    private readonly ICabinetService? _cabinetService;

    public BaseCabinetViewModel()
    {
        
    }

    public BaseCabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
    }

    [ObservableProperty] private string width = "";
    [ObservableProperty] private string height = "";
    [ObservableProperty] private string depth = "";

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

        Width = Height = Depth = string.Empty;
    }
}