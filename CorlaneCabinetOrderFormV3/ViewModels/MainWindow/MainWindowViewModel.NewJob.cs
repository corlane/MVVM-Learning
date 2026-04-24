using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    // New: Create a fresh job state — clear cabinets, reset UI state and recreate tab VMs so they match freshly booted defaults.
    [RelayCommand]
    private void NewJob()
    {
        _suppressIsModified = true;
        try
        {
            // If nothing to clear, be quick about it
            if ((_cabinetService.Cabinets == null || _cabinetService.Cabinets.Count == 0) && CurrentJobName == "Untitled Job")
            {
                NotifyMainWindow("Nothing to clear", Brushes.Gray);
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
                _cabinetService.Cabinets!.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Catch] NewJob: Cabinets.Clear failed: {ex.Message}");
            }

            // 2) Reset main-window state
            CurrentJobName = "Untitled Job";
            CurrentJobPath = null;
            SelectedCabinet = null;

            // 3) Clear preview immediately
            try
            {
                _previewSvc.ClearPreview();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Catch] NewJob: ClearPreview failed: {ex.Message}");
            }

            // 4) Reset tab VMs to default state in-place.
            //    These are DI singletons, so nulling the backing field and re-resolving
            //    returns the SAME instance — no constructor re-runs. Instead, reset them directly.
            BaseCabinetVm.ResetToNewJob();
            UpperCabinetVm.ResetToNewJob();
            FillerVm.ResetToNewJob();
            PanelVm.ResetToNewJob();

            (_placeOrderVm as IDisposable)?.Dispose();
            _placeOrderVm = null;
            _defaultsVm = null;
            _materialPricesVm = null;
            _processOrderVm = null;

            // Reset persistent "ordered" state for the new job
            _cabinetService.OrderedAtLocal = null;
            _cabinetService.ExceptionDoneKeys.Clear();

            try
            {
                PlaceOrderVm.OrderedAtLocal = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Catch] NewJob: Reset OrderedAtLocal failed: {ex.Message}");
            }

            OnPropertyChanged(nameof(PlaceOrderVm));
            OnPropertyChanged(nameof(DefaultsVm));
            OnPropertyChanged(nameof(MaterialPricesVm));
            OnPropertyChanged(nameof(ProcessOrderVm));

            // 4b) Now that VMs are reset, force tab logic so the BaseCabinetVm's
            //     default preview is generated
            if (SelectedTabIndex == 0)
                OnSelectedTabIndexChanged(0);
            else
                SelectedTabIndex = 0;

            // 5) Ensure PlaceOrder tab's transient state is fresh (material totals, pricing)
            try
            {
                var po = PlaceOrderVm;
                po.MaterialTotals.Clear();
                po.TotalPrice = 0m;

                // Reset customer info to persisted defaults (if any)

                po.CompanyName = _defaults.CompanyName;
                po.ContactName = _defaults.ContactName;
                po.PhoneNumber = _defaults.PhoneNumber;
                po.EMail = _defaults.EMail;
                po.Street = _defaults.Street;
                po.City = _defaults.City;
                po.ZipCode = _defaults.ZipCode;

                // ALSO reset the job-file customer info VM (this is what Load/SaveJob uses)
                var customerInfo = POCustomerInfoVm;
                customerInfo.CompanyName = _defaults.CompanyName;
                customerInfo.ContactName = _defaults.ContactName;
                customerInfo.PhoneNumber = _defaults.PhoneNumber;
                customerInfo.EMail = _defaults.EMail;
                customerInfo.Street = _defaults.Street;
                customerInfo.City = _defaults.City;
                customerInfo.ZipCode = _defaults.ZipCode;
                customerInfo.QuotedTotalPrice = 0m;
                customerInfo.SubmittedWithAppTitle = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Catch] NewJob: Reset PlaceOrder state failed: {ex.Message}");
            }

            // 6) New job — clear recovery file
            _autoSave.Stop();
            AutoSaveService.DeleteRecoveryFile();
        }
        finally
        {
            _suppressIsModified = false;
        }

        IsModified = false;
        // 7) Final user feedback
        NotifyMainWindow("New job ready", Brushes.Green, 3000);
    }
}
