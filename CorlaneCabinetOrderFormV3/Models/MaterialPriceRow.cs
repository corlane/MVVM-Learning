using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class MaterialPriceRow : ObservableObject
{
    [ObservableProperty]
    public partial string Species { get; set; } = "";

    [ObservableProperty]
    public partial decimal PricePerSqFt { get; set; }

    [ObservableProperty]
    public partial double SheetWidthIn { get; set; } = 48;

    [ObservableProperty]
    public partial double SheetLengthIn { get; set; } = 96;
}