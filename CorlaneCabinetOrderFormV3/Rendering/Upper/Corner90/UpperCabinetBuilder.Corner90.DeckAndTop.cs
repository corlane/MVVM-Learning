using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildDeckAndTop(UpperCabinetModel upperCab, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints, List<Point3D> deckCornerArc)
    {
        deckPoints =
        [
            new (0,0,0),
            ..deckCornerArc,
            new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
            new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
            new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
            new (0,-leftDepth + doubleMaterialThickness34,0),
        ];

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, isFaceUp: false, CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
        ModelTransforms.ApplyTransform(deck, 0, leftDepth, -MaterialThickness34, 90, 0, 0);
    }
}