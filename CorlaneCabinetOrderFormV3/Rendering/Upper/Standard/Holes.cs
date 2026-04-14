using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void AddHoles(UpperCabinetModel upperCab, double MaterialThickness34, double StretcherWidth, double height, double depth, double backThickness, double holeDiameter, double holeDepth, Model3DGroup leftEnd, Model3DGroup rightEnd)
    {
        // Construction holes (outside face) - along Depth axis, top & bottom edges
        {
            double topConstructionHoleInset = 2;
            double topConstructionHoleCount = Math.Floor(depth - 4) / 10;

            double constructionY = height - (MaterialThickness34 / 2);
            double minX = topConstructionHoleInset;
            double maxX = depth - topConstructionHoleInset;

            if (maxX < minX)
            {
                (minX, maxX) = (maxX, minX);
            }

            double span = Math.Max(0, maxX - minX);

            int segments = Math.Max(1, (int)Math.Ceiling(span / 10.0));
            int holeCount = segments + 1;

            for (int i = 0; i < holeCount; i++)
            {
                double t = holeCount == 1 ? 0 : (double)i / (holeCount - 1);
                double xx = minX + (span * t);

                double topYY = height - (MaterialThickness34 / 2);
                double bottomYY = MaterialThickness34 / 2;

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: xx,
                    centerY: topYY,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: xx,
                    centerY: topYY,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: xx,
                    centerY: bottomYY,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: xx,
                    centerY: bottomYY,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        // Back Construction Holes
        {
            double x = MaterialThickness34 / 2;

            double topY = height - ((StretcherWidth / 2) + MaterialThickness34);
            double bottomY = (StretcherWidth / 2) + MaterialThickness34;

            if (topY < bottomY)
            {
                (topY, bottomY) = (bottomY, topY);
            }

            double spanY = Math.Max(0, topY - bottomY);

            int holeCountY = backThickness == 0.25
                ? 2
                : (Math.Max(1, (int)Math.Ceiling(spanY / 10.0)) + 1);

            for (int i = 0; i < holeCountY; i++)
            {
                double t = holeCountY == 1 ? 0 : (double)i / (holeCountY - 1);
                double y = bottomY + (spanY * t);

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: x,
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: x,
                    centerY: y,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        // Hinge Holes (inside face)
        if (upperCab.DrillHingeHoles)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = depth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = hingeCenterInset;

            if (topCenterY < bottomCenterY)
            {
                (topCenterY, bottomCenterY) = (bottomCenterY, topCenterY);
            }

            double spanYY = Math.Max(0, topCenterY - bottomCenterY);

            int hingeCount = Math.Max(2, (int)Math.Ceiling(spanYY / maxHingeCenterSpacing) + 1);

            for (int h = 0; h < hingeCount; h++)
            {
                double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
                double hingeCenterY = bottomCenterY + (spanYY * t);

                double y1 = hingeCenterY - (hingeBoreSpacing / 2);
                double y2 = hingeCenterY + (hingeBoreSpacing / 2);

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: hingeX,
                    centerY: y1,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: hingeX,
                    centerY: y2,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: hingeX,
                    centerY: y1,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: hingeX,
                    centerY: y2,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        // Shelf Holes
        if (upperCab.DrillShelfHoles)
        {
            double shelfHoleCount = Math.Round((height - 12) / 1.26);

            for (int i = 0; i < shelfHoleCount; i++)
            {
                var shelfHole = CabinetPartFactory.CreateHole(
                    centerX: 1 + backThickness,
                    centerY: 6 + (i * 1.26),
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter);
                leftEnd.Children.Add(shelfHole);
            }

            for (int i = 0; i < shelfHoleCount; i++)
            {
                var shelfHole = CabinetPartFactory.CreateHole(
                    centerX: depth - 1,
                    centerY: 6 + (i * 1.26),
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter);
                leftEnd.Children.Add(shelfHole);
            }

            for (int i = 0; i < shelfHoleCount; i++)
            {
                var shelfHole = CabinetPartFactory.CreateHole(
                    centerX: 1 + backThickness,
                    centerY: 6 + (i * 1.26),
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter);
                rightEnd.Children.Add(shelfHole);
            }

            for (int i = 0; i < shelfHoleCount; i++)
            {
                var shelfHole = CabinetPartFactory.CreateHole(
                    centerX: depth - 1,
                    centerY: 6 + (i * 1.26),
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter);
                rightEnd.Children.Add(shelfHole);
            }
        }
    }
}
