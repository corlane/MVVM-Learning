using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Builds and positions 1 or 2 doors, adding them to <paramref name="cabinet"/>
    /// and optionally recording them via <paramref name="addFrontPartRow"/>.
    /// </summary>
    private static void BuildDoors(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double opening1Height,
        string doorEdgebandingSpecies,
        bool doorsHidden,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double halfMaterialThickness34 = MaterialThickness34 / 2;
        string style1 = CabinetStyles.Base.Standard;
        string? cabType = baseCab.Style;
        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double tk_Height = dim.TKHeight;
        double baseDoorGap = dim.BaseDoorGap;
        double doorLeftReveal = dim.DoorLeftReveal;
        double doorRightReveal = dim.DoorRightReveal;
        double doorTopReveal = dim.DoorTopReveal;
        double doorBottomReveal = dim.DoorBottomReveal;
        double doorSideReveal = dim.DoorSideReveal;
        var doorSpeciesForTotals = resolveDoorSpeciesForTotals(baseCab.DoorSpecies, baseCab.CustomDoorSpecies);
        double doorWidth = width - (doorSideReveal * 2);
        double doorHeight = height - doorTopReveal - doorBottomReveal - tk_Height;
        bool edgeBandingOnDoorsAndDrawerFronts = baseCab.EdgebandDoorsAndDrawers;

        if (!edgeBandingOnDoorsAndDrawerFronts)
        {
            doorEdgebandingSpecies = "None";
        }

        if (cabType == style1 && baseCab.DrwCount == 1)
        {
            doorHeight = height - opening1Height - MaterialThickness34 - halfMaterialThickness34 - (baseDoorGap / 2) - doorBottomReveal - tk_Height;
        }

        if (baseCab.DoorCount == 1)
        {
            List<Point3D> doorPoints =
            [
                new (0,0,0),
                new (doorWidth,0,0),
                new (doorWidth,doorHeight,0),
                new (0,doorHeight,0)
            ];

            if (baseCab.IncDoorsInList)
            {
                addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
            }

            if (baseCab.IncDoors)
            {
                var door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);
                if (!baseCab.HasTK)
                {
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                }
                else
                {
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                }
                if (!doorsHidden) cabinet.Children.Add(door1);
            }
        }

        if (baseCab.DoorCount == 2)
        {
            doorWidth = (doorWidth / 2) - (baseDoorGap / 2);

            List<Point3D> doorPoints =
            [
                new (0,0,0),
                new (doorWidth,0,0),
                new (doorWidth, doorHeight, 0),
                new (0,doorHeight,0)
            ];

            if (baseCab.IncDoorsInList)
            {
                addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
                addFrontPartRow(baseCab, "Door", doorHeight, doorWidth, baseCab.DoorSpecies, baseCab.DoorGrainDir);
            }

            if (baseCab.IncDoors)
            {
                var door1 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);

                var door2 = CabinetPartFactory.CreatePanel(doorPoints, MaterialThickness34, doorSpeciesForTotals, doorEdgebandingSpecies, baseCab.DoorGrainDir, baseCab, isFaceUp: false, CabinetPartKind.Door);
                if (!baseCab.HasTK)
                {
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal, depth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal, depth, 0, 0, 0);
                }
                else
                {
                    ModelTransforms.ApplyTransform(door1, -(width / 2) + doorLeftReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                    ModelTransforms.ApplyTransform(door2, (width / 2) - doorWidth - doorRightReveal, doorBottomReveal + tk_Height, depth, 0, 0, 0);
                }
                if (!doorsHidden)
                {
                    cabinet.Children.Add(door1);
                    cabinet.Children.Add(door2);
                }
            }
        }
    }

}
