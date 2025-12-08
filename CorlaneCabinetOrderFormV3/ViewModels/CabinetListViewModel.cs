using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class CabinetListViewModel : ObservableValidator
{
    public CabinetListViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;

    public CabinetListViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;

        // Subscribe to MainWindowViewModel's SelectedCabinet changes to keep in sync
        if (_mainVm != null)
        {
            _mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
                {
                    OnPropertyChanged(nameof(SelectedCabinet));
                }
            };
        }
    }

    public ObservableCollection<CabinetModel> Cabinets => _cabinetService?.Cabinets ?? new ObservableCollection<CabinetModel>();

    // Expose SelectedCabinet from MainWindowViewModel for two-way binding
    public CabinetModel? SelectedCabinet
    {
        get => _mainVm?.SelectedCabinet;
        set
        {
            if (_mainVm != null)
            {
                var oldValue = _mainVm.SelectedCabinet;
                if (oldValue != value)
                {
                    _mainVm.SelectedCabinet = value;
                    OnPropertyChanged();
                }
            }
        }
    }

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
            _cabinetService?.Remove(cabinet);
        }
    }
}