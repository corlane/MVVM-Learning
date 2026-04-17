using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Builds and positions drawer front panels, adding them to <paramref name="cabinet"/>
    /// and optionally recording them via <paramref name="addFrontPartRow"/>.
    /// </summary>
    private static void BuildDrawerFronts(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        string doorEdgebandingSpecies,
        bool doorsHidden,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        CabinetBuildResult? result)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorSideReveal = dim.DoorSideReveal;
        double baseDoorGap = dim.BaseDoorGap;
        double drwFrontWidth = width - (doorSideReveal * 2);
        if (result is not null) result.DrawerFrontWidth = drwFrontWidth;
        var doorSpeciesForTotalsForDrw = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);
        double[] drwHeights = new[] { dim.DrwFront1Height, dim.DrwFront2Height, dim.DrwFront3Height, dim.DrwFront4Height };
        bool[] incFront = new[] { baseCab.IncDrwFront1, baseCab.IncDrwFront2, baseCab.IncDrwFront3, baseCab.IncDrwFront4 };
        bool[] incFrontInList = new[] { baseCab.IncDrwFrontInList1, baseCab.IncDrwFrontInList2, baseCab.IncDrwFrontInList3, baseCab.IncDrwFrontInList4 };
        int maxFronts = Math.Min(4, Math.Max(0, baseCab.DrwCount));
        bool edgeBandingOnDoorsAndDrawerFronts = baseCab.EdgebandDoorsAndDrawers;

        if (!edgeBandingOnDoorsAndDrawerFronts)
        {
            doorEdgebandingSpecies = "None";
        }


        if (result is not null)
        {
            for (int i = 0; i < maxFronts; i++)
                result.DrawerFrontHeights.Add(drwHeights[i]);
        }

        for (int fi = 0; fi < maxFronts; fi++)
        {
            double h = drwHeights[fi];

            if (incFrontInList[fi])
            {
                addFrontPartRow(baseCab, $"Drawer Front {fi + 1}", h, drwFrontWidth, baseCab.DoorSpecies, baseCab.DrwFrontGrainDir);
            }

            if (!incFront[fi]) continue;

            List<Point3D> drwFrontPoints =
            [
                new (0,0,0),
                new (drwFrontWidth,0,0),
                new (drwFrontWidth,h,0),
                new (0,h,0)
            ];

            double cumulativeHeight = 0;
            for (int k = 0; k <= fi; k++) cumulativeHeight += drwHeights[k];

            double yPos = height - doorTopReveal - cumulativeHeight - (fi * baseDoorGap);

            var front = CabinetPartFactory.CreatePanel(drwFrontPoints, MaterialThickness34, doorSpeciesForTotalsForDrw, doorEdgebandingSpecies, baseCab.DrwFrontGrainDir, baseCab, isFaceUp: false, CabinetPartKind.DrawerFront);
            ModelTransforms.ApplyTransform(front, -(width / 2) + doorLeftReveal, yPos, depth, 0, 0, 0);
            if (!doorsHidden) cabinet.Children.Add(front);
        }
    }

}
