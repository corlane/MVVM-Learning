using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System;
using CorlaneCabinetOrderFormV3.Views;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel : ObservableValidator
{
    public string AppTitle { get; } = "Corlane Cabinet Order Form - Version 3.0.0.0";

    private readonly ICabinetService _cabinet_service;

    // DI constructor used at runtime
    public MainWindowViewModel(ICabinetService cabinetService)
    {
        _cabinet_service = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        InitializeModificationTracking();
    }

    // Parameterless ctor for design-time support
    public MainWindowViewModel() : this(new CabinetService())
    {
        // design-time: nothing extra required here
    }

    [ObservableProperty]
    public partial bool IsAdmin { get; set; } = true;

    [ObservableProperty] public partial bool ViewportVisible { get; set; } = true;

    [ObservableProperty] public partial bool CabinetListVisible { get; set; } = true;

    // Lazy-resolved tab viewmodels — resolve once and reuse so validation runs against the same instances
    private BaseCabinetViewModel? _baseCabinetVm;
    public BaseCabinetViewModel BaseCabinetVm => _baseCabinetVm ??= App.ServiceProvider.GetRequiredService<BaseCabinetViewModel>();

    private UpperCabinetViewModel? _upperCabinetVm;
    public UpperCabinetViewModel UpperCabinetVm => _upperCabinetVm ??= App.ServiceProvider.GetRequiredService<UpperCabinetViewModel>();

    private FillerViewModel? _fillerVm;
    public FillerViewModel FillerVm => _fillerVm ??= App.ServiceProvider.GetRequiredService<FillerViewModel>();

    private PanelViewModel? _panelVm;
    public PanelViewModel PanelVm => _panelVm ??= App.ServiceProvider.GetRequiredService<PanelViewModel>();

    private PlaceOrderViewModel? _placeOrderVm;
    public PlaceOrderViewModel PlaceOrderVm => _placeOrderVm ??= App.ServiceProvider.GetRequiredService<PlaceOrderViewModel>();

    private DefaultSettingsViewModel? _defaultsVm;
    public DefaultSettingsViewModel DefaultsVm => _defaultsVm ??= App.ServiceProvider.GetRequiredService<DefaultSettingsViewModel>();

    private ProcessOrderViewModel? _processOrderVm;
    public ProcessOrderViewModel ProcessOrderVm => _processOrderVm ??= App.ServiceProvider.GetRequiredService<ProcessOrderViewModel>();

    private REALLYProcessOrderViewModel? _reallyProcessOrderVm;
    public REALLYProcessOrderViewModel REALLYProcessOrderVm => _reallyProcessOrderVm ??= App.ServiceProvider.GetRequiredService<REALLYProcessOrderViewModel>();

    private POCustomerInfoViewModel? _poCustomerInfoVm;
    public POCustomerInfoViewModel POCustomerInfoVm => _poCustomerInfoVm ??= App.ServiceProvider.GetRequiredService<POCustomerInfoViewModel>();

    [RelayCommand]
    private async Task SaveJob()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Corlane Cabinet Order Form Files (*.cor)|*.cor",
            DefaultExt = "cor"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _suppressIsModified = true;
                try
                {
                    var customer = new JobCustomerInfo
                    {
                        CompanyName = POCustomerInfoVm.CompanyName,
                        ContactName = POCustomerInfoVm.ContactName,
                        PhoneNumber = POCustomerInfoVm.PhoneNumber,
                        EMail = POCustomerInfoVm.EMail,
                        Street = POCustomerInfoVm.Street,
                        City = POCustomerInfoVm.City,
                        ZipCode = POCustomerInfoVm.ZipCode
                    };

                    await _cabinet_service.SaveAsync(dialog.FileName, customer, POCustomerInfoVm.QuotedTotalPrice);

                    Notify2($"{System.IO.Path.GetFileNameWithoutExtension(dialog.FileName)} Saved", Brushes.Green, 4000);
                    CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    IsModified = false;
                }
                finally
                {
                    _suppressIsModified = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving job: {ex.Message}", "Error");
            }
        }
    }


    [RelayCommand]
    private async Task LoadJob()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Corlane Cabinet Order Form Files (*.cor)|*.cor"
        };

        if (dialog.ShowDialog() == true)
        {
            _suppressIsModified = true;
            try
            {
                try
                {
                    var job = await _cabinet_service.LoadAsync(dialog.FileName);
                    PlaceOrderVm.OrderedAtLocal = job?.OrderedAtLocal;

                    if (job != null)
                    {
                        POCustomerInfoVm.CompanyName = job.CustomerInfo.CompanyName;
                        POCustomerInfoVm.ContactName = job.CustomerInfo.ContactName;
                        POCustomerInfoVm.PhoneNumber = job.CustomerInfo.PhoneNumber;
                        POCustomerInfoVm.EMail = job.CustomerInfo.EMail;
                        POCustomerInfoVm.Street = job.CustomerInfo.Street;
                        POCustomerInfoVm.City = job.CustomerInfo.City;
                        POCustomerInfoVm.ZipCode = job.CustomerInfo.ZipCode;
                        POCustomerInfoVm.QuotedTotalPrice = job.QuotedTotalPrice;
                    }

                    Notify2($"{System.IO.Path.GetFileNameWithoutExtension(dialog.FileName)} Loaded", Brushes.Green, 4000);
                    CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    IsModified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading job: {ex.Message}", "Error");
                }
            }
            finally
            {
                _suppressIsModified = false;
            }
        }
    }


    // New: Create a fresh job state — clear cabinets, reset UI state and recreate tab VMs so they match freshly booted defaults.
    [RelayCommand]
    private void NewJob()
    {
        _suppressIsModified = true;
        try
        {
            // If nothing to clear, be quick about it
            if ((_cabinet_service.Cabinets == null || _cabinet_service.Cabinets.Count == 0) && CurrentJobName == "Untitled Job")
            {
                Notify2("Nothing to clear", Brushes.Gray);
                return;
            }

            var res = MessageBox.Show(
                "Create a new job? This will clear the current job from memory. Unsaved changes will be lost.",
                "New Job",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            // 1) Clear the shared cabinets collection
            try
            {
                _cabinet_service.Cabinets.Clear();
            }
            catch
            {
                // best-effort
            }

            // 2) Reset main-window state
            CurrentJobName = "Untitled Job";
            SelectedCabinet = null;
            SelectedTabIndex = 0;

            // 3) Clear preview immediately
            try
            {
                var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();
                previewSvc.ClearPreview();
                previewSvc.SetActiveOwner(SelectedTabIndex);
                CurrentPreviewCabinet = null;
            }
            catch
            {
                // ignore preview failures
            }

            // 4) Replace cached tab viewmodels with fresh instances from DI so their constructors run and they return to default values.
            //    Raise property-changed so UI bindings pick up the new instances.
            _baseCabinetVm = null;
            _upperCabinetVm = null;
            _fillerVm = null;
            _panelVm = null;
            _placeOrderVm = null;
            _defaultsVm = null;
            _processOrderVm = null;
            _reallyProcessOrderVm = null;

            // Reset persistent "ordered" state for the new job
            _cabinet_service.OrderedAtLocal = null;

            try
            {
                PlaceOrderVm.OrderedAtLocal = null;
            }
            catch
            {
                // ignore
            }

            OnPropertyChanged(nameof(BaseCabinetVm));
            OnPropertyChanged(nameof(UpperCabinetVm));
            OnPropertyChanged(nameof(FillerVm));
            OnPropertyChanged(nameof(PanelVm));
            OnPropertyChanged(nameof(PlaceOrderVm));
            OnPropertyChanged(nameof(DefaultsVm));
            OnPropertyChanged(nameof(ProcessOrderVm));
            OnPropertyChanged(nameof(REALLYProcessOrderVm));

            // 5) Ensure PlaceOrder tab's transient state is fresh (material totals, pricing)
            try
            {
                var po = PlaceOrderVm;
                po.MaterialTotals.Clear();
                po.TotalPrice = 0m;
                // Reset customer info to persisted defaults (if any)
                var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
                po.CompanyName = defaults.CompanyName;
                po.ContactName = defaults.ContactName;
                po.PhoneNumber = defaults.PhoneNumber;
                po.EMail = defaults.EMail;
                po.Street = defaults.Street;
                po.City = defaults.City;
                po.ZipCode = defaults.ZipCode;
                // Revalidate place order VM
                //po.ValidateAllProperties();
            }
            catch
            {
                // ignore
            }
        }
        finally
        {
            _suppressIsModified = false;
        }

        IsModified = false;
        // 6) Final user feedback
        Notify2("New job ready", Brushes.Green, 3000);
    }

    [ObservableProperty] public partial string CurrentJobName { get; set; } = "Untitled Job";

    [ObservableProperty]
    public partial int SelectedTabIndex { get; set; } = 0;


    [ObservableProperty]
    public partial CabinetModel? CurrentPreviewCabinet { get; set; }


    [ObservableProperty]
    public partial CabinetModel? SelectedCabinet { get; set; }

    partial void OnSelectedTabIndexChanged(int value)
    {
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();
        previewSvc.SetActiveOwner(value);

        try
        {
            // Validate the actual instances the views are bound to (cached properties),
            // so ClearErrors() / ValidateVisible affects the UI instance.
            switch (value)
            {
                case 0:
                    (BaseCabinetVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    break;
                case 1:
                    (UpperCabinetVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    break;
                case 2:
                    (FillerVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    break;
                case 3:
                    (PanelVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    break;
                case 4:
                    (PlaceOrderVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = true;
                    break;
                case 5:
                    (DefaultsVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = true;
                    break;
                case 7:
                    (REALLYProcessOrderVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = false;
                    break;

                default:
                    break;
            }
        }
        catch
        {
            // Swallow: validation should be best-effort and not break tab switching.
        }
    }

    // When the user clicks the list (SelectedCabinet set), you may want to force preview:
    partial void OnSelectedCabinetChanged(CabinetModel? value)
    {
        if (value == null)
        {
            return;
        }

        // Map runtime cabinet type -> tab index
        int targetTab = value switch
        {
            BaseCabinetModel => 0,
            UpperCabinetModel => 1,
            FillerModel => 2,
            PanelModel => 3,
            _ => SelectedTabIndex
        };

        // Only change tab when different (prevents unnecessary churn)
        if (SelectedTabIndex != targetTab)
        {
            SelectedTabIndex = targetTab;
        }

        // Force preview immediately with the selected cabinet's data
        var previewSvc = App.ServiceProvider.GetRequiredService<IPreviewService>();
        previewSvc.ForcePreview(value);
    }


    // Flag used to avoid marking IsModified during programmatic operations (Load/New)
    private bool _suppressIsModified;

    // Track whether the in-memory job has unsaved changes
    [ObservableProperty]
    public partial bool IsModified { get; set; } = false;

    // Computed display string used by the UI (appends marker when modified)
    public string DisplayJobName => IsModified ? $"{CurrentJobName}   *MODIFIED*" : CurrentJobName;

    // When CurrentJobName changes, notify DisplayJobName so UI updates
    partial void OnCurrentJobNameChanged(string oldValue, string newValue)
    {
        OnPropertyChanged(nameof(DisplayJobName));
    }

    // When IsModified changes, notify DisplayJobName so UI updates
    partial void OnIsModifiedChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(DisplayJobName));
    }

    // Call this once (e.g., in constructor) to wire collection/item change tracking
    private void InitializeModificationTracking()
    {
        if (_cabinet_service?.Cabinets is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged += Cabinets_CollectionChanged;

            // attach to any existing items
            foreach (var item in _cabinet_service.Cabinets)
            {
                if (item is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged += Item_PropertyChanged;
            }
        }
    }

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressIsModified) return;

        // Mark modified for add/remove/reset (user changed the collection)
        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace ||
            e.Action == NotifyCollectionChangedAction.Reset)
        {
            IsModified = true;
        }

        // Attach handlers for newly added items so property changes mark modified
        if (e.NewItems != null)
        {
            foreach (var ni in e.NewItems)
            {
                if (ni is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged += Item_PropertyChanged;
            }
        }

        // Detach handlers for removed items
        if (e.OldItems != null)
        {
            foreach (var oi in e.OldItems)
            {
                if (oi is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged -= Item_PropertyChanged;
            }
        }
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressIsModified) return;

        // Any property change on an item marks the job modified.
        IsModified = true;
    }
}