using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Assembles the final geometry for a part, handling winding order and edge routing.
/// </summary>
internal static class PanelGeometryCalculator
{
    internal static PartGeometry Compute(PartInfo part, JoineryConfig joinery)
    {
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase)
                              && part.TkHeight > 0 && part.TkDepth > 0;

        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();
        var holesThru = new List<(double x, double y, double radius)>();

        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double dadoDepth = joinery.DadoDepth;

        // ── Branch to Toekick Outline ──────────────────────────────────────────
        if (isEndPanelWithTk)
        {
            outline = BuildEndPanelWithToeKick(length, height, part.TkHeight, part.TkDepth);
        }
        else
        {
            // ── Standard Panel Outline ─────────────────────────────────────────
            outline.Add(new Vector2(0, 0));

            // Bottom Edge
            if (part.TenonEdges.HasFlag(TenonEdge.Bottom))
            {
                //var tenons = TenonCalculator.ComputeTenonRanges(length, joinery);
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
                    if (length < 6)
                    {
                        thinningPockets.Add((-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, 0, 0));
                    }
                    else
                    {
                        thinningPockets.Add((joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, 0, 0));
                    }
                }
            }
            outline.Add(new Vector2(length, 0));




            // Right Edge
            if (part.TenonEdges.HasFlag(TenonEdge.Right))
            {
                //var tenons = TenonCalculator.ComputeTenonRanges(height, joinery);
                var tenons = TenonCalculator.ComputeTenonRanges(height, joinery, forceTwoTenons: height < 6);
                foreach (var (tStart, tEnd) in tenons)
                {
                    outline.Add(new Vector2(length, tStart));
                    outline.Add(new Vector2(length + dadoDepth, tStart));
                    outline.Add(new Vector2(length + dadoDepth, tEnd));
                    outline.Add(new Vector2(length, tEnd));
                }
                if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Right))
                {
                    if (height < 6)
                    {
                        thinningPockets.Add((length, length, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun));
                    }
                    else
                    {
                        thinningPockets.Add((length, length, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun));
                    }
                }
            }
            outline.Add(new Vector2(length, height));




            // Top Edge (Reverse order for winding)
            if (part.TenonEdges.HasFlag(TenonEdge.Top))
            {
                //var tenons = TenonCalculator.ComputeTenonRanges(length, joinery);
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
                    if (length < 6)
                    {
                        thinningPockets.Add((-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, height, height));
                    }
                    else
                    {
                        thinningPockets.Add((joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, height, height));
                    }
                }
            }
            outline.Add(new Vector2(0, height));




            // Left Edge (Reverse order for winding)
            if (part.TenonEdges.HasFlag(TenonEdge.Left))
            {
                //var tenons = TenonCalculator.ComputeTenonRanges(height, joinery);
                var tenons = TenonCalculator.ComputeTenonRanges(height, joinery, forceTwoTenons: height < 6);
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
                    if (height < 6)
                    {
                        thinningPockets.Add((0, 0, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun));
                    }
                    else
                    {
                        thinningPockets.Add((0, 0, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun));
                    }
                }
            }



            // ── Compute Mortise Pockets ──────────────────────────────────────────────
            if (part.MortiseEdges.HasFlag(MortiseEdge.Left))
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Left, joinery));

            if (part.MortiseEdges.HasFlag(MortiseEdge.Right))
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Right, joinery));

            if (part.MortiseEdges.HasFlag(MortiseEdge.Bottom))
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Bottom, joinery));

            if (part.MortiseEdges.HasFlag(MortiseEdge.Top))
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Top, joinery));



            // ── Compute Screw Holes in Gaps ─────────────────────────────────────────
            if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left))
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery));

            if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right))
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Right, joinery));

            if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Bottom))
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Bottom, joinery));

            if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Top))
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Top, joinery));
        }

        // ── Mirror Right End Panels ──────────────────────────────────────────────
        PartGeometry result = new PartGeometry(
            PartInfo: part,
            OutlineVertices: outline,
            TenonThinningPockets: thinningPockets,
            MortisePockets: mortisePockets,
            Holes: holes,
            HolesThru: holesThru
        );

        if (part.Name.Contains("Right End", StringComparison.OrdinalIgnoreCase))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        return result;
    }


    /// <summary>
    /// Generates end panel outline with rectangular toekick notch at bottom.
    /// </summary>
    private static List<Vector2> BuildEndPanelWithToeKick(double length, double height, double tkHeight, double tkDepth)
    {
        return new List<Vector2>
        {
            new (0,0),
            new (length-tkHeight, 0),
            new (length-tkHeight, tkDepth),
            new (length, tkDepth),
            new (length, tkDepth+3),
            new (length-0.5, tkDepth+3),
            new (length-0.5, height-3),
            new (length, height-3),
            new (length, height),
            new (0, height),
        };
    }
}
