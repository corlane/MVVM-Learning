using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildStandardOrDrawer(
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
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow,
        CabinetBuildResult? result = null)

    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double MaterialThickness14 = MaterialDefaults.Thickness14;
        string? cabType = baseCab.Style;
        string style2 = CabinetStyles.Base.Drawer;
        string doorEdgebandingSpecies = CabinetBuildHelpers.GetDoorEdgebandingSpecies(baseCab.DoorSpecies);
        double StretcherWidth = 6;
        double topStretcherBackWidth = 3;
        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double backThickness = dim.BackThickness;
        double tk_Height = dim.TKHeight;
        double tk_Depth = dim.TKDepth;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double interiorHeight = dim.InteriorHeight;
        double shelfDepth = dim.ShelfDepth;
        double opening1Height = dim.Opening1Height;
        double deckBackInset = 0;
        Model3DGroup leftEnd = new();
        Model3DGroup rightEnd = new();
        Model3DGroup deck = new();
        Model3DGroup top = new();
        Model3DGroup shelf = new();
        Model3DGroup toekick = new();
        Model3DGroup back = new();
        List<Point3D> endPanelPoints;

        if (doorEdgebandingSpecies.Contains("Custom", StringComparison.OrdinalIgnoreCase))
        {
            doorEdgebandingSpecies = baseCab.CustomDoorSpecies;
        }

        // ── Capture core dimensions ──
        if (result is not null)
        {
            result.InteriorWidth = interiorWidth;
            result.InteriorDepth = interiorDepth;
            result.InteriorHeight = interiorHeight;
            result.ShelfDepth = shelfDepth;
            result.DrawerBoxDepth = dim.DrawerBoxDepth;
        }

        endPanelPoints = BuildEndPanels(baseCab, height, depth, tk_Height, tk_Depth);

        if (baseCab.HasLeftEnd)
        {
            leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, isFaceUp: true, CabinetPartKind.LeftEnd);
        }

        if (baseCab.HasRightEnd)
        {
            rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, isFaceUp: true, CabinetPartKind.RightEnd);
        }

        // ----------------------------
        // HOLES (base cabinets)
        // IMPORTANT: add holes before ApplyTransform(leftEnd/rightEnd, ...)
        // ----------------------------
        DrillEndPanelHoles(leftEnd, rightEnd, baseCab, dim);

        // End panel transforms
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);

        if (baseCab.HasDeck)
        {
            deck = BuildDeck(baseCab, MaterialThickness34, depth, backThickness, tk_Height, interiorWidth, deckBackInset, baseCab.HasLeftEnd, baseCab.HasRightEnd);
        }

        if (baseCab.HasTop)
        {
            top = BuildTop(baseCab, MaterialThickness34, StretcherWidth, topStretcherBackWidth, width, height, depth, interiorWidth, top, out Model3DGroup? topStretcherFront, out Model3DGroup? topStretcherBack, baseCab.HasLeftEnd, baseCab.HasRightEnd);
        }

        if (baseCab.HasToeKickBoard)
        {
            toekick = BuildToekick(baseCab, MaterialThickness34, depth, tk_Height, tk_Depth, interiorWidth, toekick, baseCab.HasLeftEnd, baseCab.HasRightEnd);
        }

        back = BuildBack(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, MaterialThickness14, StretcherWidth, width, height, backThickness, tk_Height, interiorWidth, interiorHeight, baseCab.HasLeftEnd, baseCab.HasRightEnd);

        // Drawer Stretchers
        BuildDrawerStretchers(cabinet, baseCab, dim, baseCab.HasLeftEnd, baseCab.HasRightEnd);

        shelf = BuildShelves(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, cabType, style2, backThickness, tk_Height, interiorWidth, interiorHeight, shelfDepth, opening1Height, baseCab.HasLeftEnd, baseCab.HasRightEnd);

        if (baseCab.DoorCount > 0 && cabType != style2 || baseCab.DoorCount > 0 && baseCab.IncDoorsInList && cabType != style2) // REMOVED baseCab.IncDoors so they will show up still, but be a different color indicating that we are not supplying them.  This is because some users (CANNOT SET THE DOOR COUNT PROPERLY) want to see the doors in the 3D model even if they are not being supplied by us, and it is less confusing to have them show up as a different color than to have them not show up at all.
        {
            BuildDoors(cabinet, baseCab, dim, opening1Height, doorEdgebandingSpecies, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, result);
        }

        // Drawer Fronts
        BuildDrawerFronts(cabinet, baseCab, dim, doorEdgebandingSpecies, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, result);

        // Drawer Boxes
        BuildDrawerBoxes(cabinet, baseCab, dim, addDrawerBoxRow, result, baseCab.HasLeftEnd, baseCab.HasRightEnd);

        // Rollouts or Trash Drawer
        BuildRolloutsAndTrash(cabinet, baseCab, dim, addDrawerBoxRow, result, baseCab.HasLeftEnd, baseCab.HasRightEnd);

        if (baseCab.HasLeftEnd && !leftEndHidden) cabinet.Children.Add(leftEnd);
        if (baseCab.HasRightEnd && !rightEndHidden) cabinet.Children.Add(rightEnd);
        if (baseCab.HasDeck && !deckHidden) cabinet.Children.Add(deck);
        if (baseCab.HasTop && !topHidden) cabinet.Children.Add(top);
        if (baseCab.HasBack) cabinet.Children.Add(back);
        if (baseCab.HasToeKickBoard) cabinet.Children.Add(toekick);
    }
}