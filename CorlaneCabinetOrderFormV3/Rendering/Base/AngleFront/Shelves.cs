using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildShelves(Model3DGroup cabinet, BaseCabinetModel baseCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double leftDepth, double rightDepth, double leftBackWidth, double rightBackWidth, double tk_Height, double interiorHeight, bool isPanel, string panelEBEdges, int shelfCount, out Model3DGroup? shelf, out List<Point3D>? shelfPoints)
    {
        shelf = null;
        shelfPoints = null;
        // Shelves
        if (shelfCount > 0)
        {
            double gap = .125;

            double shelfSpacing = interiorHeight + MaterialThickness34 + MaterialThickness34;
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);
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
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Shelf, plywoodTextureRotationDegrees: 45);
                ModelTransforms.ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                cabinet.Children.Add(shelf);
            }
        }
    }
}
