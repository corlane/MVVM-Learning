using CorlaneCabinetOrderFormV3.Models;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

public interface ICabinetService
{
    ObservableCollection<CabinetModel> Cabinets { get; }
    void Add(CabinetModel cabinet);
    void Remove(CabinetModel cabinet);

    Task SaveAsync(string filePath, JobCustomerInfo customerInfo, decimal quotedTotalPrice);
    Task<JobFileModel?> LoadAsync(string filePath);
}