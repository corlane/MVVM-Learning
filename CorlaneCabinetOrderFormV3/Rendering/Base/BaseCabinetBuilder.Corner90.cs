using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;
using static CorlaneCabinetOrderFormV3.Models.CabinetOptions;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildCorner90(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;
        double backLegWidth = 3;

        string doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(baseCab.DoorSpecies);

        double height = dim.Height;
        double leftFrontWidth = dim.LeftFrontWidth;
        double rightFrontWidth = dim.RightFrontWidth;
        double leftDepth = dim.LeftDepth;
        double rightDepth = dim.RightDepth;
        double tk_Height = dim.TKHeight;
        double tk_Depth = dim.TKDepth;
        double baseDoorGap = dim.BaseDoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double shelfDepth = dim.ShelfDepth;
        double backThickness = MaterialThickness34; // corner cabinets always have 3/4" backs
        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";
        int shelfCount = baseCab.ShelfCount;
        double interiorHeight = dim.InteriorHeight;

        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        double insideCornerRadius = 1.0; // adjust to taste
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
        Model3DGroup toekick1;
        Model3DGroup toekick2;
        Model3DGroup door1;
        Model3DGroup door2;

        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> backPoints;
        List<Point3D> toekickPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;

        // End Panels
        if (baseCab.HasTK)
        {
            leftEndPanelPoints =
                [
                    new (leftDepth,tk_Height,0),
                    new (leftDepth,height,0),
                    new (0,height,0),
                    new (0,0,0),
                    new (3,0,0),
                    new (3,.5,0),
                    new (leftDepth-tk_Depth-3,.5,0),
                    new (leftDepth-tk_Depth-3,0,0),
                    new (leftDepth-tk_Depth,0,0),
                    new (leftDepth-tk_Depth,tk_Height,0)
                ];

            rightEndPanelPoints =
                [
                    new (rightDepth,tk_Height,0),
                    new (rightDepth,height,0),
                    new (0,height,0),
                    new (0,0,0),
                    new (3,0,0),
                    new (3,.5,0),
                    new (rightDepth-tk_Depth-3,.5,0),
                    new (rightDepth-tk_Depth-3,0,0),
                    new (rightDepth-tk_Depth,0,0),
                    new (rightDepth-tk_Depth,tk_Height,0)
                ];
        }
        else
        {
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
        }

        //leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        //rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

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
                    double bottomYY = tk_Height + (MaterialThickness34 / 2);

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
                    double bottomYY = tk_Height + (MaterialThickness34 / 2);

                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, topYY, 0, holeDepth, holeDiameter));
                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(xx, bottomYY, 0, holeDepth, holeDiameter));
                }
            }
        }

        // Back vertical construction holes (outside face)
        {
            double x = MaterialThickness34 * 1.5;
            double topY = height - (3 + MaterialThickness34);
            double bottomY = tk_Height + MaterialThickness34;
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
        if (baseCab.DrillShelfHoles)
        {
            double shelfHoleCount = Math.Round(((height - 12) / 1.26) - tk_Height);

            double yStart = tk_Height + 6;

            double? maxShelfHoleY = null;

            double frontShelfHoleLeftX = leftDepth + backThickness - 2;
            double frontShelfHoleRightX = rightDepth + backThickness - 2;

            if (baseCab.ShelfDepth == CabinetOptions.ShelfDepth.HalfDepth)
            {
                frontShelfHoleLeftX = leftDepth / 2;
                frontShelfHoleRightX = rightDepth / 2;
            }

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
        if (baseCab.DrillHingeHoles)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = leftDepth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = tk_Height + hingeCenterInset;

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
        if (baseCab.DrillHingeHoles)
        {
            const double hingeBoreSpacing = 1.26;
            const double hingeXFromFront = 1.456;
            const double hingeCenterInset = 2.5197;
            const double maxHingeCenterSpacing = 40.0;

            double hingeX = rightDepth - hingeXFromFront;

            double topCenterY = height - hingeCenterInset;
            double bottomCenterY = tk_Height + hingeCenterInset;

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

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
        ModelTransforms.ApplyTransform(deck, 0, leftDepth, -tk_Height - MaterialThickness34, 90, 0, 0);

        // Backs

        // Left Back
        if (baseCab.HasTK)
        {
            backPoints =
            [
                new (0,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - backLegWidth - MaterialThickness34,-tk_Height,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,-tk_Height,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34 - MaterialThickness34,height-tk_Height,0),
                new (0,height-tk_Height,0)
            ];
        }
        else
        {
            backPoints =
            [
                new (0,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
                new (0,height,0)
            ];
        }
        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackBase34);

        // Shelf holes (inside face)
        if (baseCab.DrillShelfHoles)
        {
            double shelfHoleCount = Math.Round(((height - 12) / 1.26) - tk_Height);

            double yStart = tk_Height + 6;

            double? maxShelfHoleY = null;

            for (int i = 0; i < shelfHoleCount; i++)
            {
                double y = yStart + (i * 1.26);

                if (maxShelfHoleY is not null && y > maxShelfHoleY.Value)
                {
                    break;
                }

                leftBack.Children.Add(CabinetPartFactory.CreateHole(
                    centerX: leftFrontWidth + rightDepth - doubleMaterialThickness34 - 2, // this is pulled straight out of my ass. I don't know the actual spacing here. Check against ecabs and fix.
                    centerY: y,
                    rimZ: MaterialThickness34,
                    bottomZ: holeDepth,
                    diameter: holeDiameter));
            }
        }

        ModelTransforms.ApplyTransform(leftBack, 0, tk_Height, MaterialThickness34, 0, 0, 0);

        // Right Back
        backPoints =
        [
            new (0,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
            new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height-tk_Height,0),
            new (0,height-tk_Height,0),
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);


        // Toekick
        if (baseCab.HasTK)
        {
            toekickPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth - MaterialThickness34 + tk_Depth,0,0),
                    new (leftFrontWidth - MaterialThickness34 + tk_Depth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                ];
            toekick1 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick1, 0, 0.5, leftDepth - tk_Depth - MaterialThickness34, 0, 0, 0);
            cabinet.Children.Add(toekick1);

            toekickPoints =
                [
                    new (0,0,0),
                    new (rightFrontWidth + tk_Depth,0,0),
                    new (rightFrontWidth + tk_Depth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                ];
            toekick2 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick2, -leftDepth - rightFrontWidth + MaterialThickness34, 0.5, leftFrontWidth + tk_Depth - MaterialThickness34, 0, 90, 0);
            cabinet.Children.Add(toekick2);
        }


        // Shelves
        if (shelfCount > 0)
        {
            double gap = .125;

            var shelfCornerArc = GenerateInsideCornerArc(
                leftFrontWidth - MaterialThickness34 - gap,
                0,
                insideCornerRadius, arcSegments);

            shelfPoints =
            [
                new (0,0,0),
                ..shelfCornerArc,
                new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
            ];

            // Half depth shelves
            if (baseCab.ShelfDepth == CabinetOptions.ShelfDepth.HalfDepth)
            {
                double halfLeftInternal = (leftDepth - doubleMaterialThickness34 - gap) / 2;
                double halfRightInternal = (rightDepth - doubleMaterialThickness34 - gap) / 2;

                var halfShelfCornerArc = GenerateInsideCornerArc(
                    leftFrontWidth - MaterialThickness34 - gap + halfRightInternal,
                    -halfLeftInternal,
                    insideCornerRadius, arcSegments);

                shelfPoints =
                [
                    new (0, -halfLeftInternal, 0),
                    ..halfShelfCornerArc,
                    new (leftFrontWidth - MaterialThickness34 - gap + halfRightInternal, rightFrontWidth - MaterialThickness34 - gap, 0),
                    new (leftFrontWidth - MaterialThickness34 - gap + rightDepth - doubleMaterialThickness34 - gap, rightFrontWidth - MaterialThickness34 - gap, 0),
                    new (leftFrontWidth - MaterialThickness34 - gap + rightDepth - doubleMaterialThickness34 - gap, -leftDepth + doubleMaterialThickness34 + gap, 0),
                    new (0, -leftDepth + doubleMaterialThickness34 + gap, 0),
                ];
            }

            double shelfSpacing = interiorHeight + MaterialThickness34 + MaterialThickness34;
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);
            for (int i = 1; i < shelfCount + 1; i++)
            {
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, 0 + .0625, leftDepth, -i * shelfSpacing, 90, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }

        // Doors
        double cornerCabDoorOpenSideReveal = 0.875;

        if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);
            double door1Width = leftFrontWidth - doorLeftReveal - cornerCabDoorOpenSideReveal;
            double door2Width = rightFrontWidth - doorRightReveal - cornerCabDoorOpenSideReveal;

            double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

            if (baseCab.IncDoorsInList)
            {
                addFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                addFrontPartRow(baseCab, "Door", doorHeight, door2Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
            }

            if (baseCab.IncDoors)
            {
                doorPoints =
                [
                    new (0,0,0),
                    new (door1Width,0,0),
                    new (door1Width,doorHeight,0),
                    new (0,doorHeight,0)
                ];

                door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);

                doorPoints =
                    [
                        new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);


                if (!baseCab.HasTK)
                {
                    ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                }
                else
                {
                    ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal + tk_Height, leftDepth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal + tk_Height, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                }
                if (!doorsHidden)
                {
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }
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