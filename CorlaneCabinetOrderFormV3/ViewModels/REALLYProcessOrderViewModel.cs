using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Services;
using System;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class REALLYProcessOrderViewModel : ObservableValidator
{
    public REALLYProcessOrderViewModel()
    {
        // design-time
    }

    public REALLYProcessOrderViewModel(POCustomerInfoViewModel poCustomerInfoVm)
    {
        POCustomerInfoVm = poCustomerInfoVm ?? throw new ArgumentNullException(nameof(poCustomerInfoVm));
    }

    public POCustomerInfoViewModel POCustomerInfoVm { get; }
}