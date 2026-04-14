using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    public static Model3DGroup BuildTop(
        BaseCabinetModel baseCab, 
        double MaterialThickness34, 
        double StretcherWidth, 
        double topStretcherBackWidth, 
        double width, 
        double height, 
        double depth, 
        double interiorWidth, 
        bool topDeck90, 
        bool isPanel, 
        string panelEBEdges, 
        Model3DGroup top, 
        out Model3DGroup? topStretcherFront, 
        out Model3DGroup? topStretcherBack)

    {
        // Full Top
        if (string.Equals(baseCab.TopType, CabinetOptions.TopType.Full, StringComparison.OrdinalIgnoreCase))
        {
            List<Point3D> topPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);
            ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
        }

        else
        {
            List<Point3D> topStretcherFrontPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            List<Point3D> topStretcherBackPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,topStretcherBackWidth,0),
                new (0,topStretcherBackWidth,0)
            ];

            topStretcherFront = CabinetPartFactory.CreatePanel(topStretcherFrontPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.TopStretcherFront);
            topStretcherBack = CabinetPartFactory.CreatePanel(topStretcherBackPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.TopStretcherBack);

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
