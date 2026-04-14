using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void AddHoles(UpperCabinetModel upperCab, double MaterialThickness34, double height, double leftDepth, double rightDepth, double leftBackWidth, double rightBackWidth, double backThickness, Model3DGroup leftEnd, Model3DGroup rightEnd, double holeDiameter, double holeDepth)
    {
        // Construction holes (outside face) - along Depth axis, top & bottom edges
        {
            double topConstructionHoleInset = 2;

            // Left end (uses leftDepth)
            {
                double minX = topConstructionHoleInset;
                double maxX = leftDepth - topConstructionHoleInset;
                if (maxX < minX) (minX, maxX) = (maxX, minX);
                double span = Math.Max(0, maxX - minX);
                int segments = Math.Max(1, (int)Math.Ceiling(span / 10.0));
                int holeCount = segments + 1;

                for (int i = 0; i < holeCount; i++)
                {
                    double t = holeCount == 1 ? 0 : (double)i / (holeCount - 1);
                    double xx = minX + (span * t);
                    double topYY = height - (MaterialThickness34 / 2);
                    double bottomYY = MaterialThickness34 / 2;

                    leftEnd.Children.Add(CabinetPartFactory.CreateHole(xx, topYY, MaterialThickness34, holeDepth, holeDiameter));
                    leftEnd.Children.Add(CabinetPartFactory.CreateHole(xx, bottomYY, MaterialThickness34, holeDepth, holeDiameter));
                }
            }

            // Right end (uses rightDepth)
            {
                double minX = topConstructionHoleInset;
                double maxX = rightDepth - topConstructionHoleInset;
                if (maxX < minX) (minX, maxX) = (maxX, minX);
                double span = Math.Max(0, maxX - minX);
                int segments = Math.Max(1, (int)Math.Ceiling(span / 10.0));
                int holeCount = segments + 1;

                for (int i = 0; i < holeCount; i++)
                {
                    double t = holeCount == 1 ? 0 : (double)i / (holeCount - 1);
                    double xx = minX + (span * t);
                    double topYY = height - (MaterialThickness34 / 2);
                    double bottomYY = MaterialThickness34 / 2;

                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, topYY, 0, holeDepth, holeDiameter));
                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, bottomYY, 0, holeDepth, holeDiameter));
                }
            }
        }

        // Back vertical construction holes (outside face)
        {
            double x = MaterialThickness34;
            double topY = height - (2 + MaterialThickness34);
            double bottomY = 2 + MaterialThickness34;
            if (topY < bottomY) (topY, bottomY) = (bottomY, topY);
            double spanY = Math.Max(0, topY - bottomY);
            int holeCountY = Math.Max(1, (int)Math.Ceiling(spanY / 10.0)) + 1;

            for (int i = 0; i < holeCountY; i++)
            {
                double t = holeCountY == 1 ? 0 : (double)i / (holeCountY - 1);
                double y = bottomY + (spanY * t);

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, MaterialThickness34, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, 0, holeDepth, holeDiameter));
            }
        }

        // Shelf holes (inside face)
        if (upperCab.DrillShelfHoles)
        {
            double shelfHoleCount = Math.Round(((height - 12) / 1.26));

            double yStart = 6;

            double? maxShelfHoleY = null;

            double frontShelfHoleLeftX = leftDepth + backThickness - 2;
            double frontShelfHoleRightX = rightDepth + backThickness - 2;

            for (int i = 0; i < shelfHoleCount; i++)
            {
                double y = yStart + (i * 1.26);

                if (maxShelfHoleY is not null && y > maxShelfHoleY.Value)
                {
                    break;
                }

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: 2 + backThickness,
                    centerY: y,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: frontShelfHoleLeftX,
                    centerY: y,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: 2 + backThickness,
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: frontShelfHoleRightX,
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        // Hinge holes (inside face)
        // Left end hinge holes:
        if (upperCab.DrillHingeHoles)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = leftDepth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = hingeCenterInset;

            if (topCenterY < bottomCenterY)
            {
                (topCenterY, bottomCenterY) = (bottomCenterY, topCenterY);
            }

            double spanY = Math.Max(0, topCenterY - bottomCenterY);

            int hingeCount = Math.Max(2, (int)Math.Ceiling(spanY / maxHingeCenterSpacing) + 1);

            for (int h = 0; h < hingeCount; h++)
            {
                double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
                double hingeCenterY = bottomCenterY + (spanY * t);

                double y1 = hingeCenterY - (hingeBoreSpacing / 2);
                double y2 = hingeCenterY + (hingeBoreSpacing / 2);

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y1, 0, holeDepth, holeDiameter));
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y2, 0, holeDepth, holeDiameter));
            }
        }

        // Right end hinge holes:
        if (upperCab.DrillHingeHoles)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = rightDepth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = hingeCenterInset;

            if (topCenterY < bottomCenterY)
            {
                (topCenterY, bottomCenterY) = (bottomCenterY, topCenterY);
            }

            double spanY = Math.Max(0, topCenterY - bottomCenterY);

            int hingeCount = Math.Max(2, (int)Math.Ceiling(spanY / maxHingeCenterSpacing) + 1);

            for (int h = 0; h < hingeCount; h++)
            {
                double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
                double hingeCenterY = bottomCenterY + (spanY * t);

                double y1 = hingeCenterY - (hingeBoreSpacing / 2);
                double y2 = hingeCenterY + (hingeBoreSpacing / 2);

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y1, MaterialThickness34, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y2, MaterialThickness34, holeDepth, holeDiameter));
            }
        }

        ModelTransforms.ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 90, 0);
        ModelTransforms.ApplyTransform(rightEnd, -leftBackWidth, 0, -rightBackWidth, 0, 0, 0);
    }
}