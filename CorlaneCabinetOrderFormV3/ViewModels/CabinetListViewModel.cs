using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

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
            PropertyChangedEventManager.AddHandler(
                _defaults,
                DefaultSettings_PropertyChanged,
                nameof(DefaultSettingsService.DefaultDimensionFormat));
        }

        // Subscribe to MainWindowViewModel's SelectedCabinet changes to keep in sync
        if (_mainVm != null)
        {
            PropertyChangedEventManager.AddHandler(
                _mainVm,
                MainVm_PropertyChanged,
                nameof(MainWindowViewModel.SelectedCabinet));
        }

        // Track IsSelected changes on existing and future cabinets
        if (_cabinetService?.Cabinets is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged += OnCabinetsCollectionChanged;
            foreach (var cab in _cabinetService.Cabinets)
                cab.PropertyChanged += OnCabinetPropertyChanged;
        }
    }

    private void OnCabinetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CabinetModel cab in e.NewItems)
                cab.PropertyChanged += OnCabinetPropertyChanged;
        }

        if (e.OldItems != null)
        {
            foreach (CabinetModel cab in e.OldItems)
                cab.PropertyChanged -= OnCabinetPropertyChanged;
        }

        if (e.Action == NotifyCollectionChangedAction.Reset && _cabinetService?.Cabinets != null)
        {
            foreach (var cab in _cabinetService.Cabinets)
                cab.PropertyChanged += OnCabinetPropertyChanged;
        }

        OnPropertyChanged(nameof(HasSelectedCabinets));
    }

    private void OnCabinetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CabinetModel.IsSelected))
            OnPropertyChanged(nameof(HasSelectedCabinets));
    }

    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
        {
            OnPropertyChanged(nameof(SelectedCabinet));
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

    /// <summary>True when at least one cabinet has IsSelected checked.</summary>
    public bool HasSelectedCabinets =>
        _cabinetService?.Cabinets.Any(c => c.IsSelected) == true;

    [RelayCommand]
    private void SelectAll()
    {
        if (_cabinetService?.Cabinets == null) return;
        foreach (var cab in _cabinetService.Cabinets)
            cab.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        if (_cabinetService?.Cabinets == null) return;
        foreach (var cab in _cabinetService.Cabinets)
            cab.IsSelected = false;
    }

    [RelayCommand]
    private void ModifySelected()
    {
        if (_cabinetService?.Cabinets == null) return;

        var selected = _cabinetService.Cabinets.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0) return;

        var vm = new BatchModifyViewModel(selected);
        var window = new BatchModifyWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            vm.ApplyToSelected();
            _mainVm?.IsModified = true;

            // Clear selection checkboxes after applying
            foreach (var cab in selected)
                cab.IsSelected = false;
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



    public void RefreshSelectedCabinet()
    {
        _mainVm?.RefreshSelectedCabinet();

        // Keep this VM's proxy property fresh too
        OnPropertyChanged(nameof(SelectedCabinet));
    }
}