using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MVVM_Learning.Models;

public partial class CabinetListModel : ObservableValidator
{
    [ObservableProperty]
    public partial ObservableCollection<CabinetModel> CabinetList { get; set; } = new();

}
