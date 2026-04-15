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
        double shelfDepth = dim.ShelfDepth;
        double backThickness = MaterialThickness34; // corner cabinets always have 3/4" backs
        int shelfCount = baseCab.ShelfCount;
        double interiorHeight = dim.InteriorHeight;
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;
        double insideCornerRadius = 1.0; // adjust to taste
        int arcSegments = 8;

        BuildEndPanels(baseCab, MaterialThickness34, height, leftDepth, rightDepth, tk_Height, tk_Depth, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints);

        AddHoles(baseCab, MaterialThickness34, height, leftDepth, rightDepth, tk_Height, backThickness, holeDiameter, holeDepth, leftEnd, rightEnd);

        BuildDeckAndTop(baseCab, MaterialThickness34, doubleMaterialThickness34, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, tk_Height, insideCornerRadius, arcSegments, leftEnd, rightEnd, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints);

        BuildBacks(baseCab, MaterialThickness34, doubleMaterialThickness34, backLegWidth, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, tk_Height, holeDiameter, holeDepth, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints);

        BuildToekick(cabinet, baseCab, MaterialThickness34, leftFrontWidth, rightFrontWidth, leftDepth, tk_Height, tk_Depth, out Model3DGroup toekick1, out Model3DGroup toekick2, out List<Point3D> toekickPoints);

        BuildShelves(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, doubleMaterialThickness34, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, tk_Height, shelfCount, interiorHeight, insideCornerRadius, arcSegments, out Model3DGroup shelf, out List<Point3D> shelfPoints);

        BuildDoors(cabinet, baseCab, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, MaterialThickness34, doubleMaterialThickness34, doorEdgebandingSpecies, height, leftFrontWidth, rightFrontWidth, leftDepth, tk_Height, doorLeftReveal, doorRightReveal, doorTopReveal, doorBottomReveal, out Model3DGroup door1, out Model3DGroup door2, out List<Point3D> doorPoints);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);
        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
    }
}

