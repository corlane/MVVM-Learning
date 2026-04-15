using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildShelves(Model3DGroup cabinet, UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double backThickness, double interiorWidth, double interiorHeight, double shelfDepth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup? shelf, out List<Point3D>? shelfPoints)
    {
        shelf = null;
        shelfPoints = null;

        // Shelves
        double shelfSpacing = interiorHeight + MaterialThickness34;
        shelfSpacing /= (upperCab.ShelfCount + 1);

        for (int i = 1; i < upperCab.ShelfCount + 1; i++)
        {
            double backThicknessForSpacing = backThickness;
            if (backThickness == 0.25) { backThicknessForSpacing = 0; }

            shelfPoints =
            [
                new (0,0,0),
                new (interiorWidth-.125,0,0),
                new (interiorWidth-.125,shelfDepth,0),
                new (0,shelfDepth,0)
            ];
            shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Shelf);
            ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThicknessForSpacing - shelfDepth, i * shelfSpacing, 270, 0, 0);
            cabinet.Children.Add(shelf);
        }
    }

}
