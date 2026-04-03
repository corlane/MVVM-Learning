using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    // For 3D model:
    private void UpdatePreview()
    {
        var model = new UpperCabinetModel
        {
            Style = Style,
            Width = Width,
            Height = Height,
            Depth = Depth,
            Species = Species,
            EBSpecies = EBSpecies,
            LeftBackWidth = LeftBackWidth,
            RightBackWidth = RightBackWidth,
            LeftFrontWidth = LeftFrontWidth,
            RightFrontWidth = RightFrontWidth,
            LeftDepth = LeftDepth,
            RightDepth = RightDepth,
            DoorSpecies = DoorSpecies,
            CustomDoorSpecies = CustomDoorSpecies,
            BackThickness = BackThickness,
            ShelfCount = ShelfCount,
            DrillShelfHoles = DrillShelfHoles,
            DoorCount = DoorCount,
            DoorGrainDir = DoorGrainDir,
            DrillHingeHoles = DrillHingeHoles,
            IncDoors = IncDoors,
            LeftReveal = LeftReveal,
            RightReveal = RightReveal,
            TopReveal = TopReveal,
            BottomReveal = BottomReveal,
            GapWidth = GapWidth
        };

        // Request preview using the tab index owner token (Upper tab = 1)
        _previewService?.RequestPreview(1, model);
    }

    // Properties that affect 3D preview geometry — only these trigger UpdatePreview
    private static readonly HashSet<string> s_previewProperties = new(StringComparer.Ordinal)
    {
        nameof(Style), nameof(Width), nameof(Height), nameof(Depth),
        nameof(Species), nameof(EBSpecies),
        nameof(LeftBackWidth), nameof(RightBackWidth),
        nameof(LeftFrontWidth), nameof(RightFrontWidth),
        nameof(LeftDepth), nameof(RightDepth),
        nameof(DoorSpecies), nameof(BackThickness),
        nameof(ShelfCount), nameof(DrillShelfHoles),
        nameof(DoorCount), nameof(DoorGrainDir),
        nameof(IncDoors), nameof(DrillHingeHoles),
        nameof(LeftReveal), nameof(RightReveal),
        nameof(TopReveal), nameof(BottomReveal), nameof(GapWidth),
    };

}
