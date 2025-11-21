using CommunityToolkit.Mvvm.ComponentModel;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using System.Collections.ObjectModel;

namespace MVVM_Learning.ViewModels;

public partial class CabinetListViewModel(ICabinetService cabinetService) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<BaseCabinetModel> Cabinets { get; set; } = cabinetService.Cabinets; // Reference the shared collection
}