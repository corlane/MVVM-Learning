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
        bool fullThicknessTenon = false,
        double materialThickness34 = 0,
        double mStartAdditional = 0,
        double mEndAdditional = 0)
    {
        //materialThickness34 = materialThickness34;

        var pockets = new List<(double, double, double, double)>();

        // Get tenon ranges for positioning
        var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: forceTwoTenons, blindStartOverride: blindStartOverride, blindStopOverride: blindStopOverride);
        double slotWidth = (joinery.TenonThickness * materialThickness34) + joinery.TenonClearance;
        double oversize = joinery.MortiseOversize;

        foreach (var (tStart, tEnd) in tenonRanges)
        {
            double mStart = tStart - oversize + mStartAdditional;
            double mEnd = tEnd + oversize + mEndAdditional;

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





















    internal static List<(double x1, double x2, double y1, double y2)> ComputeMortisePocketsFromSpecs(
    MortisePlacementSpec spec,
    JoineryConfig joinery)
    {
        var pockets = new List<(double, double, double, double)>();
        var tenonRanges = TenonCalculator.ComputeTenonRanges(
            spec.EdgeLength, joinery,
            forceTwoTenons: spec.ForceTwoTenons,
            blindStartOverride: spec.BlindStartOverride,
            blindStopOverride: spec.BlindStopOverride);

        double slotWidth = (joinery.TenonThickness * spec.MaterialThickness34) + joinery.TenonClearance;
        double oversize = joinery.MortiseOversize;

        foreach (var (tStart, tEnd) in tenonRanges)
        {
            // Uniform shift along the edge replaces mStartAdditional/mEndAdditional
            double mStart = tStart + spec.OffsetAlongEdge - oversize;
            double mEnd = tEnd + spec.OffsetAlongEdge + oversize;

            switch (spec.Edge)
            {
                case MortiseEdge.Left:
                    pockets.Add(spec.FullThicknessTenon
                        ? (spec.MaterialThickness34 + spec.OffsetFromEdge, spec.MaterialThickness34 + spec.OffsetFromEdge - spec.MaterialThickness34, mStart, mEnd)
                        : (spec.MaterialThickness34 + spec.OffsetFromEdge, spec.MaterialThickness34 + spec.OffsetFromEdge - slotWidth, mStart, mEnd));
                    break;
                case MortiseEdge.Right:
                    pockets.Add((spec.PartWidth - spec.MaterialThickness34 - spec.OffsetFromEdge, spec.PartWidth - spec.MaterialThickness34 - spec.OffsetFromEdge + slotWidth, mStart, mEnd));
                    break;
                case MortiseEdge.Bottom:
                    pockets.Add((mStart, mEnd, spec.MaterialThickness34 + spec.OffsetFromEdge, spec.MaterialThickness34 + spec.OffsetFromEdge - slotWidth));
                    break;
                case MortiseEdge.Top:
                    pockets.Add((mStart, mEnd, spec.PartHeight - spec.MaterialThickness34 - spec.OffsetFromEdge + slotWidth, spec.PartHeight - spec.MaterialThickness34 - spec.OffsetFromEdge));
                    break;
            }
        }
        return pockets;
    }

}





















