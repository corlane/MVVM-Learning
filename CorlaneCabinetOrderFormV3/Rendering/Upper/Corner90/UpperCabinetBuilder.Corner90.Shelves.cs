using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildShelves(Model3DGroup cabinet, UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, int shelfCount, double gap, out Model3DGroup? shelf, out List<Point3D>? shelfPoints, List<Point3D> shelfCornerArc)
    {
        shelf = null;
        shelfPoints = null;

        if (shelfCount > 0)
        {
            double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
            for (int i = 1; i < shelfCount + 1; i++)
            {
                shelfPoints =
                [
                    new (0,0,0),
                    ..shelfCornerArc,
                    new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                    new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                    new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                    new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
                ];
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing, 90, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }
    }
}