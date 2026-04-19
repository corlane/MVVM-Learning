using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    private async Task LoadJob()
    {
        if (IsModified)
        {
            var res = MessageBox.Show(
                "The current job has unsaved changes. Loading a new job will discard these changes. Do you want to continue?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Corlane Cabinet Order Form Files (*.cor)|*.cor",
            InitialDirectory = _defaults.GetFileDialogDirectory(CurrentJobPath)
        };

        Notify2("Loading job...", Brushes.Blue, 100000); // yes, 100 seconds - will be cleared on success

        if (dialog.ShowDialog() == true)
        {
            _suppressIsModified = true;

            try
            {
                try
                {
                    var job = await _cabinetService.LoadAsync(dialog.FileName);
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
                        POCustomerInfoVm.SubmittedWithAppTitle = job.SubmittedWithAppTitle;
                    }

                    Notify2($"{System.IO.Path.GetFileNameWithoutExtension(dialog.FileName)} Loaded", Brushes.Green, 4000);
                    CurrentJobName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    CurrentJobPath = dialog.FileName;
                    IsModified = false;

                    _defaults.RememberFileDialogDirectory(dialog.FileName);

                    // Clean load — no recovery needed
                    _autoSave.Stop();
                    AutoSaveService.DeleteRecoveryFile();
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
        else
        {
            // User cancelled load
            Notify2("Load canceled", Brushes.Red, 2000);
        }
    }
}
