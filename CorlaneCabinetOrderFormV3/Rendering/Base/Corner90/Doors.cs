using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildDoors(Model3DGroup cabinet, BaseCabinetModel baseCab, bool doorsHidden, Func<string?, string?, string> resolveDoorSpeciesForTotals, Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow, double MaterialThickness34, double doubleMaterialThickness34, string doorEdgebandingSpecies, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double tk_Height, double doorLeftReveal, double doorRightReveal, double doorTopReveal, double doorBottomReveal, bool topDeck90, out Model3DGroup door1, out Model3DGroup door2, out List<Point3D> doorPoints)
    {
        // Initialize out parameters
        door1 = null!;
        door2 = null!;
        doorPoints = null!;

        // Doors
        double cornerCabDoorOpenSideReveal = 0.875;

        if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);
            double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
            double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;

            double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

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

                doorPoints =
                    [
                        new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);


                if (!baseCab.HasTK)
                {
                    ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                }
                else
                {
                    ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal + tk_Height, leftDepth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal + tk_Height, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                }
                if (!doorsHidden)
                {
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }
        }
    }
}
