using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVM_Learning.Models;

public partial class CabinetModel : ObservableObject
{
    [ObservableProperty] private string width = "";
    [ObservableProperty] private string height = "";
    [ObservableProperty] private string depth = "";
}