using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    private async Task SaveJob()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Corlane Cabinet Order Form Files (*.cor)|*.cor",
            DefaultExt = "cor",
            FileName = CurrentJobName + ".cor",
            InitialDirectory = _defaults.GetFileDialogDirectory(CurrentJobPath)
        };

        Notify2("Saving job...", Brushes.Blue, 100000);

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _suppressIsModified = true;
                try
                {
                    var customer = BuildCustomerInfo();

                    await _cabinetService.SaveAsync(
                        dialog.FileName,
                        customer,
                        POCustomerInfoVm.QuotedTotalPrice,
                        submittedWithAppTitle: AppTitle);

                    Notify2($"{System.IO.Path.GetFileNameWithoutExtension(dialog.FileName)} Saved", Brushes.Green, 4000);
                    CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    CurrentJobPath = dialog.FileName;
                    IsModified = false;

                    _defaults.RememberFileDialogDirectory(dialog.FileName);

                    // Successful save — no need for recovery file
                    _autoSave.Stop();
                    AutoSaveService.DeleteRecoveryFile();
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
        else
        {
            Notify2("Save canceled", Brushes.Red, 2000);
        }
    }
}
