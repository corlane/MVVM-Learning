using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVM_Learning.Models;
using MVVM_Learning.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace MVVM_Learning.ViewModels;

public partial class CabinetListViewModel(ICabinetService cabinetService) : ObservableValidator
{
    public CabinetListViewModel() : this(new CabinetService())
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService _cabinetService = cabinetService;


    public ObservableCollection<CabinetModel> Cabinets => _cabinetService.Cabinets;

    [RelayCommand]
    private void DeleteCabinet(CabinetModel cabinet)
    {
        if (cabinet is null) return;

        var result = MessageBox.Show(
            $"Delete {cabinet.CabinetType} {cabinet.Name}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _cabinetService.Remove(cabinet);
        }
    }
}