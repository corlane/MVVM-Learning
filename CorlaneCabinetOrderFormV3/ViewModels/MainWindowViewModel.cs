using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel(ICabinetService cabinetService) : ObservableValidator
{
    public MainWindowViewModel() : this(new CabinetService())
    {
        // empty constructor for design-time support
    }


    private readonly ICabinetService _cabinetService = cabinetService;

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
                await _cabinetService.SaveAsync(dialog.FileName);
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
                await _cabinetService.LoadAsync(dialog.FileName);
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
        // Use the tab index as the owner token (or map index → viewmodel instance)
        previewSvc.SetActiveOwner(value);
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
