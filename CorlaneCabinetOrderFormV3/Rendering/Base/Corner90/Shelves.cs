using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildShelves(Model3DGroup cabinet, BaseCabinetModel baseCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double doubleMaterialThickness34, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, double tk_Height, bool isPanel, string panelEBEdges, int shelfCount, double interiorHeight, double insideCornerRadius, int arcSegments, out Model3DGroup shelf, out List<Point3D> shelfPoints)
    {
        // Initialize out parameters
        shelf = new Model3DGroup();
        shelfPoints = new List<Point3D>();

        // Shelves
        if (shelfCount > 0)
        {
            double gap = .125;

            var shelfCornerArc = GenerateInsideCornerArc(
                leftFrontWidth - MaterialThickness34 - gap,
                0,
                insideCornerRadius, arcSegments);

            shelfPoints =
            [
                new (0,0,0),
                ..shelfCornerArc,
                new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
            ];

            // Half depth shelves
            if (baseCab.ShelfDepth == CabinetOptions.ShelfDepth.HalfDepth)
            {
                double halfLeftInternal = (leftDepth - doubleMaterialThickness34 - gap) / 2;
                double halfRightInternal = (rightDepth - doubleMaterialThickness34 - gap) / 2;

                var halfShelfCornerArc = GenerateInsideCornerArc(
                    leftFrontWidth - MaterialThickness34 - gap + halfRightInternal,
                    -halfLeftInternal,
                    insideCornerRadius, arcSegments);

                shelfPoints =
                [
                    new (0, -halfLeftInternal, 0),
                    ..halfShelfCornerArc,
                    new (leftFrontWidth - MaterialThickness34 - gap + halfRightInternal, rightFrontWidth - MaterialThickness34 - gap, 0),
                    new (leftFrontWidth - MaterialThickness34 - gap + rightDepth - doubleMaterialThickness34 - gap, rightFrontWidth - MaterialThickness34 - gap, 0),
                    new (leftFrontWidth - MaterialThickness34 - gap + rightDepth - doubleMaterialThickness34 - gap, -leftDepth + doubleMaterialThickness34 + gap, 0),
                    new (0, -leftDepth + doubleMaterialThickness34 + gap, 0),
                ];
            }

            double shelfSpacing = interiorHeight + MaterialThickness34 + MaterialThickness34;
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);
            for (int i = 1; i < shelfCount + 1; i++)
            {
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing, 90, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }
    }
}
