using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Builds the L-shape cabinet outline sequentially with tenon protrusions,
/// handling inside corner arcs and edge-by-edge tenon placement.
/// </summary>
internal static class LShapeJoineryCalculator
{
    internal static void ComputeLShapeJoinery(PartInfo part, List<Vector2> outline, List<(double, double, double, double)> thinningPockets, JoineryConfig joinery, CabinetModel cabinet, double mt34)
    {
        double gap = part.Name.Contains("Shelf") ? 0.125 : 0;
        double radius = 1.0;
        int segments = 8;
        var (_, _, lf, rf, ld, rd) = cabinet.GetCornerDimensions();
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
        var arc = OutlineBuilder.GenerateInsideCornerArc(insetLf, 0 + insetLd, radius, segments);
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

    internal static void AppendHorizontalEdgeWithTenons(List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth, bool protrudePositiveY, List<(double, double, double, double)> thinningPockets, PartInfo part)
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

    internal static void AppendVerticalEdgeWithTenons(List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth, bool protrudePositiveX, List<(double, double, double, double)> thinningPockets, PartInfo part)
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
}
