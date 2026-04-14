using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildShelves(Model3DGroup cabinet, UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftDepth, double rightDepth, double leftBackWidth, double rightBackWidth, bool isPanel, string panelEBEdges, int shelfCount, out Model3DGroup? shelf, out List<Point3D>? shelfPoints)
    {
        shelf = null;
        shelfPoints = null;

        if (shelfCount > 0)
        {
            double gap = .125;

            double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
            for (int i = 1; i < shelfCount + 1; i++)
            {
                shelfPoints =
                [
                    new (leftDepth,MaterialThickness34 + gap,0),
                    new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - rightDepth,0),
                    new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                    new (MaterialThickness34 + .25 + gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                    new (MaterialThickness34 + .25 + gap, MaterialThickness34 + gap,0),
                ];
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, 45, partKind: CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                cabinet.Children.Add(shelf);
            }
        }
    }
}
