using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Builds and positions drawer boxes for each opening, adding them to
    /// <paramref name="cabinet"/> and optionally recording them via <paramref name="addDrawerBoxRow"/>.
    /// </summary>
    private static void BuildDrawerBoxes(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow,
        CabinetBuildResult? result)
    {
        if (baseCab.DrwCount <= 0) return;

        if (!baseCab.IncDrwBoxOpening1 && !baseCab.IncDrwBoxOpening2 && !baseCab.IncDrwBoxOpening3 && !baseCab.IncDrwBoxOpening4
            && !baseCab.IncDrwBoxInListOpening1 && !baseCab.IncDrwBoxInListOpening2 && !baseCab.IncDrwBoxInListOpening3 && !baseCab.IncDrwBoxInListOpening4)
            return;

        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double tandemSideSpacing = .4;
        double tandemTopSpacing = .375;
        double tandemBottomSpacing = .5906;
        double tandemMidDrwBottomSpacingAdjustment = 0;
        double accurideSideSpacing = 1;
        double accurideTopSpacing = .5;
        double accurideBottomSpacing = .5;

        double height = dim.Height;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double backThickness = dim.BackThickness;
        double dbxWidth = interiorWidth;
        double dbxDepth = dim.DrawerBoxDepth;

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

        double[] openingHeights = new[] { dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height };
        bool[] incBoxOpening = new[] { baseCab.IncDrwBoxOpening1, baseCab.IncDrwBoxOpening2, baseCab.IncDrwBoxOpening3, baseCab.IncDrwBoxOpening4 };
        bool[] incBoxInList = new[] { baseCab.IncDrwBoxInListOpening1, baseCab.IncDrwBoxInListOpening2, baseCab.IncDrwBoxInListOpening3, baseCab.IncDrwBoxInListOpening4 };

        for (int oi = 0; oi < 4; oi++)
        {
            int openingIndex = oi + 1;
            if (baseCab.DrwCount < openingIndex) break;

            double dbxHeight = openingHeights[oi] - topSpacing - bottomSpacing;

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

            var dbxRotate = BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, "", false);
            Model3DGroup dbxGroup = new();
            dbxGroup.Children.Add(dbxRotate);

            double prevOpeningsSum = 0;
            for (int p = 0; p < oi; p++) prevOpeningsSum += openingHeights[p];

            double y = height - dbxHeight - MaterialThickness34 - topSpacing - prevOpeningsSum - (MaterialThickness34 * oi);

            ModelTransforms.ApplyTransform(dbxGroup, (dbxWidth / 2) - MaterialThickness34, y, interiorDepth + backThickness, 0, 0, 0);
            cabinet.Children.Add(dbxGroup);
        }
    }

    private static Model3DGroup BuildDrawerBoxRotateGroup(
        double dbxWidth,
        double dbxHeight, 
        double dbxDepth,
        double materialThickness,
        BaseCabinetModel baseCab,
        string panelEBEdges,
        bool topDeck90)
    {
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

        var leftSide = CabinetPartFactory.CreatePanel(dbxSidePoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, false, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.DrawerBoxSide);
        var rightSide = CabinetPartFactory.CreatePanel(dbxSidePoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, false, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.DrawerBoxSide);
        var front = CabinetPartFactory.CreatePanel(dbxFrontAndBackPoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, false, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.DrawerBoxFront);
        var back = CabinetPartFactory.CreatePanel(dbxFrontAndBackPoints, materialThickness, "Prefinished Ply", "PVC Hardrock Maple", "Horizontal", baseCab, topDeck90, false, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.DrawerBoxBack);
        var bottom = CabinetPartFactory.CreatePanel(dbxBottomPoints, materialThickness, "Prefinished Ply", "None", "Vertical", baseCab, topDeck90, true, panelEBEdges, isFaceUp: false, partKind: CabinetPartKind.DrawerBoxBottom);

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

        ModelTransforms.ApplyTransform(rotateGroup, 0, 0, 0, 0, 90, 0);

        return rotateGroup;
    }
}