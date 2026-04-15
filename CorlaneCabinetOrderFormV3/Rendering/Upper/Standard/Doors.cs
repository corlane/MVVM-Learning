using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildDoors(Model3DGroup cabinet, UpperCabinetModel upperCab, bool doorsHidden, Func<string?, string?, string> resolveDoorSpeciesForTotals, Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow, double MaterialThickness34, string doorEdgebandingSpecies, double width, double height, double depth, double upperDoorGap, double doorLeftReveal, double doorRightReveal, double doorTopReveal, double doorBottomReveal, double doorSideReveal, bool topDeck90, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints)
    {
        door1 = null;
        door2 = null;
        doorPoints = null;

        // Doors
        if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
        {

            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

            double doorWidth = width - (doorSideReveal * 2);
            double doorHeight = height - doorTopReveal - doorBottomReveal;

            if (upperCab.DoorCount == 1)
            {
                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth,doorHeight,0),
                        new (0,doorHeight,0)
                    ];

                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    if (!doorsHidden) cabinet.Children.Add(door1);
                }
            }

            if (upperCab.DoorCount == 2)
            {
                doorWidth = (doorWidth / 2) - (upperDoorGap / 2);

                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth, doorHeight, 0),
                        new (0,doorHeight,0)
                    ];

                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);

                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);

                    if (!doorsHidden)
                    {
                        cabinet.Children.Add(door1);
                        cabinet.Children.Add(door2);
                    }
                }
            }
        }

    }
}