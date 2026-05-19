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





















