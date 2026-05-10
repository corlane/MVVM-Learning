using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class MortiseCalculator
{
    /// <summary>
    /// Computes mortise pocket rectangles for a given edge.
    /// Reuses TenonCalculator for positioning, then applies clearance offsets.
    /// Uses full part dimensions to correctly position pockets in global coordinates.
    /// </summary>
    internal static List<(double x1, double x2, double y1, double y2)> ComputeMortisePockets(
        double edgeLength,
        double partWidth,
        double partHeight,
        MortiseEdge edge,
        JoineryConfig joinery,
        double additionalInset,
        bool forceTwoTenons = false,
        double blindStartOverride = 2,
        double blindStopOverride = 2,
        bool fullThicknessTenon = false)
    {
        double materialThickness34 = MaterialDefaults.Thickness34;

        var pockets = new List<(double, double, double, double)>();

        // Get tenon ranges for positioning
        var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: forceTwoTenons, blindStartOverride: blindStartOverride, blindStopOverride: blindStopOverride);
        double slotWidth = joinery.TenonThickness + joinery.TenonClearance;
        double oversize = joinery.MortiseOversize;

        foreach (var (tStart, tEnd) in tenonRanges)
        {
            double mStart = tStart - oversize;
            double mEnd = tEnd + oversize;

            switch (edge)
            {
                case MortiseEdge.Left:
                    // Mortise on left edge: X runs 0→slotWidth, Y runs along edge
                    if (fullThicknessTenon)
                    {
                        pockets.Add((materialThickness34 + additionalInset, materialThickness34 + additionalInset - materialThickness34, mStart, mEnd));
                    }
                    else
                    {
                        pockets.Add((materialThickness34 + additionalInset, materialThickness34 + additionalInset - slotWidth, mStart, mEnd));
                    }
                    break;
                case MortiseEdge.Right:
                    // Mortise on right edge: X runs (partWidth-slotWidth)→partWidth
                    pockets.Add((partWidth - materialThickness34 - additionalInset, partWidth - materialThickness34 - additionalInset + slotWidth, mStart, mEnd));
                    break;
                case MortiseEdge.Bottom:
                    // Mortise on bottom edge: Y runs 0→slotWidth, X runs along edge
                    pockets.Add((mStart, mEnd, materialThickness34 + additionalInset, materialThickness34 + additionalInset - slotWidth));
                    break;
                case MortiseEdge.Top:
                    // Mortise on top edge: Y runs (partHeight-slotWidth)→partHeight
                    pockets.Add((mStart, mEnd, partHeight - materialThickness34 - additionalInset + slotWidth, partHeight - materialThickness34 - additionalInset));
                    break;
            }
        }
        return pockets;
    }
}
