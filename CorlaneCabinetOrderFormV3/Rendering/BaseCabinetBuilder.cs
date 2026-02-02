using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class BaseCabinetBuilder
{
    internal static void BuildBase(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow)
    {
        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup topStretcherFront;
        Model3DGroup topStretcherBack;
        Model3DGroup stretcher;
        Model3DGroup shelf;
        Model3DGroup toekick = new();
        Model3DGroup toekick1;
        Model3DGroup toekick2;
        Model3DGroup back;
        Model3DGroup leftBack;
        Model3DGroup rightBack;
        Model3DGroup door1;
        Model3DGroup door2;
        Model3DGroup drwFront1;
        Model3DGroup drwFront2;
        Model3DGroup drwFront3;
        Model3DGroup drwFront4;
        Model3DGroup dbxLeftSide;
        Model3DGroup dbxRightSide;
        Model3DGroup dbxFront;
        Model3DGroup dbxBack;
        Model3DGroup dbxBottom;
        Model3DGroup nailer;



        List<Point3D> endPanelPoints;
        List<Point3D> leftEndPanelPoints;
        List<Point3D> rightEndPanelPoints;
        List<Point3D> deckPoints;
        List<Point3D> topPoints;
        List<Point3D> topStretcherFrontPoints;
        List<Point3D> topStretcherBackPoints;
        List<Point3D> toekickPoints;
        List<Point3D> backPoints;
        List<Point3D> stretcherPoints;
        List<Point3D> sinkStretcherPoints;
        List<Point3D> shelfPoints;
        List<Point3D> doorPoints;
        List<Point3D> drwFrontPoints;
        List<Point3D> dbxSidePoints;
        List<Point3D> dbxFrontAndBackPoints;
        List<Point3D> dbxBottomPoints;
        List<Point3D> nailerPoints;



        double MaterialThickness34 = 0.75;
        double MaterialThickness14 = 0.25;
        double halfMaterialThickness34 = MaterialThickness34 / 2; // This is to make door calcs etc. more straightforward
        double doubleMaterialThickness34 = MaterialThickness34 * 2; // This is to make door calcs etc. more straightforward

        double backLegWidth = 3;
        double StretcherWidth = 6;
        double topStretcherBackWidth = 3;

        string? cabType = baseCab.Style;
        string style1 = CabinetStyles.Base.Standard;
        string style2 = CabinetStyles.Base.Drawer;
        string style3 = CabinetStyles.Base.Corner90;
        string style4 = CabinetStyles.Base.AngleFront;

        string doorEdgebandingSpecies;

        doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(baseCab.DoorSpecies);

        double width = ConvertDimension.FractionToDouble(baseCab.Width);
        double height = ConvertDimension.FractionToDouble(baseCab.Height);
        double depth = ConvertDimension.FractionToDouble(baseCab.Depth);
        double backThickness = ConvertDimension.FractionToDouble(baseCab.BackThickness);
        if (backThickness == 0.25) { depth -= backThickness; }
        double leftFrontWidth = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth);
        double rightFrontWidth = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth);
        double leftDepth = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
        double rightDepth = ConvertDimension.FractionToDouble(baseCab.RightDepth);
        double leftBackWidth = ConvertDimension.FractionToDouble(baseCab.LeftBackWidth);
        double rightBackWidth = ConvertDimension.FractionToDouble(baseCab.RightBackWidth);
        double tk_Height = ConvertDimension.FractionToDouble(baseCab.TKHeight ?? "4");
        double tk_Depth = ConvertDimension.FractionToDouble(baseCab.TKDepth ?? "3");
        double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
        double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
        double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);
        double opening4Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight4);
        double drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);
        double drwFront2Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2);
        double drwFront3Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3);
        double drwFront4Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4);
        double interiorWidth = width - (MaterialThickness34 * 2);
        double interiorDepth = depth - backThickness;
        double interiorHeight;
        double shelfDepth;
        if (string.Equals(baseCab.ShelfDepth, CabinetOptions.ShelfDepth.HalfDepth, StringComparison.OrdinalIgnoreCase))
        {
            shelfDepth = interiorDepth / 2;
        }
        else
        {
            shelfDepth = interiorDepth;
        }
        //shelfDepth -= 0.125;
        double baseDoorGap = ConvertDimension.FractionToDouble(baseCab.GapWidth);
        double doorLeftReveal = ConvertDimension.FractionToDouble(baseCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(baseCab.RightReveal);
        double doorTopReveal = ConvertDimension.FractionToDouble(baseCab.TopReveal);
        double doorBottomReveal = ConvertDimension.FractionToDouble(baseCab.BottomReveal);
        double doorSideReveal = (doorLeftReveal + doorRightReveal) / 2; // this averages the potentially different left and right reveals so that the door creation calc can use just one variable instead of two.
        bool topDeck90 = false; // This is sent to the panel creator to let it know if it is a top or deck at 90 degrees so it cab have 2 edgebanded edges
        bool isPanel = false; // This is sent to the panel creator to let it know if it is a panel (true) so it can have edgebanding applied correctly. Also using it for doors and drawer fronts so they are banded on all 4 edges.
        int shelfCount = baseCab.ShelfCount;
        double dbxWidth = interiorWidth;
        double dbxHeight;
        double dbxDepth = interiorDepth - 1;
        if (depth >= 10.625 && depth < 13.625) dbxDepth = 9;
        if (depth >= 13.625 && depth < 16.625) dbxDepth = 12;
        if (depth >= 16.625 && depth < 19.625) dbxDepth = 15;
        if (depth >= 19.625 && depth < 22.625) dbxDepth = 18;
        if (depth >= 22.625) dbxDepth = 21;
        double tandemSideSpacing = .4; // TODO confirm and set to proper value
        double tandemTopSpacing = .375;
        double tandemBottomSpacing = .5906; // This is for top & bottom drws. Middle drws will have .375 additional (.9656) bottom spacing 
        double tandemMidDrwBottomSpacingAdjustment = 0;// lol there I fixed it .375; // see above
        double accurideSideSpacing = 1; // TODO confirm and set to proper value
        double accurideTopSpacing = .5; // TODO confirm and set to proper value
        double accurideBottomSpacing = .5; // TODO confirm and set to proper value
        double rolloutHeight = 4;

        string panelEBEdges = "";

        stretcherPoints =
        [
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,StretcherWidth,0),
            new (0,StretcherWidth,0)
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
            tk_Height = 0;
            tk_Depth = 0;

            endPanelPoints =
            [
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];
        }

        interiorHeight = height - doubleMaterialThickness34 - tk_Height;

        //Debug.WriteLine($"End Panels:");

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

        if (cabType == style1 || cabType == style2)
        {
            ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
            ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);
        }

        if (cabType == style1 || cabType == style2)
        {
            //Debug.WriteLine("Deck:");

            // Deck
            deckPoints =
            [
                new (0,0,0),
                new (interiorWidth,0,0),
                new (interiorWidth,depth,0),
                new (0,depth,0)
            ];
            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
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
                top = CabinetPartFactory.CreatePanel(topPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(top, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
            }

            else
            {
                //Debug.WriteLine("Stretcher Top");

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


                topStretcherFront = CabinetPartFactory.CreatePanel(topStretcherFrontPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                topStretcherBack = CabinetPartFactory.CreatePanel(topStretcherBackPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);

                ModelTransforms.ApplyTransform(topStretcherFront, -(interiorWidth / 2), -depth, height - MaterialThickness34, 270, 0, 0);
                ModelTransforms.ApplyTransform(topStretcherBack, -(interiorWidth / 2), -topStretcherBackWidth, height - MaterialThickness34, 270, 0, 0);
                top.Children.Add(topStretcherFront);
                top.Children.Add(topStretcherBack);
            }

            // Toekick
            if (baseCab.HasTK)
            {
                //Debug.WriteLine("Toekick");

                toekickPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(toekick, -(interiorWidth / 2), 0.5, depth - tk_Depth - MaterialThickness34, 0, 0, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
            }

            // Back
            if (backThickness == 0.75)
            {
                //Debug.WriteLine("Back");

                backPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,interiorHeight,0),
                    new (0,interiorHeight,0)
                ];
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(back, -(interiorWidth / 2), MaterialThickness34 + tk_Height, 0, 0, 0, 0);
            }
            else
            {
                //Debug.WriteLine("Back");

                backPoints =
                [
                    new (0,0,0),
                    new (width,0,0),
                    new (width,height-tk_Height,0),
                    new (0,height-tk_Height,0)
                ];
                back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness14, "PFP 1/4", "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(back, -(width / 2), tk_Height, -MaterialThickness14, 0, 0, 0);

                nailerPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,StretcherWidth,0),
                    new (0,StretcherWidth,0)
                ];

                nailer = CabinetPartFactory.CreatePanel(nailerPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(nailer, -(interiorWidth / 2), height - StretcherWidth - MaterialThickness34, 0, 0, 0, 0);
                cabinet.Children.Add(nailer);
            }

            // Drawer Stretchers
            if (cabType == style1 && baseCab.DrwCount == 1)
            {
                //Debug.WriteLine("Drw Stretcher");

                double topDeckAndStretcherThickness = (baseCab.DrwCount + 1) * MaterialThickness34;

                stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - topDeckAndStretcherThickness - opening1Height, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                if (baseCab.SinkCabinet)
                {
                    //Debug.WriteLine("Sink Stretcher");

                    sinkStretcherPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,opening1Height,0),
                        new (0,opening1Height,0)
                    ];

                    stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
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
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.DrwCount == 3)
                {
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening2HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.DrwCount == 4)
                {
                    //Debug.WriteLine("Drw Stretcher");

                    opening1HeightAdjusted += doubleMaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening2HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);

                    //Debug.WriteLine("Drw Stretcher");

                    opening3HeightAdjusted += MaterialThickness34; // moves the reference to the bottom of the stretcher
                    stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted - opening3HeightAdjusted, 270, 0, 0);
                    cabinet.Children.Add(stretcher);
                }

                if (baseCab.SinkCabinet)
                {
                    //Debug.WriteLine("Sink Stretcher");

                    sinkStretcherPoints =
                    [
                        new (0,0,0),
                        new (interiorWidth,0,0),
                        new (interiorWidth,opening1Height,0),
                        new (0,opening1Height,0)
                    ];

                    stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                    cabinet.Children.Add(stretcher);
                }
            }

            // Shelves
            if (cabType != style2)
            {
                double shelfSpacing = interiorHeight - opening1Height + MaterialThickness34; // This should be the space between the shelves
                if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; } // why the fuck does this work - oh well, it does.
                shelfSpacing /= (baseCab.ShelfCount + 1);

                for (int i = 1; i < baseCab.ShelfCount + 1; i++)
                {
                    //Debug.WriteLine("Shelf");

                    shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                    ModelTransforms.ApplyTransform(shelf, -(interiorWidth / 2) + .0625, -backThickness - shelfDepth, i * shelfSpacing, 270, 0, 0);
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

                    //Debug.WriteLine("Door");

                    if (baseCab.IncDoorsInList)
                    {
                        addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
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

                    //Debug.WriteLine("Door");

                    if (baseCab.IncDoorsInList)
                    {
                        addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                        addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                    }

                    if (baseCab.IncDoors)
                    {
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);

                        //Debug.WriteLine("Door");

                        door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
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


            // Drawer Fronts
            double drwFrontWidth = width - (doorSideReveal * 2);

            if (baseCab.IncDrwFront1 || baseCab.IncDrwFront2 || baseCab.IncDrwFront3 || baseCab.IncDrwFront4 || baseCab.IncDrwFrontInList1 || baseCab.IncDrwFrontInList2 || baseCab.IncDrwFrontInList3 || baseCab.IncDrwFrontInList4)
            {
                var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);

                if (cabType == style1 && baseCab.DrwCount == 1 && baseCab.IncDrwFront1 || cabType == style1 && baseCab.DrwCount == 1 && baseCab.IncDrwFrontInList1)
                {
                    drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                    drwFrontPoints =
                    [
                        new (0,0,0),
                        new (drwFrontWidth,0,0),
                        new (drwFrontWidth,drwFront1Height,0),
                        new (0,drwFront1Height,0)
                    ];

                    //Debug.WriteLine("Drw Front");
                    if (baseCab.IncDrwFrontInList1)
                    {
                        addFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                    }

                    if (baseCab.IncDrwFront1)
                    {
                        drwFront1 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                        cabinet.Children.Add(drwFront1);
                    }
                }

                if (cabType == "Drawer")
                {
                    if (baseCab.DrwCount == 1 && baseCab.IncDrwFront1 || baseCab.DrwCount == 1 && baseCab.IncDrwFrontInList1)
                    {
                        drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                        drwFrontPoints =
                        [
                            new (0,0,0),
                            new (drwFrontWidth,0,0),
                            new (drwFrontWidth,drwFront1Height,0),
                            new (0,drwFront1Height,0)
                        ];
                        //Debug.WriteLine("Drw Front");

                        if (baseCab.IncDrwFrontInList1)
                        {
                            addFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                        }

                        if (baseCab.IncDrwFront1)
                        {
                            drwFront1 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                            ModelTransforms.ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                            cabinet.Children.Add(drwFront1);
                        }
                    }

                    if (baseCab.DrwCount > 1)
                    {
                        // Top Drawer
                        if (baseCab.IncDrwFront1 || baseCab.IncDrwFrontInList1)
                        {
                            drwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1);

                            drwFrontPoints =
                            [
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront1Height,0),
                                new (0,drwFront1Height,0)
                            ];
                            //Debug.WriteLine("Drw Front");

                            if (baseCab.IncDrwFrontInList1)
                            {
                                addFrontPartRow(baseCab, "Drawer Front 1", drwFront1Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                            }

                            if (baseCab.IncDrwFront1)
                            {
                                drwFront1 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                ModelTransforms.ApplyTransform(drwFront1, -(width / 2) + doorLeftReveal, height - drwFront1Height - doorTopReveal, depth, 0, 0, 0);
                                cabinet.Children.Add(drwFront1);
                            }
                        }

                        // Second Drawer

                        if (baseCab.IncDrwFront2 || baseCab.IncDrwFrontInList2)
                        {
                            if (baseCab.DrwCount == 2) // if true, this is the bottom drawer
                            {
                                drwFront2Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2);
                            }

                            drwFrontPoints =
                            [
                                new (0,0,0),
                                new (drwFrontWidth,0,0),
                                new (drwFrontWidth,drwFront2Height,0),
                                new (0,drwFront2Height,0)
                            ];
                            //Debug.WriteLine("Drw Front");

                            if (baseCab.IncDrwFrontInList2)
                            {
                                addFrontPartRow(baseCab, "Drawer Front 2", drwFront2Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                            }

                            if (baseCab.IncDrwFront2)
                            {
                                drwFront2 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                ModelTransforms.ApplyTransform(drwFront2,
                                    -(width / 2) + doorLeftReveal,
                                    height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height,
                                    depth,
                                    0, 0, 0);

                                cabinet.Children.Add(drwFront2);
                            }
                        }

                        if (baseCab.DrwCount > 2)
                        {
                            // Third Drawer
                            if (baseCab.IncDrwFront3 || baseCab.IncDrwFrontInList3)
                            {
                                if (baseCab.DrwCount == 3) // if true, this is the bottom drawer
                                {
                                    drwFront3Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3);
                                }

                                drwFrontPoints =
                                [
                                    new (0,0,0),
                                    new (drwFrontWidth,0,0),
                                    new (drwFrontWidth,drwFront3Height,0),
                                    new (0,drwFront3Height,0)
                                ];
                                //Debug.WriteLine("Drw Front");

                                if (baseCab.IncDrwFrontInList3)
                                {
                                    addFrontPartRow(baseCab, "Drawer Front 3", drwFront3Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                                }

                                if (baseCab.IncDrwFront3)
                                {
                                    drwFront3 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                    ModelTransforms.ApplyTransform(drwFront3,
                                        -(width / 2) + doorLeftReveal,
                                        height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height - baseDoorGap - drwFront3Height,
                                        depth,
                                        0, 0, 0);

                                    cabinet.Children.Add(drwFront3);
                                }
                            }

                        }

                        // Fourth Drawer
                        if (baseCab.DrwCount > 3)
                        {
                            if (baseCab.IncDrwFront4 || baseCab.IncDrwFrontInList4)
                            {
                                if (baseCab.DrwCount == 4) // if true, this is the bottom drawer
                                {
                                    drwFront4Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4);
                                }

                                drwFrontPoints =
                                [
                                    new (0,0,0),
                                    new (drwFrontWidth,0,0),
                                    new (drwFrontWidth,drwFront4Height,0),
                                    new (0,drwFront4Height,0)
                                ];
                                //Debug.WriteLine("Drw Front");

                                if (baseCab.IncDrwFrontInList4)
                                {
                                    addFrontPartRow(baseCab, "Drawer Front 4", drwFront4Height, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
                                }

                                if (baseCab.IncDrwFront4)
                                {
                                    drwFront4 = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                                    ModelTransforms.ApplyTransform(drwFront4,
                                        -(width / 2) + doorLeftReveal,
                                        height - doorTopReveal - drwFront1Height - baseDoorGap - drwFront2Height - baseDoorGap - drwFront3Height - baseDoorGap - drwFront4Height,
                                        depth,
                                        0, 0, 0);

                                    cabinet.Children.Add(drwFront4);
                                }
                            }
                        }
                    }
                }
            }

            // Drawer Boxes
            if (baseCab.DrwCount > 0)
            {
                if (baseCab.IncDrwBoxOpening1 || baseCab.IncDrwBoxOpening2 || baseCab.IncDrwBoxOpening3 || baseCab.IncDrwBoxOpening4 || baseCab.IncDrwBoxInListOpening1 || baseCab.IncDrwBoxInListOpening2 || baseCab.IncDrwBoxInListOpening3 || baseCab.IncDrwBoxInListOpening4)
                {
                    //double sideSpacing;
                    double topSpacing = 0;
                    double bottomSpacing = 0;

                    if (baseCab.DrwStyle is not null)
                    {
                        if (baseCab.DrwStyle.Contains("Blum"))
                        {
                            dbxWidth -= tandemSideSpacing;
                            //sideSpacing = tandemSideSpacing;
                            topSpacing = tandemTopSpacing;
                            bottomSpacing = tandemBottomSpacing;
                        }
                        else if (baseCab.DrwStyle.Contains("Accuride"))
                        {
                            dbxWidth -= accurideSideSpacing;
                            //sideSpacing = accurideSideSpacing;
                            topSpacing = accurideTopSpacing;
                            bottomSpacing = accurideBottomSpacing;
                        }
                    }

                    dbxHeight = opening1Height - topSpacing - bottomSpacing;

                    if (baseCab.IncDrwBoxInListOpening1 && baseCab.DrwCount > 0)
                    {
                        addDrawerBoxRow(baseCab, "Drawer Box 1", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (baseCab.IncDrwBoxOpening1)
                    {
                        // Build drawer box rotate-group and position it (encapsulated)
                        var dbx1rotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        Model3DGroup dbx1 = new();
                        dbx1.Children.Add(dbx1rotate);
                        ModelTransforms.ApplyTransform(dbx1, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx1);
                    }

                    //if (baseCab.IncDrwBoxInListOpening2 && baseCab.DrwCount > 1)
                    //{ ... }

                    if (baseCab.IncDrwBoxOpening2 && baseCab.DrwCount > 1)
                    {
                        dbxHeight = opening2Height - topSpacing - bottomSpacing;
                        if (baseCab.DrwCount > 2) // if true this is a middle drawer and needs additional bottom spacing
                        {
                            dbxHeight -= tandemMidDrwBottomSpacingAdjustment;
                        }

                        var dbx2rotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        Model3DGroup dbx2 = new();
                        dbx2.Children.Add(dbx2rotate);
                        ModelTransforms.ApplyTransform(dbx2, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx2);

                        if (baseCab.IncDrwBoxInListOpening2 && baseCab.DrwCount > 1)
                        {
                            //dbxHeight = opening2Height - topSpacing - bottomSpacing;
                            addDrawerBoxRow(baseCab, "Drawer Box 2", dbxHeight, dbxWidth, dbxDepth);
                        }

                    }


                    if (baseCab.IncDrwBoxOpening3 && baseCab.DrwCount > 2)
                    {
                        dbxHeight = opening3Height - topSpacing - bottomSpacing;
                        if (baseCab.DrwCount > 3) // if true this is a middle drawer and needs additional bottom spacing
                        {
                            dbxHeight -= tandemMidDrwBottomSpacingAdjustment;
                        }

                        var dbx3rotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        Model3DGroup dbx3 = new();
                        dbx3.Children.Add(dbx3rotate);
                        ModelTransforms.ApplyTransform(dbx3, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - opening2Height - MaterialThickness34 - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx3);

                        if (baseCab.IncDrwBoxInListOpening3 && baseCab.DrwCount > 2)
                        {
                            //dbxHeight = opening2Height - topSpacing - bottomSpacing;
                            addDrawerBoxRow(baseCab, "Drawer Box 3", dbxHeight, dbxWidth, dbxDepth);
                        }

                    }

                    if (baseCab.IncDrwBoxInListOpening4 && baseCab.DrwCount > 3)
                    {
                        dbxHeight = opening4Height - topSpacing - bottomSpacing;
                        addDrawerBoxRow(baseCab, "Drawer Box 4", dbxHeight, dbxWidth, dbxDepth);
                    }

                    if (baseCab.IncDrwBoxOpening4 && baseCab.DrwCount > 3)
                    {
                        dbxHeight = opening4Height - topSpacing - bottomSpacing;

                        var dbx4rotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, panelEBEdges, topDeck90);
                        Model3DGroup dbx4 = new();
                        dbx4.Children.Add(dbx4rotate);
                        ModelTransforms.ApplyTransform(dbx4, (dbxWidth / 2) - MaterialThickness34, height - dbxHeight - MaterialThickness34 - opening1Height - opening2Height - opening3Height - MaterialThickness34 - MaterialThickness34 - MaterialThickness34 - topSpacing, interiorDepth + backThickness, 0, 0, 0);
                        cabinet.Children.Add(dbx4);
                    }
                }
            }

            // Rollouts or Trash Drawer
            if (baseCab.IncRollouts || baseCab.IncRolloutsInList || baseCab.TrashDrawer)
            {
                const double rolloutMountBracketSpacing = 1.2; // Thickness of Blum Tandem Rollout Mounting Bracket - Gus uses 3/4"
                dbxHeight = rolloutHeight;

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

                if (baseCab.RolloutCount > 0)
                {
                    dbxWidth -= rolloutMountBracketSpacing * baseCab.DoorCount;
                }

                double dbxFrontAndBackWidth = dbxWidth - (MaterialThickness34 * 2);
                double dbxBottomWidth = dbxWidth - (MaterialThickness34 * 2);
                double dbxBottomLength = dbxDepth - (MaterialThickness34 * 2);

                if (baseCab.RolloutCount >= 1 || baseCab.TrashDrawer)
                {
                    if (baseCab.TrashDrawer)
                    {
                        dbxHeight = 12;
                    }

                    // Create a fresh rotate-group for each placement to avoid reusing a Model3DGroup across parents
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
                            ModelTransforms.ApplyTransform(placement, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906 + (r * 6), interiorDepth + backThickness - .25, 0, 0, 0); // set rollout .25" back from front of cabinet
                            cabinet.Children.Add(placement);
                        }
                    }

                    if (baseCab.TrashDrawer)
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
                            ModelTransforms.ApplyTransform(trashDrawer, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906, interiorDepth + backThickness, 0, 0, 0); // set trash drawer .25" back from front of cabinet
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



        // 90 deg. Corner Cabinets Style 3
        if (cabType == style3)
        {
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
                tk_Height = 0;
                tk_Depth = 0;

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
            leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ModelTransforms.ApplyTransform(leftEnd, 0, 0, 0, 0, 270, 0);
            ModelTransforms.ApplyTransform(rightEnd, -(rightDepth - MaterialThickness34) - leftFrontWidth, 0, -leftDepth - rightFrontWidth, 0, 180, 0);

            // Deck & top
            deckPoints =
                [
                    new (0,0,0),
                    new (leftFrontWidth-MaterialThickness34,0,0),
                    new (leftFrontWidth-MaterialThickness34, rightFrontWidth-MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),rightFrontWidth - MaterialThickness34,0),
                    new ((leftFrontWidth - MaterialThickness34) + rightDepth - (doubleMaterialThickness34),-leftDepth + doubleMaterialThickness34,0),
                    new (0,-leftDepth + doubleMaterialThickness34,0),
                ];
            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);
            top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);

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
            leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(leftBack, 0, tk_Height, MaterialThickness34, 0, 0, 0);

            // Right Back
            backPoints =
            [
                new (0,0,0),
                new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,0,0),
                new (leftDepth+rightFrontWidth - MaterialThickness34 - doubleMaterialThickness34,height-tk_Height,0),
                new (0,height-tk_Height,0),
            ];
            rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(rightBack, -leftDepth - rightFrontWidth + MaterialThickness34, tk_Height, leftFrontWidth + rightDepth - doubleMaterialThickness34 - .75, 0, 90, 0);


            // Toekick
            if (baseCab.HasTK)
            {
                //Debug.WriteLine("Toekick");

                toekickPoints =
                    [
                        new (0,0,0),
                        new (leftFrontWidth - MaterialThickness34 + tk_Depth,0,0),
                        new (leftFrontWidth - MaterialThickness34 + tk_Depth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick1 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(toekick1, 0, 0.5, leftDepth - tk_Depth - MaterialThickness34, 0, 0, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
                cabinet.Children.Add(toekick1);

                toekickPoints =
                    [
                        new (0,0,0),
                        new (rightFrontWidth + tk_Depth,0,0),
                        new (rightFrontWidth + tk_Depth,tk_Height-.5,0),
                        new (0,tk_Height-.5,0)
                    ];
                toekick2 = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(toekick2, -leftDepth - rightFrontWidth + MaterialThickness34, 0.5, leftFrontWidth + tk_Depth - MaterialThickness34, 0, 90, 0); // The hardcoded 1/2" here is because the actual toekick board is 1/2" narrower than the specified toekick height
                cabinet.Children.Add(toekick2);
            }


            // Shelves
            if (shelfCount > 0)
            {
                double gap = .125;

                double shelfSpacing = (height - tk_Height - doubleMaterialThickness34) / (shelfCount + 1);
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
                    shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false);
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

                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);

                    doorPoints =
                        [
                            new (0,0,0),
                            new (door2Width,0,0),
                            new (door2Width,doorHeight,0),
                            new (0,doorHeight,0)
                        ];
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);


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


        // Angle Front - style 4
        if (cabType == style4)
        {
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
                tk_Height = 0;
                tk_Depth = 0;

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

            leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);

            ModelTransforms.ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 90, 0);
            ModelTransforms.ApplyTransform(rightEnd, -leftBackWidth, 0, -rightBackWidth, 0, 0, 0);


            // Deck & top - build original polygon then normalize so edge[0] is at origin along +X
            var originalDeck = new List<Point3D>
            {
                new (leftDepth,MaterialThickness34,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - rightDepth,0),
                new (rightBackWidth - MaterialThickness34, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, leftBackWidth - MaterialThickness34 - .25,0),
                new (MaterialThickness34 + .25, MaterialThickness34,0),
            };

            // Pick p0,p1 as the "front" edge we want to align
            var p0 = originalDeck[0];
            var p1 = originalDeck[1];

            // Vector from p0->p1 and its angle
            double vx = p1.X - p0.X;
            double vy = p1.Y - p0.Y;
            double frontWidth = Math.Sqrt(vx * vx + vy * vy);
            double angle = Math.Atan2(vy, vx); // radians

            // Precompute cos/sin for -angle (rotate points so edge lies on +X)
            double ca = Math.Cos(-angle);
            double sa = Math.Sin(-angle);

            // Translate so p0 -> origin, then rotate by -angle
            deckPoints = new List<Point3D>(originalDeck.Count);
            foreach (var q in originalDeck)
            {
                double tx = q.X - p0.X;
                double ty = q.Y - p0.Y;
                double rz = q.Z - p0.Z; // keep relative Z

                double rx = tx * ca - ty * sa;
                double ry = tx * sa + ty * ca;

                deckPoints.Add(new Point3D(rx, ry, rz));
            }

            // Create deck/top from normalized polygon (edge[0] now runs from (0,0,0) -> (edgeLen,0,0))
            deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);
            top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45);

            // Apply the same world transforms as before
            ModelTransforms.ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
            ModelTransforms.ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0); //rads to degs ((angle * 180) / Math.PI) + 90
            var deckRotated = new Model3DGroup();
            var topRotated = new Model3DGroup();

            deckRotated.Children.Add(deck);
            topRotated.Children.Add(top);

            ModelTransforms.ApplyTransform(deckRotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
            ModelTransforms.ApplyTransform(topRotated, -MaterialThickness34, height - MaterialThickness34, -leftDepth, 0, 0, 0);


            // Toekick
            if (baseCab.HasTK)
            {
                toekickPoints =
                [
                    new (-tk_Depth,0,0),
                    new (frontWidth + tk_Depth,0,0),
                    new (frontWidth + tk_Depth,tk_Height-.5,0),
                    new (-tk_Depth,tk_Height-.5,0)
                ];
                toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
                ModelTransforms.ApplyTransform(toekick, 0, 0, -tk_Depth, 0, ((angle * 180) / Math.PI) + 90, 0);
                var toekickRotated = new Model3DGroup();
                toekickRotated.Children.Add(toekick);
                ModelTransforms.ApplyTransform(toekickRotated, -MaterialThickness34, .5, -leftDepth, 0, 0, 0);
                cabinet.Children.Add(toekickRotated);
            }


            // Backs

            // Left Back

            if (baseCab.HasTK)
            {
                backPoints =
                [
                    new (0,0,0),
                    new (backLegWidth,0,0),
                    new (backLegWidth,tk_Height,0),
                    new (leftBackWidth - MaterialThickness34 - .25,tk_Height,0),
                    new (leftBackWidth - MaterialThickness34 - .25,height,0),
                    new (0,height,0)
                ];

            }

            else
            {
                backPoints =
                [
                    new (0,0,0),
                    new (leftBackWidth - MaterialThickness34 - .25,0,0),
                    new (leftBackWidth - MaterialThickness34 - .25,height - tk_Height,0),
                    new (0,height - tk_Height,0)
                ];
            }

            leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

            // Right Back
            backPoints =
            [
                new (0,tk_Height,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,tk_Height,0),
                new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
                new (0,height,0)
            ];
            rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true);
            ModelTransforms.ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);


            // Shelves
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
                    shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, 45);
                    ModelTransforms.ApplyTransform(shelf, 0, gap / 2, +i * shelfSpacing, 90, 90, 180);
                    cabinet.Children.Add(shelf);
                }
            }

            // Doors
            if (baseCab.DoorCount > 0 && baseCab.IncDoors || baseCab.DoorCount > 0 && baseCab.IncDoorsInList)
            {
                var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);

                double door1Width = frontWidth - doorLeftReveal - doorRightReveal;

                double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;

                if (baseCab.DoorCount == 1)
                {
                    if (baseCab.IncDoorsInList)
                    {
                        addFrontPartRow(baseCab, "Door", doorHeight, door1Width, baseCab.DoorSpecies, baseCab.DoorGrainDir);
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
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);
                    }
                }
                if (baseCab.DoorCount == 2)
                {
                    door1Width = (frontWidth / 2) - doorLeftReveal - (baseDoorGap / 2);
                    double door2Width = (frontWidth / 2) - doorRightReveal - (baseDoorGap / 2);

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
                        door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door1, doorLeftReveal, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door1Rotated = new Model3DGroup();
                        door1Rotated.Children.Add(door1);
                        ModelTransforms.ApplyTransform(door1Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door1Rotated);

                        doorPoints =
                        [
                            new (0,0,0),
                        new (door2Width,0,0),
                        new (door2Width,doorHeight,0),
                        new (0,doorHeight,0)
                        ];
                        door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false);
                        ModelTransforms.ApplyTransform(door2, door1Width + doorLeftReveal + baseDoorGap, doorBottomReveal, 0, 0, ((angle * 180) / Math.PI) + 90, 0);
                        var door2Rotated = new Model3DGroup();
                        door2Rotated.Children.Add(door2);
                        ModelTransforms.ApplyTransform(door2Rotated, -MaterialThickness34, tk_Height, -leftDepth, 0, 0, 0);
                        cabinet.Children.Add(door2Rotated);
                    }
                }
            }

            if (!leftEndHidden) cabinet.Children.Add(leftEnd);
            if (!rightEndHidden) cabinet.Children.Add(rightEnd);
            if (!deckHidden) cabinet.Children.Add(deck);
            if (!topHidden) cabinet.Children.Add(top);
            cabinet.Children.Add(leftBack);
            cabinet.Children.Add(rightBack);
            ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
        }
    }

    // Helper: builds the drawer box rotate-group (the group that contains sides/front/back/bottom rotated as in original code)
    // Returns the rotate-group (equivalent of original dbx1rotate) ready for placement by the caller.
    private static Model3DGroup BuildDrawerBoxRotateGroup(double dbxWidth, double dbxHeight, double dbxDepth, double materialThickness, BaseCabinetModel baseCab, string panelEBEdges, bool topDeck90)
    {
        // Parts use "Prefinished Ply" / "PVC Hardrock Maple" and orientation "Horizontal"/"Vertical" as in original code.
        var dbxSidePoints = new List<Point3D>
        {
            new(dbxDepth, dbxHeight, 0),
            new(0, dbxHeight, 0),
            new(0, 0, 0),
            new(dbxDepth, 0, 0)
        };

        var dbxFrontAndBackPoints = new List<Point3D>
        {
            new(dbxWidth - (materialThickness * 2), dbxHeight, 0),
            new(0, dbxHeight, 0),
            new(0, 0, 0),
            new(dbxWidth - (materialThickness * 2), 0, 0)
        };

        var dbxBottomPoints = new List<Point3D>
        {
            new(0, 0, 0),
            new(dbxWidth - (materialThickness * 2), 0, 0),
            new(dbxWidth - (materialThickness * 2), dbxDepth - (materialThickness * 2), 0),
            new(0, dbxDepth - (materialThickness * 2), 0)
        };

        var leftSide = CabinetPartFactory.CreatePanel(dbxSidePoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, true, panelEBEdges, isFaceUp: true);
        var rightSide = CabinetPartFactory.CreatePanel(dbxSidePoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, true, panelEBEdges, isFaceUp: true);
        var front = CabinetPartFactory.CreatePanel(dbxFrontAndBackPoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, true, panelEBEdges, isFaceUp: true);
        var back = CabinetPartFactory.CreatePanel(dbxFrontAndBackPoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, true, panelEBEdges, isFaceUp: true);
        var bottom = CabinetPartFactory.CreatePanel(dbxBottomPoints, materialThickness, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, true, panelEBEdges, isFaceUp: false);

        // Apply local transforms to assemble the box (same as original)
        ModelTransforms.ApplyTransform(leftSide, 0, 0, -(dbxWidth - materialThickness), 0, 0, 0);
        ModelTransforms.ApplyTransform(front, 0, 0, 0, 0, 90, 0);
        ModelTransforms.ApplyTransform(back, 0, 0, dbxDepth - materialThickness, 0, 90, 0);
        ModelTransforms.ApplyTransform(bottom, 0, materialThickness, -materialThickness - .5, 90, 90, 0);

        var rotateGroup = new Model3DGroup();
        rotateGroup.Children.Add(leftSide);
        rotateGroup.Children.Add(rightSide);
        rotateGroup.Children.Add(front);
        rotateGroup.Children.Add(back);
        rotateGroup.Children.Add(bottom);

        // Rotate to match original orientation (dbx1rotate had a rotation applied)
        ModelTransforms.ApplyTransform(rotateGroup, 0, 0, 0, 0, 90, 0);

        return rotateGroup;
    }

    // Small helper to apply transform and add to parent
    // keeps transform-and-add pattern in one place
    private static void ApplyTransformAndAdd(Model3DGroup parent, Model3DGroup child, double tx, double ty, double tz, double rx, double ry, double rz)
    {
        ModelTransforms.ApplyTransform(child, tx, ty, tz, rx, ry, rz);
        parent.Children.Add(child);
    }
}


