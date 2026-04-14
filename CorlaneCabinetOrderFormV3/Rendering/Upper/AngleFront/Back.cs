using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildBacks(UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftBackWidth, double rightBackWidth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints)
    {
        backPoints =
        [
            new (0,0,0),
            new (leftBackWidth - MaterialThickness34 - .25,0,0),
            new (leftBackWidth - MaterialThickness34 - .25,height,0),
            new (0,height,0)
        ];
        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

        backPoints =
        [
            new (0,0,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,0,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
            new (0,height,0)
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);
    }
}