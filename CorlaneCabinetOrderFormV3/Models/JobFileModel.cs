using System;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Models;

public sealed class JobFileModel
{
    public ObservableCollection<CabinetModel> Cabinets { get; set; } = new();

    public JobCustomerInfo CustomerInfo { get; set; } = new();

    public decimal QuotedTotalPrice { get; set; }

    public DateTime? OrderedAtLocal { get; set; }

    // Saved metadata (optional -> backward compatible with older files)
    public string? SubmittedWithAppTitle { get; set; }

    /// <summary>PO Exception "Done" states, keyed by tab ID → list of row keys. Nullable for backward compat.</summary>
    public Dictionary<string, List<string>>? ExceptionDoneKeys { get; set; }
}