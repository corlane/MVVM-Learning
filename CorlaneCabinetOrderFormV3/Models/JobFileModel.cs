using System;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Models;

public sealed class JobFileModel
{
    public ObservableCollection<CabinetModel> Cabinets { get; set; } = new();

    public JobCustomerInfo CustomerInfo { get; set; } = new();

    public decimal QuotedTotalPrice { get; set; }

    public DateTime? OrderedAtLocal { get; set; }
}