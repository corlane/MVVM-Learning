using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class CabinetListViewModel : ObservableValidator
{
    public CabinetListViewModel()
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService? _cabinetService;
    private readonly MainWindowViewModel? _mainVm;
    private readonly DefaultSettingsService? _defaults;

    // Updated constructor to accept defaults (DI-friendly)
    public CabinetListViewModel(ICabinetService cabinetService, MainWindowViewModel mainVm, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _mainVm = mainVm;
        _defaults = defaults;

        // React to default dimension format changes so list updates immediately
        if (_defaults != null)
        {
            _defaults.PropertyChanged += DefaultSettings_PropertyChanged;
        }

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

    private void DefaultSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            // Refresh the collection view on UI thread so converters re-run for each row.
            if (Application.Current?.Dispatcher == null)
            {
                CollectionViewSource.GetDefaultView(Cabinets)?.Refresh();
                OnPropertyChanged(nameof(Cabinets));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CollectionViewSource.GetDefaultView(Cabinets)?.Refresh();
                    OnPropertyChanged(nameof(Cabinets));
                });
            }
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
            _mainVm?.IsModified = true;
        }
    }
}