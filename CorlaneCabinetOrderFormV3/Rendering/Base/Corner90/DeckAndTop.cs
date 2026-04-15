using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    static void BuildDeckAndTop(BaseCabinetModel baseCab, double MaterialThickness34, double doubleMaterialThickness34, double height, double leftFrontWidth, double rightFrontWidth, double leftDepth, double rightDepth, double tk_Height, double insideCornerRadius, int arcSegments, Model3DGroup leftEnd, Model3DGroup rightEnd, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints)
    {
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

        var deckCornerArc = GenerateInsideCornerArc(
            leftFrontWidth - MaterialThickness34,
            0,
            insideCornerRadius, arcSegments);

        deckPoints =
            [
                new (0,0,0),
                ..deckCornerArc,
                new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
                new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
                new (0,-leftDepth + doubleMaterialThickness34,0),
            ];

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
        ModelTransforms.ApplyTransform(deck, 0, leftDepth, -tk_Height - MaterialThickness34, 90, 0, 0);
    }


    static List<Point3D> GenerateInsideCornerArc(
    double cornerX, double cornerY, double radius, int segments)
    {
        double cx = cornerX - radius;
        double cy = cornerY + radius;
        var pts = new List<Point3D>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            double t = (double)i / segments;
            double angle = -(Math.PI / 2.0) + (t * Math.PI / 2.0); // -90° → 0°
            pts.Add(new Point3D(
                cx + radius * Math.Cos(angle),
                cy + radius * Math.Sin(angle),
                0));
        }
        return pts;
    }
}
