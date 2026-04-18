using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Drills all end-panel holes (construction, back vertical, hinge, shelf, drawer slide)
    /// into the left and right end panels in local coordinates.
    /// Must be called before ApplyTransform on the end panels.
    /// </summary>
    private static void DrillEndPanelHoles(
        Model3DGroup leftEnd,
        Model3DGroup rightEnd,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        double depth = dim.Depth;
        double height = dim.Height;
        double tk_Height = dim.TKHeight;
        double backThickness = dim.BackThickness;
        double shelfDepth = dim.ShelfDepth;
        double opening1Height = dim.Opening1Height;
        double opening2Height = dim.Opening2Height;
        double opening3Height = dim.Opening3Height;
        double opening4Height = dim.Opening4Height;

        string style2 = CabinetStyles.Base.Drawer;

        // Construction holes (outside face) - along Depth axis, top & bottom edges
        {
            double topConstructionHoleInset = 2;

            double minX = topConstructionHoleInset;
            double maxX = depth - topConstructionHoleInset;

            if (maxX < minX)
            {
                (minX, maxX) = (maxX, minX);
            }

            double span = Math.Max(0, maxX - minX);

            int segments = Math.Max(1, (int)Math.Ceiling(span / 10.0));
            int holeCount = segments + 1;

            bool topIsStretcher = string.Equals(baseCab.TopType, CabinetOptions.TopType.Stretcher, StringComparison.OrdinalIgnoreCase);
            bool onlyEndHolesOnTop = topIsStretcher;

            for (int i = 0; i < holeCount; i++)
            {
                double t = holeCount == 1 ? 0 : (double)i / (holeCount - 1);
                double xx = minX + (span * t);

                double topYY = (height - (MaterialThickness34 / 2));
                double bottomYY = tk_Height + (MaterialThickness34 / 2);

                if (!onlyEndHolesOnTop || i == 0 || i == holeCount - 1)
                {
                    leftEnd.Children.Add(CabinetPartFactory.CreateHole(xx, topYY, MaterialThickness34, holeDepth, holeDiameter));
                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, topYY, 0, holeDepth, holeDiameter));
                }

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(xx, bottomYY, MaterialThickness34, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, bottomYY, 0, holeDepth, holeDiameter));
            }
        }

        // Back vertical construction holes (outside face)
        {
            double x = MaterialThickness34 / 2;

            double topY = height - (3 + MaterialThickness34);
            double bottomY = tk_Height + MaterialThickness34;

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

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, MaterialThickness34, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, 0, holeDepth, holeDiameter));
            }
        }

        // Hinge holes (inside face)
        if (baseCab.DrillHingeHoles && baseCab.Style != style2)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = depth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = tk_Height + hingeCenterInset;

            if (baseCab.DrwCount == 1)
            {
                double drawerStretcherBottomY = height - opening1Height - (2 * MaterialThickness34);
                topCenterY = drawerStretcherBottomY - hingeCenterInset;
            }

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

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y1, MaterialThickness34, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(hingeX, y2, MaterialThickness34, holeDepth, holeDiameter));
            }
        }

        // Shelf holes (inside face)
        if (baseCab.DrillShelfHoles && baseCab.Style != style2)
        {
            double shelfHoleCount = Math.Round(((height - 12) / 1.26) - tk_Height);

            double yStart = tk_Height + 6;

            double? maxShelfHoleY = null;
            if (baseCab.DrwCount == 1)
            {
                double drawerStretcherBottomY = height - opening1Height - (2 * MaterialThickness34);
                maxShelfHoleY = drawerStretcherBottomY - 6;
            }

            double frontShelfHoleX = shelfDepth + backThickness - 1;

            for (int i = 0; i < shelfHoleCount; i++)
            {
                double y = yStart + (i * 1.26);

                if (maxShelfHoleY is not null && y > maxShelfHoleY.Value)
                {
                    break;
                }

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: 1 + backThickness,
                    centerY: y,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: frontShelfHoleX,
                    centerY: y,
                    rimZ: 0,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: 1 + backThickness,
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));

                rightEnd.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: frontShelfHoleX,
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        // Drawer slide holes (inside face)
        if (baseCab.DrwCount > 0)
        {
            const double slideXFromFront = 1.456;
            const double slideSpacing = 2.5;
            const double stopFromBack = 3.0;
            const double yFromOpeningBottom = 1.5;

            double xStart = depth - slideXFromFront;
            double xStop = stopFromBack;

            double[] openingHeights = new[] { opening1Height, opening2Height, opening3Height, opening4Height };
            bool[] drillSlidePerOpening = new[]
            {
                baseCab.DrillSlideHolesOpening1,
                baseCab.DrillSlideHolesOpening2,
                baseCab.DrillSlideHolesOpening3,
                baseCab.DrillSlideHolesOpening4
            };

            double openingBottomY = height - MaterialThickness34 - openingHeights[0];

            for (int oi = 0; oi < 4; oi++)
            {
                int openingIndex = oi + 1;
                if (baseCab.DrwCount < openingIndex) break;

                if (drillSlidePerOpening[oi])
                {
                    double y = openingBottomY + yFromOpeningBottom;

                    for (double x = xStart; x >= xStop; x -= slideSpacing)
                    {
                        leftEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, 0, holeDepth, holeDiameter));
                        rightEnd.Children.Add(CabinetPartFactory.CreateHole(x, y, MaterialThickness34, holeDepth, holeDiameter));
                    }
                }

                if (oi + 1 < 4)
                {
                    openingBottomY -= openingHeights[oi + 1] + MaterialThickness34;
                }
            }
        }
    }

}
