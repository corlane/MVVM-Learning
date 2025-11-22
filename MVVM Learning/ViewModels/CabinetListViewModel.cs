using CommunityToolkit.Mvvm.ComponentModel;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using System.Collections.ObjectModel;

namespace MVVM_Learning.ViewModels;

public partial class CabinetListViewModel(ICabinetService cabinetService) : ObservableObject
{
    public CabinetListViewModel() : this(new CabinetService())
    {
        // empty constructor for design-time support
    }

    [ObservableProperty]
    public partial ObservableCollection<CabinetModel> Cabinets { get; set; } = cabinetService.Cabinets; // Reference the shared collection
}

