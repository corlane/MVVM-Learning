using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVM_Learning.Models;

public partial class BaseCabinetModel : ObservableObject
{
    [ObservableProperty] private string width = "";
    [ObservableProperty] private string height = "";
    [ObservableProperty] private string depth = "";
}