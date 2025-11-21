using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;

namespace MVVM_Learning.ViewModels;

public partial class CabinetViewModel : ObservableObject
{
    private readonly ICabinetService _cabinetService;

    public CabinetViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;
    }

    [ObservableProperty] private string width = "";
    [ObservableProperty] private string height = "";
    [ObservableProperty] private string depth = "";

    [RelayCommand]
    private void AddCabinet()
    {
        var newCabinet = new CabinetModel
        {
            Width = Width,
            Height = Height,
            Depth = Depth
        };

        _cabinetService.Add(newCabinet); // Add to shared service

        Width = Height = Depth = string.Empty;
    }
}