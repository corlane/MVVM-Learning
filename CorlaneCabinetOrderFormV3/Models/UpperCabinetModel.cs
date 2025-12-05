using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class UpperCabinetModel : CabinetModel
{
    // Type-specific properties for UpperCabinetModel
    [ObservableProperty] public partial string LeftBackWidth { get; set; } = "";
    [ObservableProperty] public partial string RightBackWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string RightFrontWidth { get; set; } = "";
    [ObservableProperty] public partial string LeftDepth { get; set; } = "";
    [ObservableProperty] public partial string RightDepth { get; set; } = "";
    [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty] public partial string BackThickness { get; set; } = "";
    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }
    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; } = "";
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty] public partial string RightReveal { get; set; } = "";
    [ObservableProperty] public partial string TopReveal { get; set; } = "";
    [ObservableProperty] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty] public partial string GapWidth { get; set; } = "";


}