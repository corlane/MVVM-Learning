using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class BaseCabinetViewModel : ObservableValidator
    {
        private void UpdatePreview() // Update 3D cabinet model preview
        {
            var model = new BaseCabinetModel
            {
                Style = Style,
                Width = Width,
                Height = Height,
                Depth = Depth,
                Species = Species,
                EBSpecies = EBSpecies,

                TKHeight = TKHeight,  // Subtype-specific
                LeftBackWidth = LeftBackWidth,
                RightBackWidth = RightBackWidth,
                LeftFrontWidth = LeftFrontWidth,
                RightFrontWidth = RightFrontWidth,
                LeftDepth = LeftDepth,
                RightDepth = RightDepth,
                HasTK = HasTK,
                TKDepth = TKDepth,
                DoorSpecies = DoorSpecies,
                CustomDoorSpecies = CustomDoorSpecies,
                BackThickness = BackThickness,
                TopType = TopType,
                ShelfCount = ShelfCount,
                ShelfDepth = ShelfDepth,
                DoorCount = DoorCount,
                DoorGrainDir = DoorGrainDir,
                IncDoors = IncDoors,
                DrwFrontGrainDir = DrwFrontGrainDir,
                IncDrwFronts = IncDrwFronts,
                IncDrwBoxes = IncDrwBoxes,
                DrwCount = DrwCount,
                OpeningHeight1 = OpeningHeight1,
                OpeningHeight2 = OpeningHeight2,
                OpeningHeight3 = OpeningHeight3,
                OpeningHeight4 = OpeningHeight4,
                IncDrwBoxOpening1 = IncDrwBoxOpening1,
                IncDrwBoxOpening2 = IncDrwBoxOpening2,
                IncDrwBoxOpening3 = IncDrwBoxOpening3,
                IncDrwBoxOpening4 = IncDrwBoxOpening4,
                DrwFrontHeight1 = DrwFrontHeight1,
                DrwFrontHeight2 = DrwFrontHeight2,
                DrwFrontHeight3 = DrwFrontHeight3,
                DrwFrontHeight4 = DrwFrontHeight4,
                IncDrwFront1 = IncDrwFront1,
                IncDrwFront2 = IncDrwFront2,
                IncDrwFront3 = IncDrwFront3,
                IncDrwFront4 = IncDrwFront4,
                LeftReveal = LeftReveal,
                RightReveal = RightReveal,
                TopReveal = TopReveal,
                BottomReveal = BottomReveal,
                GapWidth = GapWidth,
                IncRollouts = IncRollouts,
                RolloutCount = RolloutCount,
                RolloutStyle = RolloutStyle,
                DrwStyle = DrwStyle,
                SinkCabinet = SinkCabinet,
                TrashDrawer = TrashDrawer,
                IncTrashDrwBox = IncTrashDrwBox,
                DrillHingeHoles = DrillHingeHoles,
                DrillShelfHoles = DrillShelfHoles,
                DrillSlideHolesOpening1 = DrillSlideHolesOpening1,
                DrillSlideHolesOpening2 = DrillSlideHolesOpening2,
                DrillSlideHolesOpening3 = DrillSlideHolesOpening3,
                DrillSlideHolesOpening4 = DrillSlideHolesOpening4
            };

            // Request preview using the tab index owner token (Base tab = 0)
            _previewService?.RequestPreview(0, model);
        }

        // Properties that affect 3D preview geometry — only these trigger UpdatePreview
        private static readonly HashSet<string> s_previewProperties = new(StringComparer.Ordinal)
        {
            nameof(Style), nameof(Width), nameof(Height), nameof(Depth),
            nameof(Species), nameof(EBSpecies),
            nameof(TKHeight), nameof(TKDepth), nameof(HasTK),
            nameof(LeftBackWidth), nameof(RightBackWidth),
            nameof(LeftFrontWidth), nameof(RightFrontWidth),
            nameof(LeftDepth), nameof(RightDepth),
            nameof(DoorSpecies), nameof(BackThickness), nameof(TopType),
            nameof(ShelfCount), nameof(ShelfDepth),
            nameof(DoorCount), nameof(DoorGrainDir),
            nameof(IncDoors), nameof(IncDrwFronts), nameof(IncDrwBoxes),
            nameof(DrwCount), nameof(DrwFrontGrainDir), nameof(DrwStyle),
            nameof(OpeningHeight1), nameof(OpeningHeight2), nameof(OpeningHeight3), nameof(OpeningHeight4),
            nameof(IncDrwBoxOpening1), nameof(IncDrwBoxOpening2), nameof(IncDrwBoxOpening3), nameof(IncDrwBoxOpening4),
            nameof(DrwFrontHeight1), nameof(DrwFrontHeight2), nameof(DrwFrontHeight3), nameof(DrwFrontHeight4),
            nameof(IncDrwFront1), nameof(IncDrwFront2), nameof(IncDrwFront3), nameof(IncDrwFront4),
            nameof(LeftReveal), nameof(RightReveal), nameof(TopReveal), nameof(BottomReveal), nameof(GapWidth),
            nameof(IncRollouts), nameof(RolloutCount), nameof(RolloutStyle),
            nameof(SinkCabinet), nameof(TrashDrawer), nameof(IncTrashDrwBox),
            nameof(DrillHingeHoles), nameof(DrillShelfHoles),
            nameof(DrillSlideHolesOpening1), nameof(DrillSlideHolesOpening2),
            nameof(DrillSlideHolesOpening3), nameof(DrillSlideHolesOpening4),
        };

    }
}
