using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildBacks(BaseCabinetModel baseCab, double MaterialThickness34, double doubleMaterialThickness34, double backLegWidth, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, double tk_Height, double holeDiameter, double holeDepth, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints)
    {
        // Backs

        // Left Back
        if (baseCab.HasTK)
        {
            backPoints =
            [
                new (0,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,-tk_Height,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,-tk_Height,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,height-tk_Height,0),
                new (0,height-tk_Height,0)
            ];
        }
        else
        {
            backPoints =
            [
                new (0,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
                new (0,height,0)
            ];
        }
        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, isFaceUp: true, CabinetPartKind.BackBase34);

        AddShelfHoles(baseCab, MaterialThickness34, doubleMaterialThickness34, height, leftFrontWidth, rightDepth, tk_Height, holeDiameter, holeDepth, leftBack);

        ModelTransforms.ApplyTransform(leftBack, 0, tk_Height, MaterialThickness34, 0, 0, 0);

        // Right Back
        backPoints =
        [
            new (0,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height-tk_Height,0),
            new (0,height-tk_Height,0),
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, isFaceUp: true, CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);
    }

    private static void AddShelfHoles(BaseCabinetModel baseCab, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftFrontWidth, double rightDepth, double tk_Height, double holeDiameter, double holeDepth, Model3DGroup leftBack)
    {
        // Shelf holes (inside face)
        if (baseCab.DrillShelfHoles)
        {
            double shelfHoleCount = Math.Round(((height - 12) / 1.26) - tk_Height);

            double yStart = tk_Height + 6;

            double? maxShelfHoleY = null;

            for (int i = 0; i < shelfHoleCount; i++)
            {
                double y = yStart + (i * 1.26);

                if (maxShelfHoleY is not null && y > maxShelfHoleY.Value)
                {
                    break;
                }

                leftBack.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: leftFrontWidth + rightDepth - doubleMaterialThickness34 - 2, // this is pulled straight out of my ass. I don't know the actual spacing here. Check against ecabs and fix.
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }
    }
}
