using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildDoors(Model3DGroup cabinet, BaseCabinetModel baseCab, bool doorsHidden, Func<string?, string?, string> resolveDoorSpeciesForTotals, Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow, double MaterialThickness34, string doorEdgebandingSpecies, double height, double leftDepth, double tk_Height, double baseDoorGap, double doorLeftReveal, double doorRightReveal, double doorTopReveal, double doorBottomReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, double frontWidth, double angle)
    {
        door1 = null;
        door2 = null;
        doorPoints = null;

        // Doors
        if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);

            double door1Width = frontWidth - doorLeftReveal - doorRightReveal;

            double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

            if (baseCab.DoorCount == 1)
            {
                if (baseCab.IncDoorsInList)
                {
                    addFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                }

                if (baseCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                    new (door1Width,0,0),
                    new (door1Width,doorHeight,0),
                    new (0,doorHeight,0)
                    ];
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                    var door1Rotated = new Model3DGroup();
                    door1Rotated.Children.Add(door1);
                    ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                    if (!doorsHidden) cabinet.Children.Add(door1Rotated);
                }
            }
            if (baseCab.DoorCount == 2)
            {
                door1Width = (frontWidth / 2) - doorLeftReveal - (baseDoorGap / 2);
                double door2Width = (frontWidth / 2) - doorRightReveal - (baseDoorGap / 2);

                if (baseCab.IncDoorsInList)
                {
                    addFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    addFrontPartRow(baseCab, "Door", doorHeight, door2Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                }

                if (baseCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                    new (door1Width,0,0),
                    new (door1Width,doorHeight,0),
                    new (0,doorHeight,0)
                    ];
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                    var door1Rotated = new Model3DGroup();
                    door1Rotated.Children.Add(door1);
                    ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
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
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door2, door1Width + doorLeftReveal + baseDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                    var door2Rotated = new Model3DGroup();
                    door2Rotated.Children.Add(door2);
                    ModelTransforms.ApplyTransform(door2Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                    if (!doorsHidden)
                    {
                        ;
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }
        }
    }

}
