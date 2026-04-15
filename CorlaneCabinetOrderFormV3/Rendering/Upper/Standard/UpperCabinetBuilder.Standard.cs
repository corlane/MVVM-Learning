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
        bool doorsHidden,
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
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;

        BuildEndPanels(upperCab, MaterialThickness34, height, depth, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> endPanelPoints);

        // ----------------------------
        // HOLES
        // IMPORTANT: add holes before ApplyTransform(leftEnd/rightEnd, ...)
        // ----------------------------
        AddHoles(upperCab, MaterialThickness34, StretcherWidth, height, depth, backThickness, holeDiameter, holeDepth, leftEnd, rightEnd);

        // End panel transforms
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);

        BuildDeckAndTop(upperCab, MaterialThickness34, depth, backThickness, interiorWidth, backInsetForDeckAndTop, out Model3DGroup deck, out List<Point3D> deckPoints, out Model3DGroup top, out List<Point3D> topPoints, height);

        BuildBack(cabinet, upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, MaterialThickness14, StretcherWidth, width, height, backThickness, interiorWidth, interiorHeight, out Model3DGroup back, out Model3DGroup? nailer, out List<Point3D> backPoints, out List<Point3D>? nailerPoints);

        BuildShelves(cabinet, upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, backThickness, interiorWidth, interiorHeight, shelfDepth, out Model3DGroup? shelf, out List<Point3D>? shelfPoints);

        BuildDoors(cabinet, upperCab, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, MaterialThickness34, doorEdgebandingSpecies, width, height, depth, upperDoorGap, doorLeftReveal, doorRightReveal, doorTopReveal, doorBottomReveal, doorSideReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(back);
    }
}