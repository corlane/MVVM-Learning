using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVM_Learning.Models;

public partial class BaseCabinetModel : CabinetModel
{
    // Type-specific properties for BaseCabinetModel
    [ObservableProperty] public partial string ToeKickHeight { get; set; } = "4";  // Type-specific example
}