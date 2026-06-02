using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildTop(
        BaseCabinetModel baseCab, 
        double MaterialThickness34, 
        double StretcherWidth, 
        double topStretcherBackWidth, 
        double width, 
        double height, 
        double depth, 
        double interiorWidth,
        Model3DGroup top, 
        out Model3DGroup? topStretcherFront, 
        out Model3DGroup? topStretcherBack,
        bool hasLeftEnd,
        bool hasRightEnd
        )

    {
        // Full Top
        if (string.Equals(baseCab.TopType, CabinetOptions.TopType.Full, StringComparison.OrdinalIgnoreCase))
        {
            List<Point3D> topPoints =
            [
                new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,depth,0),
                new (hasLeftEnd ? 0 : -MaterialThickness34,depth,0)
            ];
            top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Top);
            ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
        }

        else
        {
            List<Point3D> topStretcherFrontPoints =
            [
                new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,StretcherWidth,0),
                new (hasLeftEnd ? 0 : -MaterialThickness34,StretcherWidth,0)
            ];

            List<Point3D> topStretcherBackPoints =
            [
                new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,topStretcherBackWidth,0),
                new (hasLeftEnd ? 0 : -MaterialThickness34,topStretcherBackWidth,0)
            ];

            topStretcherFront = CabinetPartFactory.CreatePanel(topStretcherFrontPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.TopStretcherFront);
            topStretcherBack = CabinetPartFactory.CreatePanel(topStretcherBackPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.TopStretcherBack);

            // Sink cuts on top stretcher front (local coords: X 0→interiorWidth, Y 0→StretcherWidth)
            if (baseCab.SinkCabinet)
            {
               AddSinkCuts(topStretcherFront, interiorWidth, width, StretcherWidth, MaterialThickness34);
            }

            ModelTransforms.ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
            ModelTransforms.ApplyTransform(topStretcherBack, -(interiorWidth / 2), -topStretcherBackWidth, height - MaterialThickness34, 270, 0, 0);
            top.Children.Add(topStretcherFront);
            top.Children.Add(topStretcherBack);
        }

        topStretcherFront = null;
        topStretcherBack = null;
        return top;
    }

}
