using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
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
        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";
        Model3DGroup leftEnd;
        Model3DGroup rightEnd;
        Model3DGroup deck;
        Model3DGroup top = new();
        Model3DGroup shelf;
        Model3DGroup toekick = new();
        Model3DGroup back;
        List<Point3D> endPanelPoints;


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

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, isFaceUp: true, CabinetPartKind.LeftEnd);

        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, isFaceUp: true, CabinetPartKind.RightEnd);

        // ----------------------------
        // HOLES (base cabinets)
        // IMPORTANT: add holes before ApplyTransform(leftEnd/rightEnd, ...)
        // ----------------------------
        DrillEndPanelHoles(leftEnd, rightEnd, baseCab, dim);

        // End panel transforms
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, interiorWidth / 2, 0, 270, 0);
        ModelTransforms.ApplyTransform(rightEnd, 0, 0, -(interiorWidth / 2) - (MaterialThickness34), 0, 270, 0);

        deck = BuildDeck(baseCab, MaterialThickness34, depth, backThickness, tk_Height, interiorWidth, deckBackInset, topDeck90, isPanel, panelEBEdges);

        top = BuildTop(baseCab, MaterialThickness34, StretcherWidth, topStretcherBackWidth, width, height, depth, interiorWidth, topDeck90, isPanel, panelEBEdges, top, out Model3DGroup? topStretcherFront, out Model3DGroup? topStretcherBack);

        toekick = BuildToekick(baseCab, MaterialThickness34, depth, tk_Height, tk_Depth, interiorWidth, topDeck90, isPanel, panelEBEdges, toekick);

        back = BuildBack(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, MaterialThickness14, StretcherWidth, width, height, backThickness, tk_Height, interiorWidth, interiorHeight, topDeck90, isPanel, panelEBEdges);

        // Drawer Stretchers
        BuildDrawerStretchers(cabinet, baseCab, dim);

        shelf = BuildShelves(cabinet, baseCab, getMatchingEdgebandingSpecies, MaterialThickness34, cabType, style2, backThickness, tk_Height, interiorWidth, interiorHeight, shelfDepth, opening1Height, topDeck90, isPanel, panelEBEdges);

        // Doors
        if (baseCab.DoorCount > 0 && baseCab.IncDoors && cabType != style2 || baseCab.DoorCount > 0 && baseCab.IncDoorsInList && cabType != style2)
        {
            BuildDoors(cabinet, baseCab, dim, opening1Height, doorEdgebandingSpecies, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow);
        }

        // Drawer Fronts
        BuildDrawerFronts(cabinet, baseCab, dim, doorEdgebandingSpecies, doorsHidden, resolveDoorSpeciesForTotals, addFrontPartRow, result);

        // Drawer Boxes
        BuildDrawerBoxes(cabinet, baseCab, dim, addDrawerBoxRow, result);

        // Rollouts or Trash Drawer
        BuildRolloutsAndTrash(cabinet, baseCab, dim, addDrawerBoxRow, result);

        if (!leftEndHidden) cabinet.Children.Add(leftEnd);
        if (!rightEndHidden) cabinet.Children.Add(rightEnd);
        if (!deckHidden) cabinet.Children.Add(deck);
        if (!topHidden) cabinet.Children.Add(top);
        cabinet.Children.Add(back);
        cabinet.Children.Add(toekick);
    }
}