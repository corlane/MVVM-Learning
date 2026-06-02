using CorlaneCabinetOrderFormV3.Converters;
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
        var cabinet = part.CabinetModel;

        // Cabinet-type-agnostic style detection
        bool isCorner90 = cabinet.IsCorner90();
        bool isAngleFront = cabinet.IsAngleFront();
        bool isLShape = isCorner90 && (part.Name.Contains("Top") || part.Name.Contains("Deck") || part.Name.Contains("Shelf"));
        bool isAngleFrontPanel = isAngleFront && (part.Name.Contains("Top") || part.Name.Contains("Deck") || part.Name.Contains("Shelf"));
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePocketsThru = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();
        var holesThru = new List<(double x, double y, double radius)>();

        // 1. Build Outline
        OutlineBuilder.BuildOutline(part, isEndPanelWithTk, isLShape || isAngleFrontPanel, materialThickness34, outline);

        // 2. Compute Joinery
        if (isLShape)
        {
            LShapeJoineryCalculator.ComputeLShapeJoinery(part, outline, thinningPockets, joinery, cabinet, materialThickness34);
            if (cabinet.HasToeKick() && part.Name.Contains("Deck"))
            {
                MortiseBlindCalculator.ComputeMortisePockets(part, mortisePockets, joinery, materialThickness34);
            }
        }
        else if (isAngleFrontPanel)
        {
            AngleFrontJoineryCalculator.ComputeAngleFrontJoinery(part, outline, thinningPockets, joinery, cabinet, materialThickness34);
            if (cabinet.HasToeKick() && part.Name.Contains("Deck"))
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

            // Generate screw holes for parts that have MortiseThruEdges but no corresponding ScrewHoleEdges (e.g., Upper Cabinet Back)
            if (part.MortiseThruEdges != MortiseThruEdge.None && part.ScrewHoleEdges == ScrewHoleEdge.None)
            {
                ScrewHoleEdgeCalculator.ComputeScrewHolesFromMortiseThru(part, holesThru, joinery, materialThickness34);
            }

            ShelfHoleEdgeCalculator.ComputeShelfHoles(part, holes, joinery, materialThickness34);

            // Drawer slides — BaseCabinetModel only (uppers don't have drawers)
            if (part.Name.Contains("End") && cabinet is BaseCabinetModel baseCabDrawers)
            {
                DrawerSlideEdgeCalculator.ComputeDrawerSlideHoles(baseCabDrawers, holes, joinery, materialThickness34);
            }

            // Hinges — BaseCabinetModel
            if (part.Name.Contains("End") && cabinet is BaseCabinetModel baseCabHinge && baseCabHinge.DrillHingeHoles && baseCabHinge.Style != CabinetStyles.Base.Drawer)
            {
                var baseDim = BaseCabinetDimensions.From(baseCabHinge);
                double panelDepth = HingeHoleEdgeCalculator.ResolveBasePanelDepth(baseCabHinge, baseDim, part.Name);
                HingeHoleEdgeCalculator.ComputeHingeHoles(baseCabHinge, baseDim, panelDepth, holes, materialThickness34);
            }

            // Hinges — UpperCabinetModel
            if (part.Name.Contains("End") && cabinet is UpperCabinetModel upperCabHinge && upperCabHinge.DrillHingeHoles)
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

        // ── Angle Front rotation: make p0→p1 horizontal at Y=mt34, clockwise around p0 ──
        if (isAngleFrontPanel && (part.Name.Contains("Top") || part.Name.Contains("Deck")))
        {
            double lbw = cabinet switch
            {
                BaseCabinetModel bc => ConvertDimension.FractionToDouble(bc.LeftBackWidth),
                UpperCabinetModel uc => ConvertDimension.FractionToDouble(uc.LeftBackWidth),
                _ => 24.0
            };
            double rbw = cabinet switch
            {
                BaseCabinetModel bc => ConvertDimension.FractionToDouble(bc.RightBackWidth),
                UpperCabinetModel uc => ConvertDimension.FractionToDouble(uc.RightBackWidth),
                _ => 24.0
            };
            double ld = cabinet switch
            {
                BaseCabinetModel bc => ConvertDimension.FractionToDouble(bc.LeftDepth),
                UpperCabinetModel uc => ConvertDimension.FractionToDouble(uc.LeftDepth),
                _ => 24.0
            };
            double rd = cabinet switch
            {
                BaseCabinetModel bc => ConvertDimension.FractionToDouble(bc.RightDepth),
                UpperCabinetModel uc => ConvertDimension.FractionToDouble(uc.RightDepth),
                _ => 24.0
            };

            // p0 = (ld, mt34), p1 = (rbw - mt34, lbw - rd)
            double pivotX = ld;
            double pivotY = materialThickness34;
            double dx = rbw - materialThickness34 - ld;
            double dy = lbw - rd - materialThickness34;

            // Clockwise rotation angle to bring p0→p1 horizontal (Y=0 relative to pivot)
            double angleRadians = Math.Atan2(dy, dx);

            result = result.RotateAngleParts(pivotX, pivotY, angleRadians);
        }

        if (isAngleFrontPanel && part.Name.Contains("Top"))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }


        return result;
    }
}
