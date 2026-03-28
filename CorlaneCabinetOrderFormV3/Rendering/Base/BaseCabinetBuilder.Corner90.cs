using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

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

        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";
        int shelfCount = baseCab.ShelfCount;

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
        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);

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
}