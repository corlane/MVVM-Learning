using MVVM_Learning.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVVM_Learning.Services;

public class CabinetService : ICabinetService
{
    public ObservableCollection<CabinetModel> Cabinets { get; } = new();  // Holds mixed subtypes

    public void Add(CabinetModel cabinet)  // Works for any subtype
    {
        Cabinets.Add(cabinet);
    }

    public void Remove(CabinetModel cabinet)
    {
        Cabinets.Remove(cabinet);
    }


    public async Task SaveAsync(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,  // Pretty-print JSON for readability
            IncludeFields = true   // If needed for any fields
        };
        var json = JsonSerializer.Serialize(Cabinets, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task LoadAsync(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var json = await File.ReadAllTextAsync(filePath);
        var loadedCabinets = JsonSerializer.Deserialize<ObservableCollection<CabinetModel>>(json);
        if (loadedCabinets != null)
        {
            Cabinets.Clear();
            foreach (var cabinet in loadedCabinets)
            {
                Cabinets.Add(cabinet);  // Auto-deserializes to correct subtypes
            }
        }
    }
}