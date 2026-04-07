using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;
using static CorlaneCabinetOrderFormV3.Models.CabinetOptions;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildCorner90(
        Model3DGroup cabinet,
        UpperCabinetModel upperCab,
        UpperCabinetDimensions dim,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;

        string doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(upperCab.DoorSpecies);

        double height = dim.Height;
        double leftFrontWidth = dim.LeftFrontWidth;
        double rightFrontWidth = dim.RightFrontWidth;
        double leftDepth = dim.LeftDepth;
        double rightDepth = dim.RightDepth;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double backThickness = MaterialThickness34;

        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";
        int shelfCount = upperCab.ShelfCount;

        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        double insideCornerRadius = 1.0;
        int arcSegments = 8;

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

        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top;
        Model3DGroup shelf;
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;

        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> backPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;

        leftEndPanelPoints =
        [
            new (leftDepth,0,0),
            new (leftDepth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        rightEndPanelPoints =
        [
            new (rightDepth,0,0),
            new (rightDepth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

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
            double x = MaterialThickness34 / 2;
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

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
        ModelTransforms.ApplyTransform(deck, 0, leftDepth, -MaterialThickness34, 90, 0, 0);

        backPoints =
        [
            new (0,0,0),
            new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
            new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
            new (0,height,0)
        ];
        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(leftBack, 0, 0, MaterialThickness34, 0, 0, 0);

        backPoints =
        [
            new (0,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height,0),
            new (0,height,0),
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackUpper34);
        ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, 0, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);

        if (shelfCount > 0)
        {
            double gap = .125;

            var shelfCornerArc = GenerateInsideCornerArc(
                leftFrontWidth - MaterialThickness34 - gap,
                0,
                insideCornerRadius, arcSegments);

            double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
            for (int i = 1; i < shelfCount + 1; i++)
            {
                shelfPoints =
                [
                    new (0,0,0),
                    ..shelfCornerArc,
                    new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                    new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                    new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                    new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
                ];
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing, 90, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }

        double cornerCabDoorOpenSideReveal = 0.875;

        if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

            double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
            double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;
            double doorHeight = height - doorTopReveal - doorBottomReveal;

            if (upperCab.IncDoorsInList)
            {
                addFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                addFrontPartRow(upperCab, "Door", doorHeight, door2Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
            }

            if (upperCab.IncDoors)
            {
                doorPoints =
                [
                    new (0,0,0),
                    new (door1Width,0,0),
                    new (door1Width,doorHeight,0),
                    new (0,doorHeight,0)
                ];
                door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);

                doorPoints =
                [
                    new (0,0,0),
                    new (door2Width,0,0),
                    new (door2Width,doorHeight,0),
                    new (0,doorHeight,0)
                ];
                door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);

                ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                if (!doorsHidden)
                {
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }            }
        }

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);
        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
    }
}