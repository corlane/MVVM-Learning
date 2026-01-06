using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class EdgeBandPriceRow : ObservableObject
{
    [ObservableProperty] private string _species = "";
    [ObservableProperty] private decimal _pricePerFt;
}