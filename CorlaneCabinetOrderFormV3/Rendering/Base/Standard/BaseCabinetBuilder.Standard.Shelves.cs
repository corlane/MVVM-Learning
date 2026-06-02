using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildShelves(Model3DGroup cabinet, BaseCabinetModel baseCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, string cabType, string style2, double backThickness, double tk_Height, double interiorWidth, double interiorHeight, double shelfDepth, double opening1Height, bool hasLeftEnd, bool hasRightEnd)
    {
        if (!baseCab.HasBack && backThickness == 0.75) { shelfDepth += 0.75; }

        List<Point3D> shelfPoints =
        [
            new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,shelfDepth,0),
            new (hasLeftEnd ? 0 : -MaterialThickness34,shelfDepth,0)
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

                if (!baseCab.HasBack && backThickness == 0.75) { backThicknessForSpacing = 0; }

                if (backThickness == 0.25) { backThicknessForSpacing = 0; }

                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Shelf);

                ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThicknessForSpacing - shelfDepth, i * shelfSpacing, 270, 0, 0);

                cabinet.Children.Add(shelf);
            }
        }

        return cabinet;
    }
}
