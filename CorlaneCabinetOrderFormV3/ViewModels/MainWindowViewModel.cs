using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel(ICabinetService cabinetService) : ObservableValidator
{
    public MainWindowViewModel() : this(new CabinetService())
    {
        // empty constructor for design-time support
    }

    private readonly ICabinetService _cabinet_service = cabinetService;

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
                await _cabinet_service.SaveAsync(dialog.FileName);
                MessageBox.Show("Job saved successfully!", "Success");
                CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
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
            try
            {
                await _cabinet_service.LoadAsync(dialog.FileName);
                //MessageBox.Show("Job loaded successfully!", "Success");
                CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job: {ex.Message}", "Error");
            }
        }
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
                    break;
                case 1:
                    (UpperCabinetVm as IValidatableViewModel)?.RunValidationVisible();
                    break;
                case 2:
                    (FillerVm as IValidatableViewModel)?.RunValidationVisible();
                    break;
                case 3:
                    (PanelVm as IValidatableViewModel)?.RunValidationVisible();
                    break;
                case 4:
                    (PlaceOrderVm as IValidatableViewModel)?.RunValidationVisible();
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


}