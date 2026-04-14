using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    
    private static void BuildDeckAndTop(UpperCabinetModel upperCab, double MaterialThickness34, double depth, double backThickness, double interiorWidth, double backInsetForDeckAndTop, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup deck, out List<Point3D> deckPoints, out Model3DGroup top, out List<Point3D> topPoints, double height)
    {
        // Deck
        if (backThickness == MaterialThickness34)
        {
            backInsetForDeckAndTop = MaterialThickness34;
        }

        deckPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,depth - backInsetForDeckAndTop,0),
            new (0,depth - backInsetForDeckAndTop,0)
        ];

        topPoints = deckPoints;

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, 0, 270, 0, 0);

        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);
        ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
    }
}