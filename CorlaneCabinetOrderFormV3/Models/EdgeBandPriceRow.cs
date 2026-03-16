using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class EdgeBandPriceRow : ObservableObject
{
    [ObservableProperty]
    public partial string Species { get; set; } = "";

    [ObservableProperty]
    public partial decimal PricePerFt { get; set; }
}