using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Assembles the final geometry for a part, handling winding order and edge routing.
/// 
/// Builds Outlines, Tenon Thinning Pockets, Mortise Pockets, Assembly Holes & Shelf Holes based on the PartInfo and JoineryConfig.
/// 
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

        // 2. Compute Joinery
        if (isLShape && baseCab != null)
        {
            ComputeLShapeJoinery(part, outline, thinningPockets, joinery, baseCab, materialThickness34);
            if (baseCab.HasTK && part.Name.Contains("Deck"))
            {
                ComputeMortisePockets(part, mortisePockets, joinery, baseCab, cabinet, materialThickness34);
            }
        }
        else if (!isLShape)
        {
            ComputeTenonsAndThinningPockets(part, outline, thinningPockets, joinery, baseCab, cabinet, materialThickness34);
            ComputeMortisePockets(part, mortisePockets, joinery, baseCab, cabinet, materialThickness34);
            ComputeScrewHoles(part, holesThru, joinery, baseCab, cabinet, materialThickness34);
            ComputeShelfHoles(part, holes, joinery, baseCab, materialThickness34);
        }

        // 3. Finalize & Mirror if needed
        var result = new PartGeometry
        (
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

    #region Outline Building
    private static void BuildOutline(PartInfo part, BaseCabinetModel? baseCab, bool isEndPanelWithTk, bool isLShape, double mt34, List<Vector2> outline)
    {
        if (isEndPanelWithTk)
        {
            outline.AddRange(BuildEndPanelWithToeKick(part.Bounds.Width, part.Bounds.Height, part.TkHeight, part.TkDepth));
        }
        else if (isLShape && baseCab != null)
        {
            // L-shape outline is built sequentially in ComputeLShapeJoinery
            // We just seed it with the first point here to keep the signature consistent
            outline.Add(new Vector2(0, 0));
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

    #region L-Shape Joinery (Sequential Edge Builder)
    private static void ComputeLShapeJoinery(PartInfo part, List<Vector2> outline, List<(double, double, double, double)> thinningPockets, JoineryConfig joinery, BaseCabinetModel baseCab, double mt34)
    {
        double gap = part.Name.Contains("Shelf") ? 0.125 : 0;
        double radius = 1.0;
        int segments = 8;
        double lf = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth);
        double rf = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth);
        double ld = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
        double rd = ConvertDimension.FractionToDouble(baseCab.RightDepth);
        double doubleMt = mt34 * 2;
        double insetLf = lf - mt34 - gap;
        double insetRf = rf - mt34 - gap;
        double insetLd = ld - doubleMt - gap;
        double insetRd = rd - doubleMt - gap;
        double dadoDepth = joinery.DadoDepth;

        // Clear the seed point added in BuildOutline
        outline.Clear();

        // 1. Start at (0,0)
        outline.Add(new Vector2(0, 0 + insetLd)); // Added + insetLd to all vertical Y points to bring X0, Y0 to the lower left of the part.

        // 2. Draw inner radius arc (NO TENONS)
        var arc = GenerateInsideCornerArc(insetLf, 0 + insetLd, radius, segments);
        foreach (var p in arc) outline.Add(new Vector2(p.X, p.Y));

        // 3. Draw the 4 outer edges sequentially with tenons
        // Edge 1: Top (Horizontal, Y=insetRf, X: insetLf → insetLf+insetRd)
        AppendHorizontalEdgeWithTenons(outline, new Vector2(insetLf, insetRf + insetLd), new Vector2(insetLf + insetRd, insetRf + insetLd), joinery, dadoDepth, protrudePositiveY: true, thinningPockets, part);

        // Edge 2: Right (Vertical, X=insetLf+insetRd, Y: insetRf → -insetLd)
        AppendVerticalEdgeWithTenons(outline, new Vector2(insetLf + insetRd, insetRf + insetLd), new Vector2(insetLf + insetRd, -insetLd + insetLd), joinery, dadoDepth, protrudePositiveX: true, thinningPockets, part);

        // Edge 3: Bottom (Horizontal, Y=-insetLd, X: insetLf+insetRd → 0)
        AppendHorizontalEdgeWithTenons(outline, new Vector2(insetLf + insetRd, -insetLd + insetLd), new Vector2(0, -insetLd + insetLd), joinery, dadoDepth, protrudePositiveY: false, thinningPockets, part);

        // Edge 4: Left (Vertical, X=0, Y: -insetLd → 0)
        AppendVerticalEdgeWithTenons(outline, new Vector2(0, -insetLd + insetLd), new Vector2(0, 0 + insetLd), joinery, dadoDepth, protrudePositiveX: false, thinningPockets, part);
    }

    private static void AppendHorizontalEdgeWithTenons(List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth, bool protrudePositiveY, List<(double, double, double, double)> thinningPockets, PartInfo part)
    {
        double x1 = start.X;
        double x2 = end.X;
        double y = start.Y;
        double length = Math.Abs(x2 - x1);
        bool leftToRight = x2 > x1;

        var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);

        // Add start point (if not already added by previous edge)
        if (outline.Count == 0 || outline.Last().X != start.X || outline.Last().Y != start.Y) outline.Add(start);

        if (tenons.Count == 0)
        {
            outline.Add(end);
            return;
        }

        if (!part.Name.Contains("Shelf"))
        {
            double currentX = x1;
            double protrusion = protrudePositiveY ? dadoDepth : -dadoDepth;

            foreach (var (tStart, tEnd) in tenons)
            {
                double tenonXStart = leftToRight ? currentX + tStart : currentX - tStart;
                double tenonXEnd = leftToRight ? currentX + tEnd : currentX - tEnd;

                // Draw to tenon start
                outline.Add(new Vector2(tenonXStart, y));
                // Protrude
                outline.Add(new Vector2(tenonXStart, y + protrusion));
                // Across tenon
                outline.Add(new Vector2(tenonXEnd, y + protrusion));
                // Back to edge
                outline.Add(new Vector2(tenonXEnd, y));
            }
        }

        // Draw remaining straight segment to end
        outline.Add(end);

        // Record thinning pocket
        if (!part.Name.Contains("Shelf"))
        {
            double pocketX1 = Math.Min(x1, x2) + joinery.TenonThinningOverrun;
            double pocketX2 = Math.Max(x1, x2) - joinery.TenonThinningOverrun;
            double pocketY1 = y;
            double pocketY2 = y;
            thinningPockets.Add((pocketX1, pocketX2, pocketY1, pocketY2));
        }
    }

    private static void AppendVerticalEdgeWithTenons(List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth, bool protrudePositiveX, List<(double, double, double, double)> thinningPockets, PartInfo part)
    {
        double y1 = start.Y;
        double y2 = end.Y;
        double x = start.X;
        double length = Math.Abs(y2 - y1);
        bool topToBottom = y2 < y1;

        var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);

        if (outline.Count == 0 || outline.Last().X != start.X || outline.Last().Y != start.Y) outline.Add(start);

        if (tenons.Count == 0)
        {
            outline.Add(end);
            return;
        }

        if (!part.Name.Contains("Shelf"))
        {
            double currentY = y1;
            double protrusion = protrudePositiveX ? dadoDepth : -dadoDepth;

            foreach (var (tStart, tEnd) in tenons)
            {
                double tenonYStart = topToBottom ? currentY - tStart : currentY + tStart;
                double tenonYEnd = topToBottom ? currentY - tEnd : currentY + tEnd;

                outline.Add(new Vector2(x, tenonYStart));
                outline.Add(new Vector2(x + protrusion, tenonYStart));
                outline.Add(new Vector2(x + protrusion, tenonYEnd));
                outline.Add(new Vector2(x, tenonYEnd));
            }
        }

        outline.Add(end);

        if (!part.Name.Contains("Shelf"))
        {
            double pocketX1 = x;
            double pocketX2 = x;
            double pocketY1 = Math.Min(y1, y2) + joinery.TenonThinningOverrun;
            double pocketY2 = Math.Max(y1, y2) - joinery.TenonThinningOverrun;
            thinningPockets.Add((pocketX1, pocketX2, pocketY1, pocketY2));
        }
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
        else if (partName.Contains("Toekick"))
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

        // --------------------------------------------------------- LEFT -------------------------------------------------------------------

        if (part.MortiseEdges.HasFlag(MortiseEdge.Left))
        {
            if (part.MortiseEdges.HasFlag(MortiseEdge.Left) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                double openingHeight = ConvertDimension.FractionToDouble(baseCab!.OpeningHeight1) + mt34;
                double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
                double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
                double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

                if (baseCab.TopType == "Stretcher") // Stretcher Top
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );

                }
                else // Full Top
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1) // Base Std 1 Drw Stretcher
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: opening1Height + mt34,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: true,
                        MaterialThickness34: mt34), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1) // Drw Base Stretchers
                {
                    for (int i = 0; i < baseCab.DrwCount; i++)
                    {
                        if (i == 1) openingHeight += opening2Height + mt34;
                        if (i == 2) openingHeight += opening3Height + mt34;

                        mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                        (
                            Edge: MortiseEdge.Left,
                            EdgeLength: stretcherWidth,
                            PartWidth: length,
                            PartHeight: height,
                            OffsetFromEdge: openingHeight,
                            OffsetAlongEdge: 0,
                            ForceTwoTenons: true,
                            BlindStartOverride: 1.25,
                            BlindStopOverride: 1.25,
                            FullThicknessTenon: true,
                            MaterialThickness34: mt34), joinery)
                        );
                    }
                }
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck")) // Base Corner 90 deg Toekick (right)
            {
                double leftDeckOffsetAlong = ConvertDimension.FractionToDouble(base90corner.RightBackWidth) - (3 * mt34) - base90corner.ToeKickRightWidth;
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Left,
                    EdgeLength: base90corner.ToeKickRightWidth,
                    PartWidth: 0,//length,
                    PartHeight: 0,//height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.LeftFrontWidth) - mt34 + ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: leftDeckOffsetAlong,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            if (part.MortiseEdges.HasFlag(MortiseEdge.Left) && part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper cab Top Mortises, 1/4" back
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec  // Upper cab Top Mortises, 3/4" back
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else // Standard catch-all Left Edge mortise
            {
                //mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                //(
                //    Edge: MortiseEdge.Left,
                //    EdgeLength: height,
                //    PartWidth: length,
                //    PartHeight: height,
                //    OffsetFromEdge: 0,
                //    OffsetAlongEdge: 0,
                //    ForceTwoTenons: false,
                //    BlindStartOverride: 2.75,
                //    BlindStopOverride: 2.75,
                //    FullThicknessTenon: false,
                //    MaterialThickness34: mt34), joinery)
                //);
            }
        }



        // --------------------------------------------------------- RIGHT -------------------------------------------------------------------

        if (part.MortiseEdges.HasFlag(MortiseEdge.Right))
        {
            if (part.MortiseEdges.HasFlag(MortiseEdge.Right) && part.Name.Contains("End") && cabinet is BaseCabinetModel) // Base Cabinet Deck, accounting for Back Thickness
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else if (part.MortiseEdges.HasFlag(MortiseEdge.Left) && part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper cab Top Mortises, 1/4" back
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec  // Upper cab Top Mortises, 3/4" back
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }



            else // Standard catch-all Right Edge mortise
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }



        // --------------------------------------------------------- BOTTOM -------------------------------------------------------------------

        if (part.MortiseEdges.HasFlag(MortiseEdge.Bottom))
        {
            if (part.Name.Contains("End") && cabinet is BaseCabinetModel &&  baseCab!.HasTK) // Std Base Toekick End Panel Mortises
            {
                double tkBottomOffsetAlong = length - ConvertDimension.FractionToDouble(baseCab.TKHeight);
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Bottom,
                    EdgeLength: ConvertDimension.FractionToDouble(baseCab.TKHeight) - 0.5,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: part.TkDepth,
                    OffsetAlongEdge: tkBottomOffsetAlong,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
            else // Standard catch-all for Bottom Edge mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Bottom,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }



        // --------------------------------------------------------- TOP -------------------------------------------------------------------

        if (part.MortiseEdges.HasFlag(MortiseEdge.Top))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Nailer mortise (1/4" back)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else // Base Back mortise (3/4" back)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: length - part.TkHeight - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper Cabinet 1/4" back end panel nailer mortises
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );

                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: length - upperNailerWidth - mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else // Upper Cabinet 3/4" back end panel mortises
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
            else if (part.Name.Contains("Deck") && part.CabinetModel is BaseCabinetModel baseCabStd && (baseCabStd.Style == CabinetStyles.Base.Standard || baseCabStd.Style == CabinetStyles.Base.Drawer)) // Base Std & Drw toekick top mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCabStd.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck"))
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: base90corner.ToeKickLeftWidth,
                    PartWidth: 0, //length,
                    PartHeight: ConvertDimension.FractionToDouble(base90corner.LeftDepth) - (2 * mt34),
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            else // Standard catch-all for Top Edge mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
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

        // --------------------------------------------------------- LEFT -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
        {
            double openingHeight = ConvertDimension.FractionToDouble(baseCab!.OpeningHeight1) + mt34;
            double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
            double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
            double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

            if (baseCab.TopType == "Stretcher") // Holes for Std/Drw Base Cab stretcher top
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Front hole
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: stretcherWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );

                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Rear hole
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: topStretcherBackWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: height - topStretcherBackWidth,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }

            if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1) // Holes for Base Std 1 Drw Drawer Stretcher
            {

                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: stretcherWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: mt34 + opening1Height,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }


            if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1) // Holes for Base Drw Drawer Stretchers
            {
                for (int i = 0; i < baseCab.DrwCount; i++)
                {
                    if (i == 1) openingHeight += opening2Height + mt34;
                    if (i == 2) openingHeight += opening3Height + mt34;

                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: openingHeight,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && cabinet is UpperCabinetModel upperCab && part.Name.Contains("End")) // Upper Cabinet Top screw holes
        {
            if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 1/4" back Top Screw Holes
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Top Screw Holes
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left)) // Generic catch-all Left Edge screw holes
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
            (
                Edge: ScrewHoleEdge.Left,
                EdgeLength: height,
                PartWidth: length,
                PartHeight: height,
                OffsetFromEdge: 0,
                OffsetAlongEdge: 0,
                ForceTwoTenons: false,
                BlindStartOverride: 2.75,
                BlindStopOverride: 2.75,
                MaterialThickness34: mt34,
                IncludeEndHoles: true), joinery)
            );
        }


        // --------------------------------------------------------- RIGHT -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right) && part.Name.Contains("End") && cabinet is BaseCabinetModel)
        {
            if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Cabinet End Panel Deck Screw Holes, 1/4" back
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else // Base Cabinet End Panel Deck Screw Holes, 3/4" back
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && cabinet is UpperCabinetModel upperCab && part.Name.Contains("End")) // Upper Cabinet Deck screw holes
        {
            if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 1/4" back Deck Screw Holes
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Deck Screw Holes
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right)) // Generic catch-all Right Edge screw holes
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Deck Screw Holes
            (
                Edge: ScrewHoleEdge.Right,
                EdgeLength: height,
                PartWidth: length,
                PartHeight: height,
                OffsetFromEdge: 0,
                OffsetAlongEdge: 0,
                ForceTwoTenons: false,
                BlindStartOverride: 2.75,
                BlindStopOverride: 2.75,
                MaterialThickness34: mt34,
                IncludeEndHoles: true), joinery)
            );
        }


        // --------------------------------------------------------- BOTTOM -------------------------------------------------------------------
        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Bottom))
        {
            if (part.Name.Contains("End") && cabinet is BaseCabinetModel && baseCab!.HasTK) // End Panel Screw Holes for toekick
            {
                double tkBottomOffsetAlong = length - ConvertDimension.FractionToDouble(baseCab.TKHeight);
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: ConvertDimension.FractionToDouble(baseCab.TKHeight) - 0.5,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: part.TkDepth,
                    OffsetAlongEdge: tkBottomOffsetAlong,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }

            else // Generic catch-all for Bottom Edge screw holes
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }




        // --------------------------------------------------------- TOP -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Top))
        {
            if (part.Name.Contains("End") && cabinet is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Cab nailer screw holes, 1/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)                   
                    );
                }

                else // Base Cabinet End Panel Back Screw Holes, 3/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length - mt34 - part.TkHeight,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper Cab nailer screw holes, 1/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );

                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: length - upperNailerWidth - mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else // Generic catch-all for Top Edge screw holes
            {
                if (cabinet is BaseCabinetModel && !part.Name.Contains("Deck")) // Base cabinet decks do not get screw holes for toekick mortises
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: part.TkDepth,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
        }
    }
    #endregion

    #region Shelf Holes
    private static void ComputeShelfHoles(PartInfo part, List<(double, double, double)> holes, JoineryConfig joinery, BaseCabinetModel? baseCab, double mt34)
    {
        if (!part.Name.Contains("End", StringComparison.OrdinalIgnoreCase)) return;

        if (part.CabinetModel is BaseCabinetModel bCab && bCab.Style == CabinetStyles.Base.Standard && bCab.DrillShelfHoles)
        {
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        if (part.CabinetModel is BaseCabinetModel bCab90Left && bCab90Left.Style == CabinetStyles.Base.Corner90 && bCab90Left.DrillShelfHoles && part.Name.Contains("Left End"))
        {
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        if (part.CabinetModel is BaseCabinetModel bCab90Right && bCab90Right.Style == CabinetStyles.Base.Corner90 && bCab90Right.DrillShelfHoles && part.Name.Contains("Right End"))
        {
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        else if (part.CabinetModel is UpperCabinetModel uCab && uCab.DrillShelfHoles)
        {
            holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }
    }
    #endregion
}
