using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildBack(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        Func<string?, string> getMatchingEdgebandingSpecies,
        double MaterialThickness34,
        double MaterialThickness14,
        double StretcherWidth,
        double width,
        double height,
        double backThickness,
        double tk_Height,
        double interiorWidth,
        double interiorHeight,
        bool hasLeftEnd,
        bool hasRightEnd)
    {
        Model3DGroup back;
        // Back
        if (baseCab.HasBack)
        {
            if (backThickness == 0.75)
            {
                List<Point3D> backPoints =
                [
                    new (hasLeftEnd ? 0 : -MaterialThickness34, -MaterialThickness34, 0),
                    new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34, -MaterialThickness34, 0),
                    new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34, interiorHeight, 0),
                    new (hasLeftEnd ? 0 : -MaterialThickness34, interiorHeight, 0)
                ];
                if (width <= 47.75 + (2 * MaterialThickness34))
                {
                    back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, isFaceUp: false, CabinetPartKind.BackBase34);
                }
                else
                {
                    back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.BackBase34);
                }
                ModelTransforms.ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34 + tk_Height, 0, 0, 0, 0);
            }
            else
            {
                List<Point3D> backPoints =
                [
                    new (0,0,0),
                    new (width,0,0),
                    new (width,height-tk_Height,0),
                    new (0,height-tk_Height,0)
                ];
                if (width <= 47.75)
                {
                    back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", baseCab, isFaceUp: false, CabinetPartKind.BackBase14);
                }
                else
                {
                    back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.BackBase14);
                }
                ModelTransforms.ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);

            }
        }
        else { back = null; }

        if (backThickness != 0.75)
        {
            List<Point3D> nailerPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            var nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);
        }
        return back;
    }

}
