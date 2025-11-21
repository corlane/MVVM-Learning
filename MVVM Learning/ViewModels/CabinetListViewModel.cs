using CommunityToolkit.Mvvm.ComponentModel;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using System.Collections.ObjectModel;

namespace MVVM_Learning.ViewModels;

public partial class CabinetListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BaseCabinetModel> cabinets;

    public CabinetListViewModel(ICabinetService cabinetService)
    {
        Cabinets = cabinetService.Cabinets; // Reference the shared collection
    }
}