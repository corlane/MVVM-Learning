using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildBacks(BaseCabinetModel baseCab, double MaterialThickness34, double doubleMaterialThickness34, double backLegWidth, double height, double leftBackWidth, double rightBackWidth, double tk_Height, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints)
    {
        // Backs

        // Left Back
        if (baseCab.HasTK)
        {
            backPoints =
            [
                new (0,0,0),
                new (backLegWidth,0,0),
                new (backLegWidth,tk_Height,0),
                new (leftBackWidth - MaterialThickness34 - .25,tk_Height,0),
                new (leftBackWidth - MaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];

        }

        else
        {
            backPoints =
            [
                new (0,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,height - tk_Height,0),
                new (0,height - tk_Height,0)
            ];
        }

        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, isFaceUp: true, CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

        // Right Back
        backPoints =
        [
            new (0,tk_Height,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,tk_Height,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
            new (0,height,0)
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, isFaceUp: true, CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);
    }
}
