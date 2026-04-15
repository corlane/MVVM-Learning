using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildDoors(Model3DGroup cabinet, UpperCabinetModel upperCab, bool doorsHidden, Func<string?, string?, string> resolveDoorSpeciesForTotals, Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow, double MaterialThickness34, string doorEdgebandingSpecies, double height, double leftDepth, double upperDoorGap, double doorLeftReveal, double doorRightReveal, double doorTopReveal, double doorBottomReveal, bool topDeck90, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, double frontWidth, double angle)
    {
        door1 = null;
        door2 = null;
        doorPoints = null;

        if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

            double door1Width = frontWidth - doorLeftReveal - doorRightReveal;
            double doorHeight = height - doorTopReveal - doorBottomReveal;

            if (upperCab.DoorCount == 1)
            {
                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                    var door1Rotated = new Model3DGroup();
                    door1Rotated.Children.Add(door1);
                    ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                    if (!doorsHidden) cabinet.Children.Add(door1Rotated);
                }
            }

            if (upperCab.DoorCount == 2)
            {
                door1Width = (frontWidth / 2) - doorLeftReveal - (upperDoorGap / 2);
                double door2Width = (frontWidth / 2) - doorRightReveal - (upperDoorGap / 2);

                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    addFrontPartRow(upperCab, "Door", doorHeight, door2Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (door1Width,0,0),
                        new (door1Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                    var door1Rotated = new Model3DGroup();
                    door1Rotated.Children.Add(door1);
                    ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                    if (!doorsHidden)
                    {
                        cabinet.Children.Add(door1Rotated); ;
                    }
                    doorPoints =
                    [
                        new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door2, door1Width + doorLeftReveal + upperDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                    var door2Rotated = new Model3DGroup();
                    door2Rotated.Children.Add(door2);
                    ModelTransforms.ApplyTransform(door2Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                    if (!doorsHidden)
                    {
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }
        }
    }

}