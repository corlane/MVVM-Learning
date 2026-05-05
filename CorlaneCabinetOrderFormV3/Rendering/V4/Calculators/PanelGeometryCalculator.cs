using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Assembles the final geometry for a part, handling winding order and edge routing.
/// </summary>
internal static class PanelGeometryCalculator
{
    internal static PartGeometry Compute(PartInfo part, JoineryConfig config)
    {
        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();

        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double dadoDepth = config.DadoDepth;

        // 1. Build Outline Vertices (Counter-Clockwise)
        outline.Add(new Vector2(0, 0));

        // Bottom Edge
        if (part.TenonEdges.HasFlag(Edge.Bottom))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, config);
            outline.Add(new Vector2(config.BlindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(tStart, 0));
                outline.Add(new Vector2(tStart, -dadoDepth));
                outline.Add(new Vector2(tEnd, -dadoDepth));
                outline.Add(new Vector2(tEnd, 0));

                // Add thinning pocket for this tenon
                thinningPockets.Add((tStart, tEnd, -dadoDepth, 0));
            }
        }
        outline.Add(new Vector2(length, 0));

        // Right Edge
        if (part.TenonEdges.HasFlag(Edge.Right))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(height, config);
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(length, tStart));
                outline.Add(new Vector2(length + dadoDepth, tStart));
                outline.Add(new Vector2(length + dadoDepth, tEnd));
                outline.Add(new Vector2(length, tEnd));

                thinningPockets.Add((length, length + dadoDepth, tStart, tEnd));
            }
        }
        outline.Add(new Vector2(length, height));

        // Top Edge (Reverse order for winding)
        if (part.TenonEdges.HasFlag(Edge.Top))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, config);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(tEnd, height));
                outline.Add(new Vector2(tEnd, height + dadoDepth));
                outline.Add(new Vector2(tStart, height + dadoDepth));
                outline.Add(new Vector2(tStart, height));

                thinningPockets.Add((tStart, tEnd, height, height + dadoDepth));
            }
        }
        outline.Add(new Vector2(0, height));

        // Left Edge (Reverse order for winding)
        if (part.TenonEdges.HasFlag(Edge.Left))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(height, config);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(0, tEnd));
                outline.Add(new Vector2(-dadoDepth, tEnd));
                outline.Add(new Vector2(-dadoDepth, tStart));
                outline.Add(new Vector2(0, tStart));

                thinningPockets.Add((-dadoDepth, 0, tStart, tEnd));
            }
        }

        return new PartGeometry(
            PartInfo: part,
            OutlineVertices: outline,
            ThinningPockets: thinningPockets,
            MortisePockets: mortisePockets,
            Holes: holes
        );
    }
}
