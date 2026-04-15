using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{  
    private static void BuildToekick(Model3DGroup cabinet, BaseCabinetModel baseCab, double MaterialThickness34, double leftDepth, double tk_Height, double tk_Depth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup? toekick, out List<Point3D>? toekickPoints, double frontWidth, double angle)
    {
        toekickPoints = null;
        toekick = null;

        // Toekick
        if (baseCab.HasTK)
        {
            toekickPoints =
            [
                new (-tk_Depth,0,0),
                new (frontWidth + tk_Depth,0,0),
                new (frontWidth + tk_Depth,tk_Height-.5,0),
                new (-tk_Depth,tk_Height-.5,0)
            ];
            toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick, 0, 0, -tk_Depth, 0, ((angle * 180) / Math.PI) + 90, 0);
            var toekickRotated = new Model3DGroup();
            toekickRotated.Children.Add(toekick);
            ModelTransforms.ApplyTransform(toekickRotated, -MaterialThickness34, .5, -leftDepth, 0, 0, 0);
            cabinet.Children.Add(toekickRotated);
        }
    }
}
