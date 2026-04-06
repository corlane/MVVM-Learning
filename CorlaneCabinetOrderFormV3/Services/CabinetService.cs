using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.Services;

public class CabinetService : ICabinetService
{
    public ObservableCollection<CabinetModel> Cabinets { get; } = new BulkObservableCollection<CabinetModel>();

    public DateTime? OrderedAtLocal { get; set; }

    public Dictionary<string, HashSet<string>> ExceptionDoneKeys { get; } = new();

    public event Action? ExceptionDoneStateChanged;
    public void RaiseExceptionDoneStateChanged() => ExceptionDoneStateChanged?.Invoke();

    // ── Staleness tracking for AccumulateAll ──────────────────────────
    private bool _totalsStale = true;
    private bool _suppressStale;   // prevents re-staleness during accumulation
    private bool _isAccumulating;  // reentrancy guard against Invoke message-pump

    public CabinetService()
    {
        ((INotifyCollectionChanged)Cabinets).CollectionChanged += OnCabinetsCollectionChanged;
    }

    private void OnCabinetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Unhook removed items
        if (e.OldItems is not null)
        {
            foreach (CabinetModel cab in e.OldItems)
                cab.PropertyChanged -= OnCabinetItemPropertyChanged;
        }

        // Hook new items
        if (e.NewItems is not null)
        {
            foreach (CabinetModel cab in e.NewItems)
                cab.PropertyChanged += OnCabinetItemPropertyChanged;
        }

        // Reset (bulk load): re-hook all current items
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var cab in Cabinets)
                cab.PropertyChanged += OnCabinetItemPropertyChanged;
        }

        if (!_suppressStale)
            _totalsStale = true;
    }

    private void OnCabinetItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_suppressStale)
            _totalsStale = true;
    }
    // ──────────────────────────────────────────────────────────────────

    public void Add(CabinetModel cabinet)
    {
        if (cabinet is null) throw new ArgumentNullException(nameof(cabinet));

        // Duplicate names are not allowed (ignore blank/null/whitespace).
        var newName = (cabinet.Name ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            bool nameExists = Cabinets.Any(c =>
                c != cabinet &&
                string.Equals((c.Name ?? "").Trim(), newName, StringComparison.OrdinalIgnoreCase));

            if (nameExists)
            {
                throw new InvalidOperationException($"Duplicate cabinet name '{newName}' is not allowed.");
            }
        }

        Cabinets.Add(cabinet);
    }

    public void Remove(CabinetModel cabinet)
    {
        Cabinets.Remove(cabinet);
    }

    public async Task SaveAsync(
        string filePath,
        JobCustomerInfo customerInfo,
        decimal quotedTotalPrice,
        string? submittedWithAppTitle)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        // Convert HashSet → List for JSON
        Dictionary<string, List<string>>? doneKeysForFile = null;
        if (ExceptionDoneKeys.Count > 0)
        {
            doneKeysForFile = new();
            foreach (var kvp in ExceptionDoneKeys)
            {
                if (kvp.Value.Count > 0)
                    doneKeysForFile[kvp.Key] = kvp.Value.ToList();
            }
        }

        var job = new JobFileModel
        {
            Cabinets = new ObservableCollection<CabinetModel>(Cabinets.ToList()),
            CustomerInfo = customerInfo ?? new JobCustomerInfo(),
            QuotedTotalPrice = quotedTotalPrice,
            OrderedAtLocal = OrderedAtLocal,
            SubmittedWithAppTitle = submittedWithAppTitle,
            ExceptionDoneKeys = doneKeysForFile
        };

        var json = JsonSerializer.Serialize(job, options);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    public async Task<JobFileModel?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
        };

        var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return null;

        JobFileModel? loadedJob;

        // Backward compat: old files were just an array of cabinets.
        if (json.TrimStart().StartsWith('['))
        {
            var loadedCabinets = JsonSerializer.Deserialize<ObservableCollection<CabinetModel>>(json, options) ?? new();
            loadedJob = new JobFileModel
            {
                Cabinets = loadedCabinets,
                CustomerInfo = new JobCustomerInfo(),
                QuotedTotalPrice = 0m,
                OrderedAtLocal = null,
                SubmittedWithAppTitle = null
            };
        }
        else
        {
            loadedJob = JsonSerializer.Deserialize<JobFileModel>(json, options);
        }

        if (loadedJob == null) return null;

        OrderedAtLocal = loadedJob.OrderedAtLocal;

        // Restore Done keys BEFORE adding cabinets (so VMs see them when CollectionChanged fires Refresh)
        ExceptionDoneKeys.Clear();
        if (loadedJob.ExceptionDoneKeys != null)
        {
            foreach (var kvp in loadedJob.ExceptionDoneKeys)
            {
                ExceptionDoneKeys[kvp.Key] = new HashSet<string>(kvp.Value);
            }
        }

        var bulk = (BulkObservableCollection<CabinetModel>)Cabinets;

        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                bulk.Clear();
                bulk.AddRange(loadedJob.Cabinets);
            }, System.Windows.Threading.DispatcherPriority.DataBind);
        }
        else
        {
            bulk.Clear();
            bulk.AddRange(loadedJob.Cabinets);
        }

        return loadedJob;
    }

    public void AccumulateMaterialAndEdgeTotals(CabinetModel cab)
    {
        if (cab == null) return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        // When already on the UI thread, run directly to avoid Dispatcher.Invoke
        // message-pumping which can cause reentrant execution of other Background-
        // priority work (e.g., debounced Rebuilds from size-list VMs).
        if (dispatcher.CheckAccess())
        {
            DoAccumulate(cab);
        }
        else
        {
            // Off UI thread — marshal synchronously
            dispatcher.Invoke(() => DoAccumulate(cab), DispatcherPriority.Background);
        }
    }

    private static void DoAccumulate(CabinetModel cab)
    {
        try
        {
            cab.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Catch] AccumulateTotals for '{cab.Name}': {ex.Message}");
        }
    }

    public void AccumulateAllMaterialAndEdgeTotals()
    {
        // Skip if totals are already current, or if we're already mid-accumulation
        // (reentrancy via Dispatcher message-pump processing debounced Rebuilds).
        if (!_totalsStale || _isAccumulating) return;

        // Mark not-stale BEFORE the loop so any reentrant caller sees fresh state
        _totalsStale = false;
        _isAccumulating = true;
        _suppressStale = true;
        try
        {
            foreach (var cab in Cabinets)
            {
                AccumulateMaterialAndEdgeTotals(cab);
            }
        }
        finally
        {
            _isAccumulating = false;
            _suppressStale = false;
        }
    }
}