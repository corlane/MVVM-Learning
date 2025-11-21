using MVVM_Learning.Models;
using System.Collections.ObjectModel;

namespace MVVM_Learning.Services;

public class CabinetService : ICabinetService
{
    public ObservableCollection<BaseCabinetModel> Cabinets { get; } = new();

    public void Add(BaseCabinetModel cabinet)
    {
        Cabinets.Add(cabinet);
    }
}