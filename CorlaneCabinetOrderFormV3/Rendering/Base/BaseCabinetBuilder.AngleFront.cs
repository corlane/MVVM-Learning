using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildAngleFront(
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
        double leftDepth = dim.LeftDepth;
        double rightDepth = dim.RightDepth;
        double leftBackWidth = dim.LeftBackWidth;
        double rightBackWidth = dim.RightBackWidth;
        double tk_Height = dim.TKHeight;
        double tk_Depth = dim.TKDepth;
        double baseDoorGap = dim.BaseDoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double interiorHeight = dim.InteriorHeight;

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
        Model3DGroup toekick;
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

        var p0 = originalDeck[0];
        var p1 = originalDeck[1];

        double vx = p1.X - p0.X;
        double vy = p1.Y - p0.Y;
        double frontWidth = Math.Sqrt(vx * vx + vy * vy);
        double angle = Math.Atan2(vy, vx);

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

        deck = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45, partKind: CabinetPartKind.Deck);
        top = CabinetPartFactory.CreatePanel(deckPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, isPanel, panelEBEdges, isFaceUp: false, ((angle * 180) / Math.PI) - 45, partKind: CabinetPartKind.Top);

        ModelTransforms.ApplyTransform(top, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
        ModelTransforms.ApplyTransform(deck, 0, 0, 0, -90, ((angle * 180) / Math.PI) + 90, 0);
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
            toekick = CabinetPartFactory.CreatePanel(toekickPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.Toekick);
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

        leftBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(leftBack, -leftBackWidth + .25, 0, -MaterialThickness34 - .25, 0, 0, 0);

        // Right Back
        backPoints =
        [
            new (0,tk_Height,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,tk_Height,0),
            new (rightBackWidth - doubleMaterialThickness34 - .25,height,0),
            new (0,height,0)
        ];
        rightBack = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, baseCab.Species, "None", "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.BackBase34);
        ModelTransforms.ApplyTransform(rightBack, MaterialThickness34 + .25, 0, -leftBackWidth + .25, 0, 90, 0);


        // Shelves
        if (shelfCount > 0)
        {
            double gap = .125;

            double shelfSpacing = interiorHeight + MaterialThickness34 + MaterialThickness34;
            if (baseCab.HasTK) { shelfSpacing += tk_Height * 2; }
            shelfSpacing /= (baseCab.ShelfCount + 1);
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
                shelf = CabinetPartFactory.CreatePanel(shelfPoints, MaterialThickness34, baseCab.Species, getMatchingEdgebandingSpecies(baseCab.Species), "Horizontal", baseCab, true, isPanel, panelEBEdges, isFaceUp: false, 45, partKind: CabinetPartKind.Shelf);
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
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
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
                    door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
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
                    door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, topDeck90, true, "TBLR", isFaceUp: false, partKind: CabinetPartKind.Door);
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
        if (!deckHidden) cabinet.Children.Add(deckRotated);
        if (!topHidden) cabinet.Children.Add(topRotated);
        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);
        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
    }
}