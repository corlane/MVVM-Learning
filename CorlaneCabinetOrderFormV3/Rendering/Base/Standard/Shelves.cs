using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildShelves(Model3DGroup cabinet, BaseCabinetModel baseCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, string cabType, string style2, double backThickness, double tk_Height, double interiorWidth, double interiorHeight, double shelfDepth, double opening1Height, bool topDeck90, bool isPanel, string panelEBEdges)
    {
        List<Point3D> shelfPoints =
        [
            new (0,0,0),
            new (interiorWidth-.125,0,0),
            new (interiorWidth-.125,shelfDepth,0),
            new (0,shelfDepth,0)
        ];

        Model3DGroup shelf;
        // Shelves
        if (cabType != style2)
        {
            double shelfSpacing = interiorHeight - opening1Height + MaterialThickness34;
            if (baseCab.DrwCount == 0) { shelfSpacing = interiorHeight; }
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);

            for (int i = 1; i < baseCab.ShelfCount + 1; i++)
            {
                double backThicknessForSpacing = backThickness;
                if (backThickness == 0.25) { backThicknessForSpacing = 0; }
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThicknessForSpacing - shelfDepth, i * shelfSpacing, 270, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }

        return cabinet;
    }
}
