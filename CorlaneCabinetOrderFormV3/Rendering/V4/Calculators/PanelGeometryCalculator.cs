using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Assembles the final geometry for a part, handling winding order and edge routing.
/// </summary>
internal static class PanelGeometryCalculator
{
    internal static PartGeometry Compute(PartInfo part, JoineryConfig joinery, CabinetModel cabinet, double materialThickness34)
    {
        var baseCab = part.CabinetModel as BaseCabinetModel;
        bool isCorner90 = baseCab != null && baseCab.Style == CabinetStyles.Base.Corner90;
        bool isLShape = isCorner90 && (part.Name.Contains("Top") || part.Name.Contains("Deck") || part.Name.Contains("Shelf"));
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();
        var holesThru = new List<(double x, double y, double radius)>();

        // 1. Build Outline
        BuildOutline(part, baseCab, isEndPanelWithTk, isLShape, materialThickness34, outline);

        // 2. Compute Joinery (skipped for L-shapes until custom logic is added)
        if (!isLShape)
        {
            ComputeTenonsAndThinningPockets(part, outline, thinningPockets, joinery, baseCab, cabinet, materialThickness34);
            ComputeMortisePockets(part, mortisePockets, joinery, baseCab, cabinet, materialThickness34);
            ComputeScrewHoles(part, holesThru, joinery, baseCab, cabinet, materialThickness34);
            ComputeShelfHoles(part, holes, joinery, baseCab, materialThickness34);
        }

        // 3. Finalize & Mirror if needed
        var result = new PartGeometry(
            PartInfo: part,
            OutlineVertices: outline,
            TenonThinningPockets: thinningPockets,
            MortisePockets: mortisePockets,
            Holes: holes,
            HolesThru: holesThru
        );

        if (!isCorner90 && part.Name.Contains("Right End", StringComparison.OrdinalIgnoreCase))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        return result;
    }

    #region Outline Building
    private static void BuildOutline(PartInfo part, BaseCabinetModel? baseCab, bool isEndPanelWithTk, bool isLShape, double mt34, List<Vector2> outline)
    {
        if (isEndPanelWithTk)
        {
            outline.AddRange(BuildEndPanelWithToeKick(part.Bounds.Width, part.Bounds.Height, part.TkHeight, part.TkDepth));
        }
        else if (isLShape && baseCab != null)
        {
            double gap = part.Name.Contains("Shelf") ? 0.125 : 0;
            double radius = 1.0;
            int segments = 8;
            double lf = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth);
            double rf = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth);
            double ld = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
            double rd = ConvertDimension.FractionToDouble(baseCab.RightDepth);
            outline.AddRange(BuildLShapedPanel(lf, rf, ld, rd, mt34, gap, radius, segments));
        }
        else
        {
            outline.Add(new Vector2(0, 0));
        }
    }

    private static List<Vector2> BuildEndPanelWithToeKick(double length, double height, double tkHeight, double tkDepth)
    {
        return new List<Vector2>
        {
            new(0, 0),
            new(length - tkHeight, 0),
            new(length - tkHeight, tkDepth),
            new(length, tkDepth),
            new(length, tkDepth + 3),
            new(length - 0.5, tkDepth + 3),
            new(length - 0.5, height - 3),
            new(length, height - 3),
            new(length, height),
            new(0, height)
        };
    }

    private static List<Vector2> BuildLShapedPanel(double leftFront, double rightFront, double leftDepth, double rightDepth, double mt34, double gap, double radius, int segments)
    {
        double doubleMt = mt34 * 2;
        double insetLeftFront = leftFront - mt34 - gap;
        double insetRightFront = rightFront - mt34 - gap;
        double insetLeftDepth = leftDepth - doubleMt - gap;
        double insetRightDepth = rightDepth - doubleMt - gap;

        var arc = GenerateInsideCornerArc(insetLeftFront, 0, radius, segments);
        var outline = new List<Vector2> { new(0, 0) };
        foreach (var p in arc) outline.Add(new Vector2(p.X, p.Y));
        outline.Add(new Vector2(insetLeftFront, insetRightFront));
        outline.Add(new Vector2(insetLeftFront + insetRightDepth, insetRightFront));
        outline.Add(new Vector2(insetLeftFront + insetRightDepth, -insetLeftDepth));
        outline.Add(new Vector2(0, -insetLeftDepth));
        return outline;
    }

    private static List<(double X, double Y)> GenerateInsideCornerArc(double cornerX, double cornerY, double radius, int segments)
    {
        double cx = cornerX - radius;
        double cy = cornerY + radius;
        var pts = new List<(double X, double Y)>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            double t = (double)i / segments;
            double angle = -(Math.PI / 2.0) + (t * Math.PI / 2.0);
            pts.Add((cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle)));
        }
        return pts;
    }
    #endregion

    #region Tenons & Thinning Pockets
    private static void ComputeTenonsAndThinningPockets(PartInfo part, List<Vector2> outline, List<(double, double, double, double)> thinningPockets, JoineryConfig joinery, BaseCabinetModel? baseCab, CabinetModel cabinet, double mt34)
    {
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double dadoDepth = joinery.DadoDepth;
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        if (part.TenonEdges.HasFlag(TenonEdge.Bottom))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            outline.Add(new Vector2(joinery.BlindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(tStart, 0));
                outline.Add(new Vector2(tStart, -dadoDepth));
                outline.Add(new Vector2(tEnd, -dadoDepth));
                outline.Add(new Vector2(tEnd, 0));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Bottom))
            {
                thinningPockets.Add(length < 6
                    ? (-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, 0, 0)
                    : (joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, 0, 0));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, 0));

        if (part.TenonEdges.HasFlag(TenonEdge.Right))
        {
            var tenons = ResolveTenonRanges(height, joinery, part.Name, cabinet);
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(length, tStart));
                outline.Add(new Vector2(length + dadoDepth, tStart));
                outline.Add(new Vector2(length + dadoDepth, tEnd));
                outline.Add(new Vector2(length, tEnd));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Right))
            {
                thinningPockets.Add(height < 6
                    ? (length, length, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun)
                    : (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer")
                        ? (length, length, 0, height)
                        : (length, length, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun)));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, height));

        if (part.TenonEdges.HasFlag(TenonEdge.Top))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(tEnd, height));
                outline.Add(new Vector2(tEnd, height + dadoDepth));
                outline.Add(new Vector2(tStart, height + dadoDepth));
                outline.Add(new Vector2(tStart, height));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Top))
            {
                thinningPockets.Add(length < 6
                    ? (-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, height, height)
                    : (joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, height, height));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(0, height));

        if (part.TenonEdges.HasFlag(TenonEdge.Left))
        {
            var tenons = ResolveTenonRanges(height, joinery, part.Name, cabinet);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(0, tEnd));
                outline.Add(new Vector2(-dadoDepth, tEnd));
                outline.Add(new Vector2(-dadoDepth, tStart));
                outline.Add(new Vector2(0, tStart));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Left))
            {
                thinningPockets.Add(height < 6
                    ? (0, 0, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun)
                    : (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer")
                        ? (0, 0, 0, height)
                        : (0, 0, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun)));
            }
        }
    }

    private static List<(double start, double end)> ResolveTenonRanges(double edgeLength, JoineryConfig joinery, string partName, CabinetModel cabinet)
    {
        var tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: edgeLength < 6);

        if (partName.Contains("Stretcher") || partName.Equals("Nailer"))
        {
            if (cabinet is BaseCabinetModel)
                tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 1.25, blindStopOverride: 1.25, forceTwoTenons: true);
            else if (cabinet is UpperCabinetModel && partName.Contains("Nailer"))
                tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 1, blindStopOverride: 1, forceTwoTenons: false);
        }
        else if (partName.Equals("Toekick"))
        {
            tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 0, blindStopOverride: 0, forceTwoTenons: true);
        }

        return tenons;
    }
    #endregion

    #region Mortises
    private static void ComputeMortisePockets(PartInfo part, List<(double, double, double, double)> mortisePockets, JoineryConfig joinery, BaseCabinetModel? baseCab, CabinetModel cabinet, double mt34)
    {
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double stretcherWidth = 6;
        double upperNailerWidth = 4;

        if (part.MortiseEdges.HasFlag(MortiseEdge.Left) && part.Name.Contains("End") && baseCab != null)
        {
            double openingHeight = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1) + mt34;
            double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
            double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
            double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

            if (baseCab.TopType == "Stretcher")
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, materialThickness34: mt34));
            else
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Left, joinery, additionalInset: 0, materialThickness34: mt34));

            if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: opening1Height + mt34, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, fullThicknessTenon: true, materialThickness34: mt34));

            if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
            {
                for (int i = 0; i < baseCab.DrwCount; i++)
                {
                    if (i == 1) openingHeight += opening2Height + mt34;
                    if (i == 2) openingHeight += opening3Height + mt34;
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: openingHeight, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, fullThicknessTenon: true, materialThickness34: mt34));
                }
            }
        }
        else if (part.MortiseEdges.HasFlag(MortiseEdge.Left))
        {
            mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Left, joinery, additionalInset: 0, materialThickness34: mt34));
        }

        if (part.MortiseEdges.HasFlag(MortiseEdge.Right) && part.Name.Contains("End") && baseCab != null)
        {
            if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Right, joinery, additionalInset: part.TkHeight, materialThickness34: mt34));
            else
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height - mt34, length, height, MortiseEdge.Right, joinery, additionalInset: part.TkHeight, materialThickness34: mt34));
        }
        else if (part.MortiseEdges.HasFlag(MortiseEdge.Right))
        {
            mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Right, joinery, additionalInset: 0, materialThickness34: mt34));
        }

        if (part.MortiseEdges.HasFlag(MortiseEdge.Bottom))
            mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Bottom, joinery, additionalInset: 0, materialThickness34: mt34));

        if (part.MortiseEdges.HasFlag(MortiseEdge.Top))
        {
            if (part.Name.Contains("End") && baseCab != null)
            {
                if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, height, MortiseEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true, materialThickness34: mt34));
                else
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length - ConvertDimension.FractionToDouble(baseCab.TKHeight), length, height, MortiseEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(upperNailerWidth, length, height, MortiseEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true, materialThickness34: mt34));
                else
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
            else
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
        }
    }
    #endregion

    #region Screw Holes
    private static void ComputeScrewHoles(PartInfo part, List<(double, double, double)> holesThru, JoineryConfig joinery, BaseCabinetModel? baseCab, CabinetModel cabinet, double mt34)
    {
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double stretcherWidth = 6;
        double upperNailerWidth = 4;
        double topStretcherBackWidth = 3;

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && part.Name.Contains("End") && baseCab != null)
        {
            double openingHeight = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1) + mt34;
            double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
            double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
            double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

            if (baseCab.TopType == "Stretcher")
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, materialThickness34: mt34));
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, height - topStretcherBackWidth, blindStopOverride: 0, materialThickness34: mt34));
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0, materialThickness34: mt34));
            }

            if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: opening1Height + mt34, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, materialThickness34: mt34));

            if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
            {
                for (int i = 0; i < baseCab.DrwCount; i++)
                {
                    if (i == 1) openingHeight += opening2Height + mt34;
                    if (i == 2) openingHeight += opening3Height + mt34;
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: openingHeight, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, materialThickness34: mt34));
                }
            }
        }
        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left))
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0, materialThickness34: mt34));
        }

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right) && part.Name.Contains("End") && baseCab != null)
        {
            if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Right, joinery, additionalInset: part.TkHeight, materialThickness34: mt34));
            else
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height - mt34, length, height, ScrewHoleEdge.Right, joinery, additionalInset: part.TkHeight, materialThickness34: mt34));
        }
        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right))
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Right, joinery, additionalInset: 0, materialThickness34: mt34));
        }

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Bottom))
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Bottom, joinery, additionalInset: 0, materialThickness34: mt34));

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Top))
        {
            if (part.Name.Contains("End") && baseCab != null)
            {
                if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true, materialThickness34: mt34));
                else
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length - ConvertDimension.FractionToDouble(baseCab.TKHeight), length - ConvertDimension.FractionToDouble(baseCab.TKHeight), height, ScrewHoleEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(upperNailerWidth, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true, materialThickness34: mt34));
                else
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0, materialThickness34: mt34));
            }
        }
    }
    #endregion

    #region Shelf Holes
    private static void ComputeShelfHoles(PartInfo part, List<(double, double, double)> holes, JoineryConfig joinery, BaseCabinetModel? baseCab, double mt34)
    {
        if (!part.Name.Contains("End", StringComparison.OrdinalIgnoreCase)) return;

        if (part.CabinetModel is BaseCabinetModel bCab && bCab.Style != CabinetStyles.Base.Drawer && bCab.DrillShelfHoles)
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        else if (part.CabinetModel is UpperCabinetModel uCab && uCab.DrillShelfHoles)
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
    }
    #endregion
}
