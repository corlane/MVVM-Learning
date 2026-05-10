using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Pure math logic for computing tenon ranges and thinning pockets.
/// </summary>
internal static class TenonCalculator
{
    /// <summary>
    /// Computes the start/end ranges of tenons along an edge.
    /// </summary>
    internal static List<(double start, double end)> ComputeTenonRanges(
        double edgeLength,
        JoineryConfig joinery,
        bool forceTwoTenons = false,
        double? blindStartOverride = null,
        double? blindStopOverride = null)
    {
        var blindStart = blindStartOverride ?? joinery.BlindStart;
        var blindStop = blindStopOverride ?? joinery.BlindStop;

        double usableStart = blindStart;
        double usableEnd = edgeLength - blindStop;
        double usableLength = usableEnd - usableStart;

        if (usableLength <= 0) return [];

        // Determine tenon count
        int tenonCount = forceTwoTenons ? 2 : CalculateTenonCount(usableLength, joinery);
        int gapCount = tenonCount - 1;

        // Calculate dimensions
        double totalGapWidth = gapCount * joinery.GapWidth;
        double totalTenonWidth = usableLength - totalGapWidth;

        if (totalTenonWidth < 0) totalTenonWidth = 0; // Safety clamp

        double singleTenonWidth = totalTenonWidth / tenonCount;

        // Generate ranges
        var ranges = new List<(double start, double end)>();
        double currentX = usableStart;

        for (int i = 0; i < tenonCount; i++)
        {
            double tStart = currentX;
            double tEnd = currentX + singleTenonWidth;
            ranges.Add((tStart, tEnd));

            // Move to next tenon position (add tenon width + gap width)
            currentX += singleTenonWidth + (i < gapCount ? joinery.GapWidth : 0);
        }

        return ranges;
    }

    private static int CalculateTenonCount(double usableLength, JoineryConfig config)
    {
        // Simple heuristic: 1 tenon for short edges, more for longer ones
        // Mirrors existing GapCount logic but isolated
        if (usableLength < 9) return 1;
        if (usableLength < 20) return 2;
        if (usableLength < 30) return 3;
        return 4;
    }
}
