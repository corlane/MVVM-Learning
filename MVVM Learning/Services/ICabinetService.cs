using MVVM_Learning.Models;
using System.Collections.ObjectModel;

namespace MVVM_Learning.Services;

public interface ICabinetService
{
    ObservableCollection<CabinetModel> Cabinets { get; }
    void Add(CabinetModel cabinet);

    Task SaveAsync(string filePath);  // New: Save to file
    Task LoadAsync(string filePath);  // New: Load from file
}