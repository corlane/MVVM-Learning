using CorlaneCabinetOrderFormV3.Models;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

// ICabinetService.cs
// Defines the contract for the cabinet data service consumed by all ViewModels that need
// access to the current job's cabinet list or persistence operations.
//
// Abstracting behind this interface allows ViewModels to remain decoupled from the
// concrete CabinetService implementation, making them easier to test and the service
// easier to swap or mock.
//
// Contract covers:
//   - Cabinets collection: the master ObservableCollection<CabinetModel> that drives
//     all cabinet list UI across the app
//   - Add / Remove: mutating the cabinet list through controlled entry points
//     (Add enforces duplicate name rules in the concrete implementation)
//   - SaveAsync / LoadAsync: full job file persistence including customer info,
//     quoted price, order timestamp, and exception done-state
//   - OrderedAtLocal: tracks when the job was submitted/ordered
//   - ExceptionDoneKeys / RaiseExceptionDoneStateChanged: runtime storage and change
//     notification for PO exception "Done" button state, used by IsModified tracking
//   - AccumulateMaterialAndEdgeTotals / AccumulateAllMaterialAndEdgeTotals: triggers
//     the rendering pipeline to recompute per-cabinet material area and edgebanding
//     totals, consumed by DoorSizesListViewModel and DrawerBoxSizesListViewModel

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

    /// <summary>Resets and re-accumulates material area and edgebanding totals for a single cabinet.</summary>
    void AccumulateMaterialAndEdgeTotals(CabinetModel cab);

    /// <summary>Resets and re-accumulates material area and edgebanding totals for every cabinet in the list.</summary>
    void AccumulateAllMaterialAndEdgeTotals();
}