using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildBack(Model3DGroup cabinet, UpperCabinetModel upperCab, Func<string?, string> getMatchingEdgebandingSpecies, double MaterialThickness34, double MaterialThickness14, double StretcherWidth, double width, double height, double backThickness, double interiorWidth, double interiorHeight, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup back, out Model3DGroup? nailer, out List<Point3D> backPoints, out List<Point3D>? nailerPoints)
    {
        nailer = null;
        nailerPoints = null;

        // Back
        if (backThickness == 0.75)
        {
            backPoints =
            [
                new (0,-MaterialThickness34,0),
                new (interiorWidth,-MaterialThickness34,0),
                new (interiorWidth,interiorHeight + (MaterialThickness34),0),
                new (0,interiorHeight + (MaterialThickness34),0)
            ];
            if (width <= 47.75 + (2 * MaterialThickness34))
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Vertical", upperCab, isFaceUp: false, CabinetPartKind.BackUpper34);
            }
            else
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "PVC Hardrock Maple", "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.BackUpper34);
            }
            ModelTransforms.ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34, 0, 0, 0, 0);
        }
        else
        {
            backPoints =
            [
                new (0,0,0),
                new (width,0,0),
                new (width,height,0),
                new (0,height,0)
            ];
            if (width <= 47.75)
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", upperCab, isFaceUp: false, CabinetPartKind.BackUpper14);
            }
            else
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.BackUpper14);
            }
            ModelTransforms.ApplyTransform(back, -(width / 2), 0, -MaterialThickness14, 0, 0, 0);

            nailerPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);

            nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), 0 + MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);
        }
    }
}