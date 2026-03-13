using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

public interface ICabinetService
{
    ObservableCollection<CabinetModel> Cabinets { get; }
    void Add(CabinetModel cabinet);
    void Remove(CabinetModel cabinet);

    DateTime? OrderedAtLocal { get; set; }

    Task SaveAsync(
        string filePath,
        JobCustomerInfo customerInfo,
        decimal quotedTotalPrice,
        string? submittedWithAppTitle);

    Task<JobFileModel?> LoadAsync(string filePath);

    /// <summary>Runtime storage for PO exception Done-state keys, keyed by tab ID.</summary>
    Dictionary<string, HashSet<string>> ExceptionDoneKeys { get; }

    /// <summary>Raised when any PO exception Done button is toggled (for IsModified tracking).</summary>
    event Action? ExceptionDoneStateChanged;
    void RaiseExceptionDoneStateChanged();
}