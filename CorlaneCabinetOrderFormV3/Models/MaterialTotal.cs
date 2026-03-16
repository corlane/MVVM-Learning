using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class MaterialTotal : ObservableObject
{
    [ObservableProperty]
    public partial string Species { get; set; } = "";

    [ObservableProperty]
    public partial double Quantity { get; set; }

    [ObservableProperty]
    public partial string Unit { get; set; } = "";

    [ObservableProperty]
    public partial decimal UnitPrice { get; set; }

    [ObservableProperty]
    public partial double SqFt { get; set; }

    partial void OnQuantityChanged(double oldValue, double newValue)
    {
        OnPropertyChanged(nameof(LineTotal));
    }

    partial void OnUnitPriceChanged(decimal oldValue, decimal newValue)
    {
        OnPropertyChanged(nameof(LineTotal));
    }

    public decimal LineTotal => Math.Round(UnitPrice * (decimal)Quantity, 2);
}