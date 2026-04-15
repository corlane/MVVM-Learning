using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildAngleFront(
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
        double leftDepth = dim.LeftDepth;
        double rightDepth = dim.RightDepth;
        double leftBackWidth = dim.LeftBackWidth;
        double rightBackWidth = dim.RightBackWidth;
        double upperDoorGap = dim.DoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double backThickness = MaterialThickness34;
        int shelfCount = upperCab.ShelfCount;
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        BuildEndPanels(upperCab, MaterialThickness34, height, leftDepth, rightDepth, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints);

        AddHoles(upperCab, MaterialThickness34, height, leftDepth, rightDepth, leftBackWidth, rightBackWidth, backThickness, leftEnd, rightEnd, holeDiameter, holeDepth);

        BuildDeckAndTop(upperCab, MaterialThickness34, height, leftDepth, rightDepth, leftBackWidth, rightBackWidth, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints, out double frontWidth, out double angle, out Model3DGroup deckRotated, out Model3DGroup topRotated);

        BuildBacks(upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, doubleMaterialThickness34, height, leftBackWidth, rightBackWidth, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints);

        BuildShelves(cabinet, upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, doubleMaterialThickness34, height, leftDepth, rightDepth, leftBackWidth, rightBackWidth, shelfCount, out Model3DGroup? shelf, out List<Point3D>? shelfPoints);

        BuildDoors(cabinet, upperCab, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, MaterialThickness34, doorEdgebandingSpecies, height, leftDepth, upperDoorGap, doorLeftReveal, doorRightReveal, doorTopReveal, doorBottomReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, frontWidth, angle);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deckRotated);
        if (!topHidden) cabinet.Children.Add(topRotated);

        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);

        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
    }
}