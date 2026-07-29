using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel : ObservableValidator
{
    public string AppTitle { get; } = "Corlane Cabinet Order Form - Version 3.1.0.18";

    private readonly ICabinetService _cabinetService;
    private readonly AutoSaveService _autoSave;
    private readonly DefaultSettingsService _defaults;
    private readonly IPrintService _printer;
    private readonly IPreviewService _previewSvc;
    private readonly IServiceProvider _services;

    // DI constructor used at runtime
    public MainWindowViewModel(
        ICabinetService cabinetService,
        DefaultSettingsService defaults,
        IPrintService printer,
        IPreviewService previewSvc,
        IServiceProvider services)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _printer = printer ?? throw new ArgumentNullException(nameof(printer));
        _previewSvc = previewSvc ?? throw new ArgumentNullException(nameof(previewSvc));
        _services = services ?? throw new ArgumentNullException(nameof(services));

        InitializeModificationTracking();

        // ── Auto-save setup ──────────────────────────────────────────
        _autoSave = new AutoSaveService(cabinetService);
        _autoSave.Configure(
            customerInfoProvider: () => BuildCustomerInfo(),
            quotedPriceProvider: () => POCustomerInfoVm.QuotedTotalPrice);

        // Load persisted UI scale
        UIScale = _defaults.UIScale is > 0 ? _defaults.UIScale : 1.0;

        _cabinetService.ExceptionDoneStateChanged += () =>
        {
            if (!_suppressIsModified)
                IsModified = true;
        };
    }

    // Parameterless ctor for design-time support
    public MainWindowViewModel() : this(
        new CabinetService(),
        new DefaultSettingsService(),
        null!,
        null!,
        null!)
    {
        // design-time: nothing extra required here
    }

    // Lazy-resolved tab viewmodels — resolve once and reuse so validation runs against the same instances
    private BaseCabinetViewModel? _baseCabinetVm;
    public BaseCabinetViewModel BaseCabinetVm => _baseCabinetVm ??= _services.GetRequiredService<BaseCabinetViewModel>();

    private UpperCabinetViewModel? _upperCabinetVm;
    public UpperCabinetViewModel UpperCabinetVm => _upperCabinetVm ??= _services.GetRequiredService<UpperCabinetViewModel>();

    private FillerViewModel? _fillerVm;
    public FillerViewModel FillerVm => _fillerVm ??= _services.GetRequiredService<FillerViewModel>();

    private PanelViewModel? _panelVm;
    public PanelViewModel PanelVm => _panelVm ??= _services.GetRequiredService<PanelViewModel>();

    private PlaceOrderViewModel? _placeOrderVm;
    public PlaceOrderViewModel PlaceOrderVm => _placeOrderVm ??= _services.GetRequiredService<PlaceOrderViewModel>();

    private DefaultSettingsViewModel? _defaultsVm;
    public DefaultSettingsViewModel DefaultsVm => _defaultsVm ??= _services.GetRequiredService<DefaultSettingsViewModel>();

    private MaterialPricesViewModel? _materialPricesVm;
    public MaterialPricesViewModel MaterialPricesVm => _materialPricesVm ??= _services.GetRequiredService<MaterialPricesViewModel>();

    private ProcessOrderViewModel? _processOrderVm;
    public ProcessOrderViewModel ProcessOrderVm => _processOrderVm ??= _services.GetRequiredService<ProcessOrderViewModel>();

    private POCustomerInfoViewModel? _poCustomerInfoVm;
    public POCustomerInfoViewModel POCustomerInfoVm => _poCustomerInfoVm ??= _services.GetRequiredService<POCustomerInfoViewModel>();

    // Flag used to avoid marking IsModified during programmatic operations (Load/New)
    private bool _suppressIsModified;

    // Computed display string used by the UI (appends marker when modified)
    public string DisplayJobName => IsModified ? $"{CurrentJobName}   *MODIFIED*" : CurrentJobName;

    [ObservableProperty] public partial bool IsAdmin { get; set; } = false;

    [ObservableProperty] public partial bool ViewportVisible { get; set; } = true;

    [ObservableProperty] public partial bool CabinetListVisible { get; set; } = true;

    [ObservableProperty] public partial bool RightPanelVisible { get; set; } = true;

    // ── UI Scale ───────────────────────────────────────────────
    [ObservableProperty]
    public partial double UIScale { get; set; } = 1.0; partial void OnUIScaleChanged(double value)
    {
        _defaults.UIScale = value;
        _ = _defaults.SaveAsync();
    }

    [ObservableProperty] public partial string CurrentJobName { get; set; } = "Untitled Job"; partial void OnCurrentJobNameChanged(string oldValue, string newValue)
    {
        OnPropertyChanged(nameof(DisplayJobName));
    }


    /// <summary>Full file path of the last saved/loaded .cor file, or null for a fresh job.</summary>
    [ObservableProperty] public partial string? CurrentJobPath { get; set; }

    [ObservableProperty] public partial int SelectedTabIndex { get; set; } = 0; partial void OnSelectedTabIndexChanged(int value)
    {
        _previewSvc.SetActiveOwner(value);

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
                    RightPanelVisible = true;
                    break;
                case 1:
                    (UpperCabinetVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    RightPanelVisible = true;
                    break;
                case 2:
                    (FillerVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    RightPanelVisible = true;
                    break;
                case 3:
                    (PanelVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = true;
                    CabinetListVisible = true;
                    RightPanelVisible = true;
                    break;
                case 4:
                    (PlaceOrderVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = true;
                    RightPanelVisible = true;
                    break;
                case 5:
                    (DefaultsVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = true;
                    RightPanelVisible = true;
                    break;

                case 6:
                    ViewportVisible = false;
                    CabinetListVisible = false;
                    RightPanelVisible = false;
                    break;

                case 7:
                    (ProcessOrderVm as IValidatableViewModel)?.RunValidationVisible();
                    ViewportVisible = false;
                    CabinetListVisible = false;
                    RightPanelVisible = false;
                    break;

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Catch] Tab validation failed on tab {value}: {ex.Message}");
        }
    }

    [ObservableProperty] public partial CabinetModel? SelectedCabinet { get; set; } partial void OnSelectedCabinetChanged(CabinetModel? value)
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
        _previewSvc.ForcePreview(value);
    }

    // Track whether the in-memory job has unsaved changes
    [ObservableProperty] public partial bool IsModified { get; set; } = false; partial void OnIsModifiedChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(DisplayJobName));

        AutoSave();
    }


    // Call this once (e.g., in constructor) to wire collection/item change tracking
    private void InitializeModificationTracking()
    {
        if (_cabinetService?.Cabinets is INotifyCollectionChanged coll)
        {
            coll.CollectionChanged += Cabinets_CollectionChanged;

            // attach to any existing items
            foreach (var item in _cabinetService.Cabinets)
            {
                if (item is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged += Item_PropertyChanged;
            }
        }
    }

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressIsModified) return;

        // Mark modified for add/remove/reset/move (user changed the collection)
        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace ||
            e.Action == NotifyCollectionChangedAction.Reset ||
            e.Action == NotifyCollectionChangedAction.Move)
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

        // On Reset (bulk load), NewItems is null — re-hook all current items
        if (e.Action == NotifyCollectionChangedAction.Reset && _cabinetService?.Cabinets != null)
        {
            foreach (var item in _cabinetService.Cabinets)
            {
                if (item is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged += Item_PropertyChanged;
            }
        }
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressIsModified) return;

        // UI-only flags — don't mark job as modified
        if (e.PropertyName is nameof(CabinetModel.IsSelected)
                           or nameof(CabinetModel.IsHighlighted)
                           or nameof(CabinetModel.Thumbnail))
            return;

        // Any property change on an item marks the job modified.
        IsModified = true;
    }

    public void RefreshSelectedCabinet()
    {
        OnPropertyChanged(nameof(SelectedCabinet));
        OnSelectedCabinetChanged(SelectedCabinet);
    }

    private JobCustomerInfo BuildCustomerInfo() => new()
    {
        CompanyName = POCustomerInfoVm.CompanyName,
        ContactName = POCustomerInfoVm.ContactName,
        PhoneNumber = POCustomerInfoVm.PhoneNumber,
        EMail = POCustomerInfoVm.EMail,
        Street = POCustomerInfoVm.Street,
        City = POCustomerInfoVm.City,
        ZipCode = POCustomerInfoVm.ZipCode
    };

    public void AutoSave()
    {
        _ = _autoSave.SaveRecoveryAsync();
    }

    [RelayCommand]
    private static void Help()
    {
        const string helpUrl = "https://corlanecabinetry.com/cabinet-order-form-help/";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = helpUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open help page.\n\n{ex.Message}", "Help", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
