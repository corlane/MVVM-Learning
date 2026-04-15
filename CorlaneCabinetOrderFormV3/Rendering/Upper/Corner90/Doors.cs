using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildDoors(Model3DGroup cabinet, UpperCabinetModel upperCab, bool doorsHidden, Func<string?, string?, string> resolveDoorSpeciesForTotals, Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow, double MaterialThickness34, double doubleMaterialThickness34, string doorEdgebandingSpecies, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double doorLeftReveal, double doorRightReveal, double doorTopReveal, double doorBottomReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, double cornerCabDoorOpenSideReveal)
    {
        door1 = null;
        door2 = null;
        doorPoints = null;

        if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

            double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
            double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;
            double doorHeight = height - doorTopReveal - doorBottomReveal;

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

                doorPoints =
                [
                    new (0,0,0),
                    new (door2Width,0,0),
                    new (door2Width,doorHeight,0),
                    new (0,doorHeight,0)
                ];
                door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, isFaceUp: false, CabinetPartKind.Door);

                ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                if (!doorsHidden)
                {
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }
        }
    }
}