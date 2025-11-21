using MVVM_Learning.Models;
using System.Collections.ObjectModel;

namespace MVVM_Learning.Services;

public class CabinetService : ICabinetService
{
    public ObservableCollection<CabinetModel> Cabinets { get; } = new();

    public void Add(CabinetModel cabinet)
    {
        Cabinets.Add(cabinet);
    }
}