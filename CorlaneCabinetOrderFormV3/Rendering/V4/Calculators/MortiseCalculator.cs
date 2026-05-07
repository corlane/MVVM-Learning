using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class MortiseCalculator
{
    /// <summary>
    /// Computes mortise pocket rectangles for a given edge.
    /// Reuses TenonCalculator for positioning, then applies clearance offsets.
    /// </summary>
    internal static List<(double x1, double x2, double y1, double y2)> ComputeMortisePockets(
        double edgeLength,
        MortiseEdge edge,
        JoineryConfig joinery)
    {
        var pockets = new List<(double, double, double, double)>();

        // Get tenon ranges for positioning
        var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery);

        double slotWidth = joinery.TenonThickness + joinery.TenonClearance;
        double oversize = joinery.MortiseOversize;
        double blindStart = joinery.BlindStart;
        double blindStop = joinery.BlindStop;

        foreach (var (tStart, tEnd) in tenonRanges)
        {
            double mStart = Math.Max(tStart - oversize, blindStart);
            double mEnd = Math.Min(tEnd + oversize, edgeLength - blindStop);

            switch (edge)
            {
                case MortiseEdge.Left:
                    // Mortise on left edge: X runs 0→slotWidth, Y runs along edge
                    pockets.Add((0, slotWidth, mStart, mEnd));
                    break;
                case MortiseEdge.Right:
                    // Mortise on right edge: X runs (length-slotWidth)→length
                    pockets.Add((edgeLength - slotWidth, edgeLength, mStart, mEnd));
                    break;
                case MortiseEdge.Bottom:
                    // Mortise on bottom edge: Y runs 0→slotWidth, X runs along edge
                    pockets.Add((mStart, mEnd, 0, slotWidth));
                    break;
                case MortiseEdge.Top:
                    // Mortise on top edge: Y runs (height-slotWidth)→height
                    pockets.Add((mStart, mEnd, edgeLength - slotWidth, edgeLength));
                    break;
            }
        }

        return pockets;
    }
}
