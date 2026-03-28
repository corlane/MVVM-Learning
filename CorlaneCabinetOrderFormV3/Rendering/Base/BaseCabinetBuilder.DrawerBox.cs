using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static Model3DGroup BuildDrawerBoxRotateGroup(double dbxWidth, double dbxHeight, double dbxDepth, double materialThickness, BaseCabinetModel baseCab, string panelEBEdges, bool topDeck90)
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

    private static void ApplyTransformAndAdd(Model3DGroup parent, Model3DGroup child, double tx, double ty, double tz, double rx, double ry, double rz)
    {
        ModelTransforms.ApplyTransform(child, tx, ty, tz, rx, ry, rz);
        parent.Children.Add(child);
    }
}