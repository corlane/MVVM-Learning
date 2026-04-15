using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildToekick(Model3DGroup cabinet, BaseCabinetModel baseCab, double MaterialThickness34, double leftFrontWidth, double rightFrontWidth, double leftDepth, double tk_Height, double tk_Depth, out Model3DGroup toekick1, out Model3DGroup toekick2, out List<Point3D> toekickPoints)
    {
        // Initialize out parameters
        toekick1 = null!;
        toekick2 = null!;
        toekickPoints = null!;

        // Toekick
        if (baseCab.HasTK)
        {
            toekickPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth - MaterialThickness34 + tk_Depth,0,0),
                    new (leftFrontWidth - MaterialThickness34 + tk_Depth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                ];
            toekick1 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick1, 0, 0.5, leftDepth - tk_Depth - MaterialThickness34, 0, 0, 0);
            cabinet.Children.Add(toekick1);

            toekickPoints =
                [
                    new (0,0,0),
                    new (rightFrontWidth + tk_Depth,0,0),
                    new (rightFrontWidth + tk_Depth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                ];
            toekick2 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick2, -leftDepth - rightFrontWidth + MaterialThickness34, 0.5, leftFrontWidth + tk_Depth - MaterialThickness34, 0, 90, 0);
            cabinet.Children.Add(toekick2);
        }
    }
}
