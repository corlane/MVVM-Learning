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
        double backThickness = MaterialThickness34; // corner cabinets always have 3/4" backs
        int shelfCount = baseCab.ShelfCount;
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        BuildEndPanelsAngleFront(baseCab, MaterialThickness34, height, leftDepth, rightDepth, tk_Height, tk_Depth, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints);

        AddHoles(baseCab, MaterialThickness34, height, leftDepth, rightDepth, leftBackWidth, rightBackWidth, tk_Height, backThickness, holeDiameter, holeDepth, leftEnd, rightEnd);

        BuildDeckAndTop(baseCab, MaterialThickness34, height, leftDepth, rightDepth, leftBackWidth, rightBackWidth, tk_Height, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints, out double frontWidth, out double angle, out Model3DGroup deckRotated, out Model3DGroup topRotated);

        BuildToekick(cabinet, baseCab, MaterialThickness34, leftDepth, tk_Height, tk_Depth, out Model3DGroup? toekick, out List<Point3D>? toekickPoints, frontWidth, angle);

        BuildBacks(baseCab, MaterialThickness34, doubleMaterialThickness34, backLegWidth, height, leftBackWidth, rightBackWidth, tk_Height, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints);

        BuildShelves(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, leftDepth, rightDepth, leftBackWidth, rightBackWidth, tk_Height, interiorHeight, shelfCount, out Model3DGroup? shelf, out List<Point3D>? shelfPoints);

        BuildDoors(cabinet, baseCab, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, MaterialThickness34, doorEdgebandingSpecies, height, leftDepth, tk_Height, baseDoorGap, doorLeftReveal, doorRightReveal, doorTopReveal, doorBottomReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, frontWidth, angle);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deckRotated);
        if (!topHidden) cabinet.Children.Add(topRotated);
        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);
        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, -135, 0);
    }
}