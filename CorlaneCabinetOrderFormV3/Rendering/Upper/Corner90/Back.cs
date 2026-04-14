using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildBacks(UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints)
    {
        backPoints =
        [
            new (0,0,0),
            new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
            new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
            new (0,height,0)
        ];
        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(leftBack, 0, 0, MaterialThickness34, 0, 0, 0);

        backPoints =
        [
            new (0,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height,0),
            new (0,height,0),
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, 0, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);
    }
}