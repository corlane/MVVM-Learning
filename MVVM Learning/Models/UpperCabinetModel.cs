using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVM_Learning.Models;

public partial class UpperCabinetModel : ObservableObject
{
    [ObservableProperty]
    public partial string Width { get; set; } = "";

    [ObservableProperty]
    public partial string Height { get; set; } = "";

    [ObservableProperty]
    public partial string Depth { get; set; } = "";
}