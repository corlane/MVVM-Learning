using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildToekick(BaseCabinetModel baseCab, double MaterialThickness34, double depth, double tk_Height, double tk_Depth, double interiorWidth, Model3DGroup toekick, bool hasLeftEnd, bool hasRightEnd)
    {
        // Toekick
        if (baseCab.HasTK)
        {
            List<Point3D> toekickPoints =
            [
                new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,tk_Height-.5,0),
                new (hasLeftEnd ? 0 : -MaterialThickness34,tk_Height-.5,0)
            ];
            toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick, -(interiorWidth / 2), 0.5, depth - tk_Depth - MaterialThickness34, 0, 0, 0);
        }

        return toekick;
    }
}
