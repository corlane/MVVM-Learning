using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildDeckAndTop(UpperCabinetModel upperCab, double MaterialThickness34, double height, double leftDepth, double rightDepth, double leftBackWidth, double rightBackWidth, bool isPanel, string panelEBEdges, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints, out double frontWidth, out double angle, out Model3DGroup deckRotated, out Model3DGroup topRotated)
    {
        var originalDeck = new List<Point3D>
        {
            new (leftDepth,MaterialThickness34,0),
            new (rightBackWidth - MaterialThickness34, leftBackWidth - rightDepth,0),
            new (rightBackWidth - MaterialThickness34, leftBackWidth - MaterialThickness34 - .25,0),
            new (MaterialThickness34 + .25, leftBackWidth - MaterialThickness34 - .25,0),
            new (MaterialThickness34 + .25, MaterialThickness34,0),
        };

        var p0 = originalDeck[0];
        var p1 = originalDeck[1];

        double vx = p1.X - p0.X;
        double vy = p1.Y - p0.Y;
        frontWidth = Math.Sqrt(vx * vx + vy * vy);
        angle = Math.Atan2(vy, vx);
        double ca = Math.Cos(-angle);
        double sa = Math.Sin(-angle);

        deckPoints = new List<Point3D>(originalDeck.Count);
        foreach (var q in originalDeck)
        {
            double tx = q.X - p0.X;
            double ty = q.Y - p0.Y;
            double rz = q.Z - p0.Z;

            double rx = tx * ca - ty * sa;
            double ry = tx * sa + ty * ca;

            deckPoints.Add(new Point3D(rx, ry, rz));
        }

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45, partKind: CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45, partKind: CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
        ModelTransforms.ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);

        deckRotated = new Model3DGroup();
        topRotated = new Model3DGroup();
        deckRotated.Children.Add(deck);
        topRotated.Children.Add(top);

        ModelTransforms.ApplyTransform(deckRotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
        ModelTransforms.ApplyTransform(topRotated, -MaterialThickness34, height - MaterialThickness34, -leftDepth, 0, 0, 0);
    }
}