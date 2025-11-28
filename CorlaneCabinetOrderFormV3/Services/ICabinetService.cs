using CorlaneCabinetOrderFormV3.Models;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

public interface ICabinetService
{
    ObservableCollection<CabinetModel> Cabinets { get; }
    void Add(CabinetModel cabinet);
    void Remove(CabinetModel cabinet);

    Task SaveAsync(string filePath);  // New: Save to file
    Task LoadAsync(string filePath);  // New: Load from file
}