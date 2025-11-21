using MVVM_Learning.Models;
using System.Collections.ObjectModel;

namespace MVVM_Learning.Services;

public interface ICabinetService
{
    ObservableCollection<BaseCabinetModel> Cabinets { get; }
    void Add(BaseCabinetModel cabinet);
}