using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Orchestrates the assembly of final geometry for a part by delegating to specialized calculators.
/// 
/// Coordinates Outlines, Tenon Thinning Pockets, Mortise Pockets, Assembly Holes & Shelf Holes
/// based on the PartInfo and JoineryConfig.
/// </summary>
internal static class PanelGeometryCalculator
{
    internal static PartGeometry Compute(PartInfo part, JoineryConfig joinery, double materialThickness34)
    {
        var baseCab = part.CabinetModel as BaseCabinetModel;
        bool isCorner90 = baseCab != null && baseCab.Style == CabinetStyles.Base.Corner90;
        bool isLShape = isCorner90 && (part.Name.Contains("Top") || part.Name.Contains("Deck") || part.Name.Contains("Shelf"));
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePocketsThru = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();
        var holesThru = new List<(double x, double y, double radius)>();

        // 1. Build Outline
        OutlineBuilder.BuildOutline(part, isEndPanelWithTk, isLShape, materialThickness34, outline);

        // 2. Compute Joinery
        if (isLShape && part.CabinetModel is BaseCabinetModel)
        {
            LShapeJoineryCalculator.ComputeLShapeJoinery(part, outline, thinningPockets, joinery, baseCab!, materialThickness34);
            if (baseCab!.HasTK && part.Name.Contains("Deck"))
            {
                MortiseBlindCalculator.ComputeMortisePockets(part, mortisePockets, joinery, materialThickness34);
            }
        }
        else if (!isLShape)
        {
            TenonAndThinningCalculator.ComputeTenonsAndThinningPockets(part, outline, thinningPockets, joinery, materialThickness34);
            MortiseBlindCalculator.ComputeMortisePockets(part, mortisePockets, joinery, materialThickness34);
            MortiseThruCalculator.ComputeMortisePocketsThru(part, mortisePocketsThru, joinery, materialThickness34);
            ScrewHoleEdgeCalculator.ComputeScrewHoles(part, holesThru, joinery, materialThickness34);
            ShelfHoleEdgeCalculator.ComputeShelfHoles(part, holes, joinery, materialThickness34);

            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                DrawerSlideEdgeCalculator.ComputeDrawerSlideHoles(baseCab!, holes, joinery, materialThickness34);
            }

            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel baseCabHinge && baseCabHinge.DrillHingeHoles && baseCabHinge.Style != CabinetStyles.Base.Drawer)
            {
                var baseDim = BaseCabinetDimensions.From(baseCabHinge);
                double panelDepth = HingeHoleEdgeCalculator.ResolveBasePanelDepth(baseCabHinge, baseDim, part.Name);
                HingeHoleEdgeCalculator.ComputeHingeHoles(baseCabHinge, baseDim, panelDepth, holes, materialThickness34);
            }

            if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCabHinge && upperCabHinge.DrillHingeHoles)
            {
                var upperDim = UpperCabinetDimensions.From(upperCabHinge);
                double panelDepth = HingeHoleEdgeCalculator.ResolveUpperPanelDepth(upperCabHinge, upperDim, part.Name);
                HingeHoleEdgeCalculator.ComputeUpperHingeHoles(upperCabHinge, upperDim, panelDepth, holes);
            }
        }

        // 3. Finalize & Mirror if needed
        var result = new PartGeometry
        (
            PartInfo: part,
            OutlineVertices: outline,
            TenonThinningPockets: thinningPockets,
            MortisePockets: mortisePockets,
            MortisePocketsThru: mortisePocketsThru,
            Holes: holes,
            HolesThru: holesThru
        );

        if (!isCorner90 && part.Name.Contains("Right End", StringComparison.OrdinalIgnoreCase))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        if (isCorner90 && part.Name.Contains("Left End", StringComparison.OrdinalIgnoreCase))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        if (isLShape && part.Name.Contains("Top"))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        return result;
    }
}
