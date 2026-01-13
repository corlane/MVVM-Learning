using CorlaneCabinetOrderFormV3.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace CorlaneCabinetOrderFormV3.Services;

public class CabinetService : ICabinetService
{
    public ObservableCollection<CabinetModel> Cabinets { get; } = new();  // Holds mixed subtypes

    public DateTime? OrderedAtLocal { get; set; }

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

    public async Task SaveAsync(string filePath, JobCustomerInfo customerInfo, decimal quotedTotalPrice)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };

        var job = new JobFileModel
        {
            Cabinets = new ObservableCollection<CabinetModel>(Cabinets.ToList()),
            CustomerInfo = customerInfo ?? new JobCustomerInfo(),
            QuotedTotalPrice = quotedTotalPrice,
            OrderedAtLocal = OrderedAtLocal
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
                OrderedAtLocal = null
            };
        }
        else
        {
            loadedJob = JsonSerializer.Deserialize<JobFileModel>(json, options);
        }

        if (loadedJob == null) return null;

        OrderedAtLocal = loadedJob.OrderedAtLocal;

        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Cabinets.Clear();
            }, System.Windows.Threading.DispatcherPriority.DataBind);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var cabinet in loadedJob.Cabinets)
                {
                    Cabinets.Add(cabinet);
                }
            }, System.Windows.Threading.DispatcherPriority.DataBind);
        }
        else
        {
            Cabinets.Clear();
            foreach (var cabinet in loadedJob.Cabinets)
            {
                Cabinets.Add(cabinet);
            }
        }

        return loadedJob;
    }
}


