using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildStandardOrDrawer(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow,
        CabinetBuildResult? result = null)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double MaterialThickness14 = MaterialDefaults.Thickness14;
        double halfMaterialThickness34 = MaterialThickness34 / 2;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;

        string? cabType = baseCab.Style;
        string style1 = CabinetStyles.Base.Standard;
        string style2 = CabinetStyles.Base.Drawer;

        string doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(baseCab.DoorSpecies);

        double StretcherWidth = 6;
        double topStretcherBackWidth = 3;

        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double backThickness = dim.BackThickness;
        double tk_Height = dim.TKHeight;
        double tk_Depth = dim.TKDepth;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double interiorHeight = dim.InteriorHeight;
        double shelfDepth = dim.ShelfDepth;
        double opening1Height = dim.Opening1Height;
        double opening2Height = dim.Opening2Height;
        double opening3Height = dim.Opening3Height;
        double opening4Height = dim.Opening4Height;
        double drwFront1Height = dim.DrwFront1Height;
        double drwFront2Height = dim.DrwFront2Height;
        double drwFront3Height = dim.DrwFront3Height;
        double drwFront4Height = dim.DrwFront4Height;
        double baseDoorGap = dim.BaseDoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double doorSideReveal = dim.DoorSideReveal;
        double deckBackInset = 0;

        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";

        int shelfCount = baseCab.ShelfCount;

        double dbxWidth = interiorWidth;
        double dbxHeight;
        double dbxDepth = dim.DrawerBoxDepth;
        double tandemSideSpacing = .4;
        double tandemTopSpacing = .375;
        double tandemBottomSpacing = .5906;
        double tandemMidDrwBottomSpacingAdjustment = 0;
        double accurideSideSpacing = 1;
        double accurideTopSpacing = .5;
        double accurideBottomSpacing = .5;
        double rolloutHeight = 4;

        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        // ── Capture core dimensions ──
        if (result is not null)
        {
            result.InteriorWidth = interiorWidth;
            result.InteriorDepth = interiorDepth;
            result.InteriorHeight = interiorHeight;
            result.ShelfDepth = shelfDepth;
            result.DrawerBoxDepth = dbxDepth;
        }

        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup topStretcherFront;
        Model3DGroup topStretcherBack;
        Model3DGroup stretcher;
        Model3DGroup shelf;
        Model3DGroup toekick = new();
        Model3DGroup back;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup nailer;

        List<Point3D> endPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> topStretcherFrontPoints;
        List<Point3D> topStretcherBackPoints;
        List<Point3D> toekickPoints;
        List<Point3D> backPoints;
        List<Point3D> stretcherPoints;
        List<Point3D> trashRolloutStretcherPoints;
        List<Point3D> sinkStretcherPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> drwFrontPoints;
        List<Point3D> nailerPoints;

        stretcherPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,StretcherWidth,0),
            new (0,StretcherWidth,0)
        ];

        trashRolloutStretcherPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,interiorDepth,0),
            new (0,interiorDepth,0)
        ];

        shelfPoints =
        [
            new (0,0,0),
            new (interiorWidth-.125,0,0),
            new (interiorWidth-.125,shelfDepth,0),
            new (0,shelfDepth,0)
        ];

        if (baseCab.HasTK)
        {
            endPanelPoints =
            [
                new (depth,tk_Height,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0),
                new (3,0,0),
                new (3,.5,0),
                new (depth-tk_Depth-3,.5,0),
                new (depth-tk_Depth-3,0,0),
                new (depth-tk_Depth,0,0),
                new (depth-tk_Depth,tk_Height,0)
            ];
        }
        else
        {
            endPanelPoints =
            [
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];
        }

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);


        // ----------------------------
        // HOLES (base cabinets)
        // IMPORTANT: add holes before ApplyTransform(leftEnd/rightEnd, ...)
        // ----------------------------

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


        // End panel transforms
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);


        // Deck
        if (backThickness == MaterialThickness34) { deckBackInset = MaterialThickness34; }
        deckPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,depth - deckBackInset,0),
            new (0,depth - deckBackInset,0)
        ];
        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, tk_Height, 270, 0, 0);

        // Full Top
        if (string.Equals(baseCab.TopType, CabinetOptions.TopType.Full, StringComparison.OrdinalIgnoreCase))
        {
            topPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);
            ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
        }

        else
        {
            topStretcherFrontPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            topStretcherBackPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,topStretcherBackWidth,0),
                new (0,topStretcherBackWidth,0)
            ];


            topStretcherFront = CabinetPartFactory.CreatePanel(topStretcherFrontPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.TopStretcherFront);
            topStretcherBack = CabinetPartFactory.CreatePanel(topStretcherBackPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.TopStretcherBack);

            // Sink cuts on top stretcher front (local coords: X 0→interiorWidth, Y 0→StretcherWidth)
            if (baseCab.SinkCabinet)
            {
                double cutWidth = 0.5;
                double cutLength = 4.5; // TODO: fill in cut length
                double cutRimZ = MaterialThickness34;
                double cutBottomZ = 0;

                // TODO: fill in centerX / centerY for each cut
                double cut1CenterX = 2;
                double cut1CenterY = StretcherWidth - (cutLength / 2);

                topStretcherFront.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));


                cut1CenterX = width - MaterialThickness34 - 2.75;

                topStretcherFront.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));

                cut1CenterX = interiorWidth / 2;
                cut1CenterY = 1.75;

                cutLength = .5;
                cutWidth = width - (2.75 * 2);

                topStretcherFront.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));
            }

            ModelTransforms.ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
            ModelTransforms.ApplyTransform(topStretcherBack, -(interiorWidth / 2), -topStretcherBackWidth, height - MaterialThickness34, 270, 0, 0);
            top.Children.Add(topStretcherFront);
            top.Children.Add(topStretcherBack);
        }

        // Toekick
        if (baseCab.HasTK)
        {
            toekickPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,tk_Height-.5,0),
                    new (0,tk_Height-.5,0)
                ];
            toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Toekick);
            ModelTransforms.ApplyTransform(toekick, -(interiorWidth / 2), 0.5, depth - tk_Depth - MaterialThickness34, 0, 0, 0);
        }

        // Back
        if (backThickness == 0.75)
        {
            backPoints =
            [
                new (0,-MaterialThickness34,0),
                new (interiorWidth,-MaterialThickness34,0),
                new (interiorWidth,interiorHeight,0),
                new (0,interiorHeight,0)
            ];
            back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackBase34);
            ModelTransforms.ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34 + tk_Height, 0, 0, 0, 0);
        }
        else
        {
            backPoints =
            [
                new (0,0,0),
                new (width,0,0),
                new (width,height-tk_Height,0),
                new (0,height-tk_Height,0)
            ];
            back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackBase14);
            ModelTransforms.ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);

            nailerPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);
        }

        // Drawer Stretchers
        if (cabType == style1 && baseCab.DrwCount == 1)
        {
            double topDeckAndStretcherThickness = (baseCab.DrwCount + 1) * MaterialThickness34;

            if (baseCab.TrashDrawer)
            {
                stretcherPoints = trashRolloutStretcherPoints;
            }

            stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);

            if (baseCab.SinkCabinet)
            {
                double cutWidth = 0.5;
                double cutLength = 4.5; // TODO: fill in cut length
                double cutRimZ = MaterialThickness34;
                double cutBottomZ = 0;

                // TODO: fill in centerX / centerY for each cut
                double cut1CenterX = 2;
                double cut1CenterY = StretcherWidth - (cutLength / 2);

                stretcher.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));


                cut1CenterX = width - MaterialThickness34 - 2.75;

                stretcher.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));

                cut1CenterX = interiorWidth / 2;
                cut1CenterY = 1.75;

                cutLength = .5;
                cutWidth = width - (2.75 * 2);

                stretcher.Children.Add(CabinetPartFactory.CreateRectangularCut(
                    cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));
            }

            ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - topDeckAndStretcherThickness - opening1Height, 270, 0, 0);
            cabinet.Children.Add(stretcher);

            if (baseCab.SinkCabinet)
            {
                sinkStretcherPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,opening1Height,0),
                    new (0,opening1Height,0)
                ];

                stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.SinkStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                cabinet.Children.Add(stretcher);
            }
        }

        if (cabType == style2)
        {
            double opening1HeightAdjusted = opening1Height;
            double opening2HeightAdjusted = opening2Height;
            double opening3HeightAdjusted = opening3Height;

            if (baseCab.DrwCount == 2)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);
            }

            if (baseCab.DrwCount == 3)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                opening2HeightAdjusted += MaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);
            }

            if (baseCab.DrwCount == 4)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                opening2HeightAdjusted += MaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                opening3HeightAdjusted += MaterialThickness34;
                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted - opening3HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);
            }

            if (baseCab.SinkCabinet)
            {
                sinkStretcherPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,opening1Height,0),
                    new (0,opening1Height,0)
                ];

                stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.SinkStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                cabinet.Children.Add(stretcher);
            }
        }

        // Shelves
        if (cabType != style2)
        {
            double shelfSpacing = interiorHeight - opening1Height + MaterialThickness34;
            if (baseCab.DrwCount == 0) { shelfSpacing = interiorHeight; }
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);

            for (int i = 1; i < baseCab.ShelfCount + 1; i++)
            {
                double backThicknessForSpacing = backThickness;
                if (backThickness == 0.25) { backThicknessForSpacing = 0; }
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Shelf);
                ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThicknessForSpacing - shelfDepth, i * shelfSpacing, 270, 0, 0);
                cabinet.Children.Add(shelf);
            }
        }


        // Doors
        if (baseCab.DoorCount > 0 && baseCab.IncDoors && cabType != style2 || baseCab.DoorCount > 0 && baseCab.IncDoorsInList && cabType != style2)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);

            double doorWidth = width - (doorSideReveal * 2);
            double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

            if (cabType == style1 && baseCab.DrwCount == 1)
            {
                doorHeight = height - opening1Height - MaterialThickness34 - halfMaterialThickness34 - (baseDoorGap / 2) - doorBottomReveal - tk_Height;
            }

            if (baseCab.DoorCount == 1)
            {
                doorPoints =
                [
                    new (0,0,0),
                    new (doorWidth,0,0),
                    new (doorWidth,doorHeight,0),
                    new (0,doorHeight,0)
                ];

                if (baseCab.IncDoorsInList)
                {
                    addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                }

                if (baseCab.IncDoors)
                {
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
                    if (!baseCab.HasTK)
                    {
                        ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    }
                    else
                    {
                        ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                    }
                    cabinet.Children.Add(door1);
                }
            }

            if (baseCab.DoorCount == 2)
            {
                doorWidth = (doorWidth / 2) - (baseDoorGap / 2);

                doorPoints =
                [
                new (0,0,0),
                new (doorWidth,0,0),
                new (doorWidth, doorHeight, 0),
                new (0,doorHeight,0)
                ];

                if (baseCab.IncDoorsInList)
                {
                    addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                }

                if (baseCab.IncDoors)
                {
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);

                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
                    if (!baseCab.HasTK)
                    {
                        ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                        ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);
                    }
                    else
                    {
                        ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                        ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                    }
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }

        }


        // Drawer Fronts (grouped)
        double drwFrontWidth = width - (doorSideReveal * 2);
        if (result is not null)
            result.DrawerFrontWidth = drwFrontWidth;

        var doorSpeciesForTotalsForDrw = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);

        double[] drwHeights = new[] { drwFront1Height, drwFront2Height, drwFront3Height, drwFront4Height };
        bool[] incFront = new[] { baseCab.IncDrwFront1, baseCab.IncDrwFront2, baseCab.IncDrwFront3, baseCab.IncDrwFront4 };
        bool[] incFrontInList = new[] { baseCab.IncDrwFrontInList1, baseCab.IncDrwFrontInList2, baseCab.IncDrwFrontInList3, baseCab.IncDrwFrontInList4 };

        int maxFronts = Math.Min(4, Math.Max(0, baseCab.DrwCount));

        if (result is not null)
        {
            for (int i = 0; i < maxFronts; i++)
                result.DrawerFrontHeights.Add(drwHeights[i]);
        }

        for (int fi = 0; fi < maxFronts; fi++)
        {
            double h = drwHeights[fi];

            if (incFrontInList[fi])
            {
                addFrontPartRow(baseCab, $"Drawer Front {fi + 1}", h, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
            }

            if (!incFront[fi]) continue;

            drwFrontPoints =
            [
                new (0,0,0),
                new (drwFrontWidth,0,0),
                new (drwFrontWidth,h,0),
                new (0,h,0)
            ];

            double cumulativeHeight = 0;
            for (int k = 0; k <= fi; k++) cumulativeHeight += drwHeights[k];

            double yPos = height - doorTopReveal - cumulativeHeight - (fi * baseDoorGap);

            var front = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotalsForDrw, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.DrawerFront);
            ModelTransforms.ApplyTransform(front, -(width / 2) + doorLeftReveal, yPos, depth, 0, 0, 0);
            cabinet.Children.Add(front);
        }


        // Drawer Boxes (grouped)
        if (baseCab.DrwCount > 0)
        {
            if (baseCab.IncDrwBoxOpening1 || baseCab.IncDrwBoxOpening2 || baseCab.IncDrwBoxOpening3 || baseCab.IncDrwBoxOpening4 || baseCab.IncDrwBoxInListOpening1 || baseCab.IncDrwBoxInListOpening2 || baseCab.IncDrwBoxInListOpening3 || baseCab.IncDrwBoxInListOpening4)
            {
                double topSpacing = 0;
                double bottomSpacing = 0;

                if (baseCab.DrwStyle is not null)
                {
                    if (baseCab.DrwStyle.Contains("Blum"))
                    {
                        dbxWidth -= tandemSideSpacing;
                        topSpacing = tandemTopSpacing;
                        bottomSpacing = tandemBottomSpacing;
                    }
                    else if (baseCab.DrwStyle.Contains("Accuride"))
                    {
                        dbxWidth -= accurideSideSpacing;
                        topSpacing = accurideTopSpacing;
                        bottomSpacing = accurideBottomSpacing;
                    }
                }

                if (result is not null)
                    result.DrawerBoxWidth = dbxWidth;

                double[] openingHeights = new[] { opening1Height, opening2Height, opening3Height, opening4Height };
                bool[] incBoxOpening = new[] { baseCab.IncDrwBoxOpening1, baseCab.IncDrwBoxOpening2, baseCab.IncDrwBoxOpening3, baseCab.IncDrwBoxOpening4 };
                bool[] incBoxInList = new[] { baseCab.IncDrwBoxInListOpening1, baseCab.IncDrwBoxInListOpening2, baseCab.IncDrwBoxInListOpening3, baseCab.IncDrwBoxInListOpening4 };

                for (int oi = 0; oi < 4; oi++)
                {
                    int openingIndex = oi + 1;
                    if (baseCab.DrwCount < openingIndex) break;

                    dbxHeight = openingHeights[oi] - topSpacing - bottomSpacing;

                    if (baseCab.DrwCount > openingIndex)
                    {
                        dbxHeight -= tandemMidDrwBottomSpacingAdjustment;
                    }

                    result?.DrawerBoxHeights.Add(dbxHeight);

                    if (incBoxInList[oi])
                    {
                        addDrawerBoxRow(baseCab, $"Drawer Box {openingIndex}", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (!incBoxOpening[oi]) continue;

                    var dbxRotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                    Model3DGroup dbxGroup = new();
                    dbxGroup.Children.Add(dbxRotate);

                    double prevOpeningsSum = 0;
                    for (int p = 0; p < oi; p++) prevOpeningsSum += openingHeights[p];

                    double y = height - dbxHeight - MaterialThickness34 - topSpacing - prevOpeningsSum - (MaterialThickness34 * oi);

                    ModelTransforms.ApplyTransform(dbxGroup, (dbxWidth / 2) - MaterialThickness34, y, interiorDepth + backThickness, 0, 0, 0);
                    cabinet.Children.Add(dbxGroup);
                }
            }
        }

        // Rollouts or Trash Drawer
        if (baseCab.IncRollouts || baseCab.IncRolloutsInList || baseCab.IncTrashDrwBox)
        {
            const double rolloutMountBracketSpacing = 1;
            dbxHeight = rolloutHeight;

            if (baseCab.RolloutStyle is not null)
            {
                if (baseCab.RolloutStyle.Contains("Blum"))
                {
                    dbxWidth = interiorWidth - tandemSideSpacing;
                }
                else if (baseCab.RolloutStyle.Contains("Accuride"))
                {
                    dbxWidth = interiorWidth - accurideSideSpacing;
                }
            }

            if (baseCab.RolloutCount > 0)
            {
                dbxWidth -= rolloutMountBracketSpacing * baseCab.DoorCount;
            }

            // ── Capture rollout dims — these are the actual values used for geometry ──
            if (result is not null)
            {
                result.RolloutWidth = dbxWidth;
                result.RolloutHeight = dbxHeight;
                result.RolloutDepth = dbxDepth;
            }

            double dbxFrontAndBackWidth = dbxWidth - (MaterialThickness34 * 2);
            double dbxBottomWidth = dbxWidth - (MaterialThickness34 * 2);
            double dbxBottomLength = dbxDepth - (MaterialThickness34 * 2);

            if (baseCab.RolloutCount >= 1 || (baseCab.IncTrashDrwBox && baseCab.TrashDrawer))
            {
                if (baseCab.IncTrashDrwBox && baseCab.TrashDrawer)
                {
                    dbxHeight = 12;
                }

                if (baseCab.IncRollouts)
                {
                    for (int r = 0; r < baseCab.RolloutCount; r++)
                    {
                        if (baseCab.IncRolloutsInList)
                        {
                            addDrawerBoxRow(baseCab, "Rollout", dbxHeight, dbxWidth, dbxDepth);
                        }

                        var rotateGroup = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        var placement = new Model3DGroup();
                        placement.Children.Add(rotateGroup);
                        ModelTransforms.ApplyTransform(placement, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906 + (r * 6), interiorDepth + backThickness - .25, 0, 0, 0);
                        cabinet.Children.Add(placement);
                    }
                }

                if (baseCab.DrwStyle is not null)
                {
                    if (baseCab.DrwStyle.Contains("Blum"))
                    {
                        dbxWidth = interiorWidth - tandemSideSpacing;
                    }
                    else if (baseCab.DrwStyle.Contains("Accuride"))
                    {
                        dbxWidth = interiorWidth - accurideSideSpacing;
                    }
                }

                if (baseCab.IncTrashDrwBox && baseCab.TrashDrawer)
                {
                    if (baseCab.IncDrwBoxesInList)
                    {
                        addDrawerBoxRow(baseCab, "Trash Drawer", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (baseCab.IncDrwBoxes)
                    {
                        var rotateGroup = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        var trashDrawer = new Model3DGroup();
                        trashDrawer.Children.Add(rotateGroup);
                        ModelTransforms.ApplyTransform(trashDrawer, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(trashDrawer);
                    }
                }
            }
        }

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(back);
        cabinet.Children.Add(toekick);
    }
}