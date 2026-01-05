using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class CabinetModel
{
    // Temporary UI-only flag used to animate/highlight a row when recently updated.
    [ObservableProperty]
    public partial bool IsHighlighted { get; set; }
}