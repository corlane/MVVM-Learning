using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildStandard(
        Model3DGroup cabinet,
        UpperCabinetModel upperCab,
        UpperCabinetDimensions dim,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double MaterialThickness14 = MaterialDefaults.Thickness14;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;

        string doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(upperCab.DoorSpecies);

        double StretcherWidth = 4;

        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double backThickness = dim.BackThickness;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double interiorHeight = dim.InteriorHeight;
        double shelfDepth = dim.ShelfDepth;
        double upperDoorGap = dim.DoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double doorSideReveal = dim.DoorSideReveal;
        double backInsetForDeckAndTop = 0;

        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";

        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top;
        Model3DGroup shelf;
        Model3DGroup back;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup nailer;

        List<Point3D> endPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> backPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> nailerPoints;

        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

        // ----------------------------
        // HOLES
        // IMPORTANT: add holes before ApplyTransform(leftEnd/rightEnd, ...)
        // ----------------------------

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


        // End panel transforms
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);

        if (backThickness == MaterialThickness34) { backInsetForDeckAndTop = MaterialThickness34; }
        // Deck
        deckPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,depth - backInsetForDeckAndTop,0),
            new (0,depth - backInsetForDeckAndTop,0)
        ];
        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Deck);
        ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, 0, 270, 0, 0);

        // Full Top
        topPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,depth - backInsetForDeckAndTop,0),
            new (0,depth - backInsetForDeckAndTop,0)
        ];
        top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Top);
        ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);

        // Back
        if (backThickness == 0.75)
        {
            backPoints =
            [
                new (0,-MaterialThickness34,0),
                new (interiorWidth,-MaterialThickness34,0),
                new (interiorWidth,interiorHeight + (MaterialThickness34),0),
                new (0,interiorHeight + (MaterialThickness34),0)
            ];
            if (width <= 47.75 + (2 * MaterialThickness34))
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "Hardrock Maple", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackUpper34);
            }
            else
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "Hardrock Maple", "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackUpper34);
            }
            ModelTransforms.ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34, 0, 0, 0, 0);
        }
        else
        {
            backPoints =
            [
                new (0,0,0),
                new (width,0,0),
                new (width,height,0),
                new (0,height,0)
            ];
            if (width <= 47.75)
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackUpper14);
            }
            else
            {
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, "None", "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.BackUpper14);
            }
            ModelTransforms.ApplyTransform(back, -(width / 2), 0, -MaterialThickness14, 0, 0, 0);

            nailerPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,StretcherWidth,0),
                new (0,StretcherWidth,0)
            ];

            nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);

            nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Nailer);
            ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), 0 + MaterialThickness34, 0, 0, 0, 0);
            cabinet.Children.Add(nailer);
        }

        // Shelves
        double shelfSpacing = interiorHeight + MaterialThickness34;
        shelfSpacing /= (upperCab.ShelfCount + 1);

        for (int i = 1; i < upperCab.ShelfCount + 1; i++)
        {
            double backThicknessForSpacing = backThickness;
            if (backThickness == 0.25) { backThicknessForSpacing = 0; }

            shelfPoints =
            [
                new (0,0,0),
                new (interiorWidth-.125,0,0),
                new (interiorWidth-.125,shelfDepth,0),
                new (0,shelfDepth,0)
            ];
            shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Shelf);
            ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThicknessForSpacing - shelfDepth, i * shelfSpacing, 270, 0, 0);
            cabinet.Children.Add(shelf);
        }

        // Doors
        if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
        {
            var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

            double doorWidth = width - (doorSideReveal * 2);
            double doorHeight = height - doorTopReveal - doorBottomReveal;

            if (upperCab.DoorCount == 1)
            {
                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth,doorHeight,0),
                        new (0,doorHeight,0)
                    ];

                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    cabinet.Children.Add(door1);
                }
            }

            if (upperCab.DoorCount == 2)
            {
                doorWidth = (doorWidth / 2) - (upperDoorGap / 2);

                if (upperCab.IncDoorsInList)
                {
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                    addFrontPartRow(upperCab, "Door", doorHeight, doorWidth, upperCab.DoorSpecies, upperCab.DoorGrainDir);
                }

                if (upperCab.IncDoors)
                {
                    doorPoints =
                    [
                        new (0,0,0),
                        new (doorWidth,0,0),
                        new (doorWidth, doorHeight, 0),
                        new (0,doorHeight,0)
                    ];

                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);

                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);

                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }
        }

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(back);
    }
}