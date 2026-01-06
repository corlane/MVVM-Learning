using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POCustomerInfoViewModel : ObservableObject
{
    [ObservableProperty] public partial string? CompanyName { get; set; }
    [ObservableProperty] public partial string? ContactName { get; set; }
    [ObservableProperty] public partial string? PhoneNumber { get; set; }
    [ObservableProperty] public partial string? EMail { get; set; }
    [ObservableProperty] public partial string? Street { get; set; }
    [ObservableProperty] public partial string? City { get; set; }
    [ObservableProperty] public partial string? ZipCode { get; set; }

    [ObservableProperty] public partial decimal QuotedTotalPrice { get; set; }

    public string FormattedTotal => QuotedTotalPrice.ToString("C2");

    partial void OnQuotedTotalPriceChanged(decimal oldValue, decimal newValue)
    {
        OnPropertyChanged(nameof(FormattedTotal));
    }
}