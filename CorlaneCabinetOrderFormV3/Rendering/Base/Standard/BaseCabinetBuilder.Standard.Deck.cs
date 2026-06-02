using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildDeck(
        BaseCabinetModel baseCab, 
        double MaterialThickness34, 
        double depth, 
        double backThickness, 
        double tk_Height, 
        double interiorWidth, 
        double deckBackInset,
        bool hasLeftEnd,
        bool hasRightEnd
        )

    {
        Model3DGroup deck;
        // Deck
        double mt34 = MaterialThickness34;
        if (backThickness == MaterialThickness34) { deckBackInset = MaterialThickness34; }

        if (!baseCab.HasBack && backThickness == 0.75) { deckBackInset = 0; }

        List<Point3D> deckPoints =
        [
            new (hasLeftEnd ? 0 : -mt34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + mt34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + mt34,depth - deckBackInset,0),
            new (hasLeftEnd ? 0 : -mt34,depth - deckBackInset,0)
        ];

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Deck);
        ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);
        return deck;
    }
}
