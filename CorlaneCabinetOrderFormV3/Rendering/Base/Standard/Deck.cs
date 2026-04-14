using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    public static Model3DGroup BuildDeck(
        BaseCabinetModel baseCab, 
        double MaterialThickness34, 
        double depth, 
        double backThickness, 
        double tk_Height, 
        double interiorWidth, 
        double deckBackInset,
        bool topDeck90, 
        bool isPanel, 
        string panelEBEdges)

    {
        Model3DGroup deck;
        // Deck
        if (backThickness == MaterialThickness34) { deckBackInset = MaterialThickness34; }
        List<Point3D> deckPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,depth - deckBackInset,0),
            new (0,depth - deckBackInset,0)
        ];
        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);
        return deck;
    }
}
