using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Builds the 5-sided polygon outline for Angle Front cabinet deck/top panels with tenon protrusions.
/// Joinery goes on edges 1→2, 2→3, 3→4, 4→0 only. Edge 0→1 (front) has no joinery.
/// </summary>
internal static class AngleFrontJoineryCalculator
{
    internal static void ComputeAngleFrontJoinery(
        PartInfo part,
        List<Vector2> outline,
        List<(double x1, double x2, double y1, double y2)> thinningPockets,
        JoineryConfig joinery,
        CabinetModel cabinet,
        double mt34)
    {
        // Resolve dimensions from the appropriate model type
        double lbw, rbw, ld, rd;

        if (cabinet is BaseCabinetModel baseCab)
        {
            lbw = ConvertDimension.FractionToDouble(baseCab.LeftBackWidth);
            rbw = ConvertDimension.FractionToDouble(baseCab.RightBackWidth);
            ld = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
            rd = ConvertDimension.FractionToDouble(baseCab.RightDepth);
        }
        else if (cabinet is UpperCabinetModel upperCab)
        {
            lbw = ConvertDimension.FractionToDouble(upperCab.LeftBackWidth);
            rbw = ConvertDimension.FractionToDouble(upperCab.RightBackWidth);
            ld = ConvertDimension.FractionToDouble(upperCab.LeftDepth);
            rd = ConvertDimension.FractionToDouble(upperCab.RightDepth);
        }
        else
        {
            // Fallback — should not happen
            lbw = rbw = ld = rd = 24.0;
        }

        double gap = part.Name.Contains("Shelf") ? 0.125 : 0;
        double dadoDepth = joinery.DadoDepth;
        double backSetback = mt34 + 0.25; // constant from 3D builder: mt + .25

        outline.Clear();

        // ── Build the 5-sided polygon with tenons on non-front edges ──

        // Point 0: front-left corner
        var p0 = new Vector2(ld, mt34);
        outline.Add(p0);

        // Edge 0→1: Front edge — NO joinery (derived/calculated)
        var p1 = new Vector2(rbw - mt34, lbw - rd);
        outline.Add(p1);

        // Edge 1→2: Right side vertical (upward in Y from front-right to back-right inner)
        var p2 = new Vector2(rbw - mt34, lbw - backSetback);
        AppendVerticalEdgeWithTenons(outline, p1, p2, joinery, dadoDepth, protrudePositiveX: true, thinningPockets, part);

        // Edge 2→3: Back horizontal (leftward from right to left)
        var p3 = new Vector2(backSetback, lbw - backSetback);
        AppendHorizontalEdgeWithTenons(outline, p2, p3, joinery, dadoDepth, protrudePositiveY: true, thinningPockets, part);

        // Edge 3→4: Left-back vertical (downward in Y from back-left inner to inside-front-left)
        var p4 = new Vector2(backSetback, mt34);
        AppendVerticalEdgeWithTenons(outline, p3, p4, joinery, dadoDepth, protrudePositiveX: false, thinningPockets, part);

        // Edge 4→0: Left-front vertical (downward in Y from inside-front-left to front-left)
        AppendHorizontalEdgeWithTenons(outline, p4, p0, joinery, dadoDepth, protrudePositiveY: false, thinningPockets, part);
    }

    internal static void AppendHorizontalEdgeWithTenons(
        List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth,
        bool protrudePositiveY, List<(double x1, double x2, double y1, double y2)> thinningPockets, PartInfo part)
    {
        double x1 = start.X;
        double x2 = end.X;
        double y = start.Y;
        double length = Math.Abs(x2 - x1);
        bool leftToRight = x2 > x1;

        var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);

        if (outline.Count == 0 || outline.Last().X != start.X || outline.Last().Y != start.Y)
            outline.Add(start);

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

                outline.Add(new Vector2(tenonXStart, y));
                outline.Add(new Vector2(tenonXStart, y + protrusion));
                outline.Add(new Vector2(tenonXEnd, y + protrusion));
                outline.Add(new Vector2(tenonXEnd, y));
            }
        }

        outline.Add(end);

        if (!part.Name.Contains("Shelf"))
        {
            double pocketX1 = Math.Min(x1, x2) + joinery.TenonThinningOverrun;
            double pocketX2 = Math.Max(x1, x2) - joinery.TenonThinningOverrun;
            thinningPockets.Add((pocketX1, pocketX2, y, y));
        }
    }

    internal static void AppendVerticalEdgeWithTenons(
        List<Vector2> outline, Vector2 start, Vector2 end, JoineryConfig joinery, double dadoDepth,
        bool protrudePositiveX, List<(double x1, double x2, double y1, double y2)> thinningPockets, PartInfo part)
    {
        double y1 = start.Y;
        double y2 = end.Y;
        double x = start.X;
        double length = Math.Abs(y2 - y1);
        bool topToBottom = y2 < y1;

        var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);

        if (outline.Count == 0 || outline.Last().X != start.X || outline.Last().Y != start.Y)
            outline.Add(start);

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
            double pocketY1 = Math.Min(y1, y2) + joinery.TenonThinningOverrun;
            double pocketY2 = Math.Max(y1, y2) - joinery.TenonThinningOverrun;
            thinningPockets.Add((x, x, pocketY1, pocketY2));
        }
    }
}
