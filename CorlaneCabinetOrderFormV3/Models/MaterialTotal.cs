using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class MaterialTotal : ObservableObject
{
    [ObservableProperty] private string _species = "";
    [ObservableProperty] private double _quantity; // ft² for panels, ft for edgebanding
    [ObservableProperty] private string _unit = ""; // "ft²" or "ft"
    [ObservableProperty] private decimal _unitPrice;

    partial void OnQuantityChanged(double oldValue, double newValue)
    {
        OnPropertyChanged(nameof(LineTotal));
    }

    partial void OnUnitPriceChanged(decimal oldValue, decimal newValue)
    {
        OnPropertyChanged(nameof(LineTotal));
    }

    public decimal LineTotal => Math.Round(_unitPrice * (decimal)_quantity, 2);
}