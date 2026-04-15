using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildCorner90(
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
        double leftFrontWidth = dim.LeftFrontWidth;
        double rightFrontWidth = dim.RightFrontWidth;
        double leftDepth = dim.LeftDepth;
        double rightDepth = dim.RightDepth;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double backThickness = MaterialThickness34;
        int shelfCount = upperCab.ShelfCount;
        double holeDiameter = 0.197;
        double holeDepth = MaterialThickness34 / 2;
        double gap = .125;
        double insideCornerRadius = 1.0;
        int arcSegments = 8;

        BuildEndPanels90(upperCab, MaterialThickness34, height, leftDepth, rightDepth, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints);

        AddHoles(upperCab, MaterialThickness34, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, backThickness, holeDiameter, holeDepth, leftEnd, rightEnd);

        static List<Point3D> GenerateInsideCornerArc(double cornerX, double cornerY, double radius, int segments)
        {
            double cx = cornerX - radius;
            double cy = cornerY + radius;
            var pts = new List<Point3D>(segments + 1);
            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;
                double angle = -(Math.PI / 2.0) + (t * Math.PI / 2.0); // -90° → 0°
                pts.Add(new Point3D(
                    cx + radius * Math.Cos(angle),
                    cy + radius * Math.Sin(angle),
                    0));
            }
            return pts;
        }

        var deckCornerArc = GenerateInsideCornerArc(leftFrontWidth - MaterialThickness34, 0, insideCornerRadius, arcSegments);

        BuildDeckAndTop(upperCab, MaterialThickness34, doubleMaterialThickness34, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, out Model3DGroup deck, out Model3DGroup top, out List<Point3D> deckPoints, deckCornerArc);

        BuildBacks(upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, doubleMaterialThickness34, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, out Model3DGroup leftBack, out Model3DGroup rightBack, out List<Point3D> backPoints);

        var shelfCornerArc = GenerateInsideCornerArc(leftFrontWidth - MaterialThickness34 - gap, 0, insideCornerRadius, arcSegments);

        BuildShelves(cabinet, upperCab, getMatchingEdgebandingSpecies, MaterialThickness34, doubleMaterialThickness34, height, leftFrontWidth, rightFrontWidth, leftDepth, rightDepth, shelfCount, gap, out Model3DGroup? shelf, out List<Point3D>? shelfPoints, shelfCornerArc);

        double cornerCabDoorOpenSideReveal = 0.875;

        BuildDoors(cabinet, upperCab, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, MaterialThickness34, doubleMaterialThickness34, doorEdgebandingSpecies, height, leftFrontWidth, rightFrontWidth, leftDepth, doorLeftReveal, doorRightReveal, doorTopReveal, doorBottomReveal, out Model3DGroup? door1, out Model3DGroup? door2, out List<Point3D>? doorPoints, cornerCabDoorOpenSideReveal);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(leftBack);
        cabinet.Children.Add(rightBack);
        ModelTransforms.ApplyTransform(cabinet, 0, 0, 0, 0, 45, 0);
    }

}