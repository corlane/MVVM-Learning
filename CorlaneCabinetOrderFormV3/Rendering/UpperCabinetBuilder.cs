using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class UpperCabinetBuilder
{
    internal static void BuildUpper(
        Model3DGroup cabinet,
        UpperCabinetModel upperCab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top;
        Model3DGroup shelf;
        Model3DGroup back;
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup nailer;

        List<Point3D> endPanelPoints;
        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> backPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> nailerPoints;

        double MaterialThickness34 = 0.75;
        double MaterialThickness14 = 0.25;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;

        string? cabType = upperCab.Style;
        string style1 = CabinetStyles.Upper.Standard;
        string style2 = CabinetStyles.Upper.Corner90;
        string style3 = CabinetStyles.Upper.AngleFront;

        double width = ConvertDimension.FractionToDouble(upperCab.Width);
        double height = ConvertDimension.FractionToDouble(upperCab.Height);
        double depth = ConvertDimension.FractionToDouble(upperCab.Depth);

        double backThickness = ConvertDimension.FractionToDouble(upperCab.BackThickness);
        if (backThickness == 0.25) { depth -= backThickness; }

        double leftFrontWidth = ConvertDimension.FractionToDouble(upperCab.LeftFrontWidth);
        double rightFrontWidth = ConvertDimension.FractionToDouble(upperCab.RightFrontWidth);
        double leftDepth = ConvertDimension.FractionToDouble(upperCab.LeftDepth);
        double rightDepth = ConvertDimension.FractionToDouble(upperCab.RightDepth);
        double leftBackWidth = ConvertDimension.FractionToDouble(upperCab.LeftBackWidth);
        double rightBackWidth = ConvertDimension.FractionToDouble(upperCab.RightBackWidth);

        double interiorWidth = width - (MaterialThickness34 * 2);
        double interiorDepth = depth - backThickness;
        double interiorHeight = height - doubleMaterialThickness34;

        int shelfCount = upperCab.ShelfCount;
        double shelfDepth = interiorDepth;
        shelfDepth -= 0.125;

        double upperDoorGap = ConvertDimension.FractionToDouble(upperCab.GapWidth);
        double doorLeftReveal = ConvertDimension.FractionToDouble(upperCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(upperCab.RightReveal);
        double doorTopReveal = ConvertDimension.FractionToDouble(upperCab.TopReveal);
        double doorBottomReveal = ConvertDimension.FractionToDouble(upperCab.BottomReveal);

        double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2;

        double StretcherWidth = 4;
        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";

        string doorEdgebandingSpecies;

        doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(upperCab.DoorSpecies);

        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

        if (string.Equals(cabType, style1, StringComparison.OrdinalIgnoreCase))
        {
            ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
            ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
        }

        if (string.Equals(cabType, style1, StringComparison.OrdinalIgnoreCase))
        {
            // Deck
            deckPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
            ModelTransforms.ApplyTransform(deck, -(interiorWidth / 2), -depth, 0, 270, 0, 0);

            // Full Top
            topPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
            ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);

            // Back
            if (backThickness == 0.75)
            {
                backPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,interiorHeight,0),
                    new (0,interiorHeight,0)
                ];
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
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
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(back, -(width / 2), 0, -MaterialThickness14, 0, 0, 0);

                nailerPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                ];

                nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);

                nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), 0 + MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);
            }

            // Shelves
            double shelfSpacing = interiorHeight + MaterialThickness34;
            shelfSpacing /= (upperCab.ShelfCount + 1);

            for (int i = 1; i < upperCab.ShelfCount + 1; i++)
            {
                shelfPoints =
                [
                    new (0,0,0),
                    new (interiorWidth-.125,0,0),
                    new (interiorWidth-.125,shelfDepth,0),
                    new (0,shelfDepth,0)
                ];
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -MaterialThickness34 - shelfDepth, i * shelfSpacing, 270, 0, 0);
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

                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
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

                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);

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

        // 90 deg. Corner Cabinet Style 2
        if (cabType == style2)
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

            leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ModelTransforms.ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
            ModelTransforms.ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

            deckPoints =
            [
                new (0,0,0),
                new (leftFrontWidth-MaterialThickness34,0,0),
                new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
                new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
                new (0,-leftDepth + doubleMaterialThickness34,0),
            ];

            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);
            top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);

            ModelTransforms.ApplyTransform(top, 0, leftDepth, -height, 90, 0, 0);
            ModelTransforms.ApplyTransform(deck, 0, leftDepth, -MaterialThickness34, 90, 0, 0);

            backPoints =
            [
                new (0,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,0,0),
                new (leftFrontWidth + rightDepth - MaterialThickness34  - MaterialThickness34,height,0),
                new (0,height,0)
            ];
            leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(leftBack, 0, 0, MaterialThickness34, 0, 0, 0);

            backPoints =
            [
                new (0,0,0),
                new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
                new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height,0),
                new (0,height,0),
            ];
            rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, 0, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);

            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                    [
                        new (0,0,0),
                        new (leftFrontWidth-MaterialThickness34-gap,0,0),
                        new (leftFrontWidth-MaterialThickness34-gap, rightFrontWidth-MaterialThickness34-gap,0),
                        new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,rightFrontWidth - MaterialThickness34-gap,0),
                        new (leftFrontWidth - MaterialThickness34-gap + rightDepth - doubleMaterialThickness34 - gap,-leftDepth + doubleMaterialThickness34 + gap,0),
                        new (0,-leftDepth + doubleMaterialThickness34 + gap,0),
                    ];
                    shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false);
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
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);

                    doorPoints =
                    [
                        new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                    ];
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);

                    ModelTransforms.ApplyTransform(door1, -MaterialThickness34 + doorLeftReveal, doorBottomReveal, leftDepth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, -leftDepth - door2Width - cornerCabDoorOpenSideReveal, doorBottomReveal, leftFrontWidth - (doubleMaterialThickness34), 0, 90, 0);
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
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

        // Angle Front Corner Cabinet Style 3
        if (cabType == style3)
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

            leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ModelTransforms.ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 90, 0);
            ModelTransforms.ApplyTransform(rightEnd, -leftBackWidth, 0, -rightBackWidth, 0, 0, 0);

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
            double frontWidth = Math.Sqrt(vx * vx + vy * vy);
            double angle = Math.Atan2(vy, vx); // radians

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

            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);
            top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Horizontal", upperCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);

            ModelTransforms.ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
            ModelTransforms.ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);

            var deckRotated = new Model3DGroup();
            var topRotated = new Model3DGroup();

            deckRotated.Children.Add(deck);
            topRotated.Children.Add(top);

            ModelTransforms.ApplyTransform(deckRotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
            ModelTransforms.ApplyTransform(topRotated, -MaterialThickness34, height - MaterialThickness34, -leftDepth, 0, 0, 0);

            backPoints =
            [
                new (0,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,0,0),
                new (leftBackWidth - MaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

            backPoints =
            [
                new (0,0,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,0,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);

            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - doubleMaterialThickness34) / (shelfCount + 1);
                for (int i = 1; i < shelfCount + 1; i++)
                {
                    shelfPoints =
                    [
                        new (leftDepth,MaterialThickness34 + gap,0),
                        new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - rightDepth,0),
                        new (rightBackWidth - MaterialThickness34 - gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                        new (MaterialThickness34 + .25 + gap, leftBackWidth - MaterialThickness34 - .25 - gap,0),
                        new (MaterialThickness34 + .25 + gap, MaterialThickness34 + gap,0),
                    ];
                    shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, upperCab.Species, getMatchingEdgebandingSpecies(upperCab.Species), "Horizontal", upperCab, true, isPanel, panelEBEdges, isFaceUp: false, 45);
                    ModelTransforms.ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                    cabinet.Children.Add(shelf);
                }
            }

            if (upperCab.DoorCount > 0 && upperCab.IncDoors || upperCab.DoorCount > 0 && upperCab.IncDoorsInList)
            {
                var doorSpeciesForTotals = resolveDoorSpeciesForTotals(upperCab.DoorSpecies, upperCab.CustomDoorSpecies);

                double door1Width = frontWidth - doorLeftReveal - doorRightReveal;
                double doorHeight = height - doorTopReveal - doorBottomReveal;

                if (upperCab.DoorCount == 1)
                {
                    if (upperCab.IncDoorsInList)
                    {
                        addFrontPartRow(upperCab, "Door", doorHeight, door1Width, upperCab.DoorSpecies, upperCab.DoorGrainDir);
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
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);
                    }
                }

                if (upperCab.DoorCount == 2)
                {
                    door1Width = (frontWidth / 2) - doorLeftReveal - (upperDoorGap / 2);
                    double door2Width = (frontWidth / 2) - doorRightReveal - (upperDoorGap / 2);

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
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);

                        doorPoints =
                        [
                            new (0,0,0),
                            new (door2Width,0,0),
                            new (door2Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                        door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, upperCab.DoorGrainDir, upperCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door2, door1Width + doorLeftReveal + upperDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);

                        var door2Rotated = new Model3DGroup();
                        door2Rotated.Children.Add(door2);
                        ModelTransforms.ApplyTransform(door2Rotated, -MaterialThickness34, 0, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }

            if (!leftEndHidden) cabinet.Children.Add(leftEnd);
            if (!rightEndHidden) cabinet.Children.Add(rightEnd);
            if (!deckHidden) cabinet.Children.Add(deckRotated);
            if (!topHidden) cabinet.Children.Add(topRotated);

            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
        }
    }
}