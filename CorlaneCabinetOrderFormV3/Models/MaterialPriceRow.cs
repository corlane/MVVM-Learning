using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class MaterialPriceRow : ObservableObject
{
    [ObservableProperty] private string _species = "";
    [ObservableProperty] private decimal _pricePerSqFt;
    [ObservableProperty] private double _sheetWidthIn = 48;
    [ObservableProperty] private double _sheetLengthIn = 96;
}