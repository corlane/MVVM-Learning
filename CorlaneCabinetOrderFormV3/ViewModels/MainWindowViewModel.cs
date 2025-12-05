using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel(ICabinetService cabinetService) : ObservableValidator
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public MainWindowViewModel() : this(new CabinetService())
    {
        // empty constructor for design-time support

    }


    [ObservableProperty] public partial bool IsDarkMode { get; set; }
    

    private readonly ICabinetService _cabinetService = cabinetService;

    [RelayCommand]
    private async Task SaveJob()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _cabinetService.SaveAsync(dialog.FileName);
                MessageBox.Show("Job saved successfully!", "Success");
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
            Filter = "JSON Files (*.json)|*.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _cabinetService.LoadAsync(dialog.FileName);
                MessageBox.Show("Job loaded successfully!", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job: {ex.Message}", "Error");
            }
        }
    }


    [ObservableProperty]
    public partial int SelectedTabIndex { get; set; } = 0;


    [ObservableProperty]
    public partial CabinetModel? CurrentPreviewCabinet { get; set; }


    [ObservableProperty]
    public partial CabinetModel? SelectedCabinet { get; set; }

    partial void OnSelectedCabinetChanged(CabinetModel? value)
    {
        if (value == null)
        {
            // DON'T clear the preview when deselecting - keep the last cabinet visible
            // CurrentPreviewCabinet = null;
            return;
        }

        // Determine target tab but don't switch if already there
        int targetTab = value switch
        {
            BaseCabinetModel => 0,
            UpperCabinetModel => 1,
            FillerModel => 2,
            PanelModel => 3,
            _ => SelectedTabIndex
        };

        // Only change tab if different (prevents feedback loop)
        if (SelectedTabIndex != targetTab)
        {
            SelectedTabIndex = targetTab;
        }

        CurrentPreviewCabinet = value;
    }
}