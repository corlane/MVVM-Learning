using CorlaneCabinetOrderFormV3.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Periodically saves a recovery snapshot of the current job so work
/// can be recovered after an unexpected crash or OS freeze.
/// </summary>
internal sealed class AutoSaveService : IDisposable
{
    private static readonly string RecoveryDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CorlaneCabinetOrderFormV3");

    internal const string RecoveryFileName = "recovery.cor";

    internal static readonly string RecoveryFilePath =
        Path.Combine(RecoveryDirectory, RecoveryFileName);

    private readonly ICabinetService _cabinetService;
    private readonly DispatcherTimer _timer;
    private Func<JobCustomerInfo>? _customerInfoProvider;
    private Func<decimal>? _quotedPriceProvider;

    public AutoSaveService(ICabinetService cabinetService, TimeSpan? interval = null)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        _timer = new DispatcherTimer
        {
            Interval = interval ?? TimeSpan.FromMinutes(5)
        };
        _timer.Tick += async (_, _) => await SaveRecoveryAsync();
    }

    /// <summary>
    /// Provide callbacks so auto-save can capture customer info and price
    /// without taking a dependency on any ViewModel.
    /// </summary>
    public void Configure(Func<JobCustomerInfo> customerInfoProvider, Func<decimal> quotedPriceProvider)
    {
        _customerInfoProvider = customerInfoProvider;
        _quotedPriceProvider = quotedPriceProvider;
    }

    /// <summary>Start or restart the auto-save countdown.</summary>
    public void Start()
    {
        _timer.Stop();
        _timer.Start();
    }

    /// <summary>Stop the timer (e.g., after a manual save or New Job).</summary>
    public void Stop() => _timer.Stop();

    /// <summary>
    /// Write a recovery snapshot now. Called by the timer tick,
    /// but can also be called explicitly (e.g., before a risky operation).
    /// </summary>
    public async Task SaveRecoveryAsync()
    {
        try
        {
            if (_cabinetService.Cabinets.Count == 0)
                return;

            Directory.CreateDirectory(RecoveryDirectory);

            var customerInfo = _customerInfoProvider?.Invoke() ?? new JobCustomerInfo();
            var price = _quotedPriceProvider?.Invoke() ?? 0m;

            await _cabinetService.SaveAsync(RecoveryFilePath, customerInfo, price, submittedWithAppTitle: null)
                .ConfigureAwait(false);

            Debug.WriteLine($"[AutoSave] Recovery snapshot saved at {DateTime.Now:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AutoSave] Failed: {ex.Message}");
        }
    }

    /// <summary>Returns true if a recovery file exists from a previous session.</summary>
    public static bool HasRecoveryFile() => File.Exists(RecoveryFilePath);

    /// <summary>Delete the recovery file (call after successful manual save or New Job).</summary>
    public static void DeleteRecoveryFile()
    {
        try
        {
            if (File.Exists(RecoveryFilePath))
                File.Delete(RecoveryFilePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AutoSave] Could not delete recovery file: {ex.Message}");
        }
    }

    public void Dispose() => _timer.Stop();
}