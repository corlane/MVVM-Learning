using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class BaseCabinetModel : CabinetModel
{
    // Type-specific properties for BaseCabinetModel
    [ObservableProperty] public partial string LeftBackWidth { get; set; }
    [ObservableProperty] public partial string RightBackWidth { get; set; }
    [ObservableProperty] public partial string LeftFrontWidth { get; set; }
    [ObservableProperty] public partial string RightFrontWidth { get; set; }
    [ObservableProperty] public partial string LeftDepth { get; set; }
    [ObservableProperty] public partial string RightDepth { get; set; }
    [ObservableProperty] public partial string FrontWidth { get; set; }
    [ObservableProperty] public partial bool HasTK { get; set; }
    [ObservableProperty] public partial string TKHeight { get; set; }
    [ObservableProperty] public partial string TKDepth { get; set; }
    [ObservableProperty] public partial string DoorSpecies { get; set; }
    [ObservableProperty] public partial string CustomDoorSpecies { get; set; }
    [ObservableProperty] public partial string BackThickness { get; set; }
    [ObservableProperty] public partial string TopType { get; set; }
    [ObservableProperty] public partial int ShelfCount { get; set; }
    [ObservableProperty] public partial string ShelfDepth { get; set; }
    [ObservableProperty] public partial bool DrillShelfHoles { get; set; }
    [ObservableProperty] public partial int DoorCount { get; set; }
    [ObservableProperty] public partial string DoorGrainDir { get; set; }
    [ObservableProperty] public partial bool IncDoorsInList { get; set; }
    [ObservableProperty] public partial bool IncDoors { get; set; }
    [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
    [ObservableProperty] public partial string DrwFrontGrainDir { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontsInList { get; set; }
    [ObservableProperty] public partial bool IncDrwFronts { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxesInList { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxes { get; set; }
    [ObservableProperty] public partial bool DrillSlideHoles { get; set; }
    [ObservableProperty] public partial int DrwCount { get; set; }
    [ObservableProperty] public partial string DrwStyle { get; set; }
    [ObservableProperty] public partial string OpeningHeight1 { get; set; }
    [ObservableProperty] public partial string OpeningHeight2 { get; set; }
    [ObservableProperty] public partial string OpeningHeight3 { get; set; }
    [ObservableProperty] public partial string OpeningHeight4 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxOpening4 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening1 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening2 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening3 { get; set; }
    [ObservableProperty] public partial bool DrillSlideHolesOpening4 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening1 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening2 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening3 { get; set; }
    [ObservableProperty] public partial bool IncDrwBoxInListOpening4 { get; set; }
    [ObservableProperty] public partial string DrwFrontHeight1 { get; set; }
    [ObservableProperty] public partial string DrwFrontHeight2 { get; set; }
    [ObservableProperty] public partial string DrwFrontHeight3 { get; set; }
    [ObservableProperty] public partial string DrwFrontHeight4 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFront4 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList1 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList2 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList3 { get; set; }
    [ObservableProperty] public partial bool IncDrwFrontInList4 { get; set; }
    [ObservableProperty] public partial string LeftReveal { get; set; }
    [ObservableProperty] public partial string RightReveal { get; set; }
    [ObservableProperty] public partial string TopReveal { get; set; }
    [ObservableProperty] public partial string BottomReveal { get; set; }
    [ObservableProperty] public partial string GapWidth { get; set; }
    [ObservableProperty] public partial bool IncRollouts { get; set; }
    [ObservableProperty] public partial bool IncRolloutsInList { get; set; }
    [ObservableProperty] public partial int RolloutCount { get; set; }
    [ObservableProperty] public partial bool SinkCabinet { get; set; }
    [ObservableProperty] public partial bool TrashDrawer { get; set; }

    private void BumpGeometry() => BumpGeometryVersion();

    partial void OnLeftBackWidthChanged(string value) => BumpGeometry();
    partial void OnRightBackWidthChanged(string value) => BumpGeometry();
    partial void OnLeftFrontWidthChanged(string value) => BumpGeometry();
    partial void OnRightFrontWidthChanged(string value) => BumpGeometry();
    partial void OnLeftDepthChanged(string value) => BumpGeometry();
    partial void OnRightDepthChanged(string value) => BumpGeometry();
    partial void OnHasTKChanged(bool value) => BumpGeometry();
    partial void OnTKHeightChanged(string value) => BumpGeometry();
    partial void OnTKDepthChanged(string value) => BumpGeometry();
    partial void OnBackThicknessChanged(string value) => BumpGeometry();
    partial void OnTopTypeChanged(string value) => BumpGeometry();
    partial void OnShelfCountChanged(int value) => BumpGeometry();
    partial void OnShelfDepthChanged(string value) => BumpGeometry();

    partial void OnDoorCountChanged(int value) => BumpGeometry();
    partial void OnDoorSpeciesChanged(string value) => BumpGeometry();
    partial void OnCustomDoorSpeciesChanged(string value) => BumpGeometry();
    partial void OnDoorGrainDirChanged(string value) => BumpGeometry();

    partial void OnDrwCountChanged(int value) => BumpGeometry();
    partial void OnDrwStyleChanged(string value) => BumpGeometry();
    partial void OnOpeningHeight1Changed(string value) => BumpGeometry();
    partial void OnOpeningHeight2Changed(string value) => BumpGeometry();
    partial void OnOpeningHeight3Changed(string value) => BumpGeometry();
    partial void OnOpeningHeight4Changed(string value) => BumpGeometry();
    partial void OnDrwFrontHeight1Changed(string value) => BumpGeometry();
    partial void OnDrwFrontHeight2Changed(string value) => BumpGeometry();
    partial void OnDrwFrontHeight3Changed(string value) => BumpGeometry();
    partial void OnDrwFrontHeight4Changed(string value) => BumpGeometry();

    partial void OnLeftRevealChanged(string value) => BumpGeometry();
    partial void OnRightRevealChanged(string value) => BumpGeometry();
    partial void OnTopRevealChanged(string value) => BumpGeometry();
    partial void OnBottomRevealChanged(string value) => BumpGeometry();
    partial void OnGapWidthChanged(string value) => BumpGeometry();
}
