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
        JoineryConfig config,
        bool forceTwoTenons = false,
        double? blindStartOverride = null,
        double? blindStopOverride = null)
    {
        var blindStart = blindStartOverride ?? config.BlindStart;
        var blindStop = blindStopOverride ?? config.BlindStop;

        double usableStart = blindStart;
        double usableEnd = edgeLength - blindStop;
        double usableLength = usableEnd - usableStart;

        if (usableLength <= 0) return [];

        // Determine tenon count
        int tenonCount = forceTwoTenons ? 2 : CalculateTenonCount(usableLength, config);
        int gapCount = tenonCount - 1;

        // Calculate dimensions
        double totalGapWidth = gapCount * config.GapWidth;
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
            currentX += singleTenonWidth + (i < gapCount ? config.GapWidth : 0);
        }

        return ranges;
    }


    /// <summary>
    /// Computes thinning pocket rectangles for a given edge.
    /// </summary>
    internal static List<(double x1, double x2, double y1, double y2)> ComputeThinningPockets(
        List<(double start, double end)> tenonRanges,
        double pocketDepth)
    {
        var pockets = new List<(double x1, double x2, double y1, double y2)>();

        foreach (var (start, end) in tenonRanges)
        {
            // Pocket extends from surface (0) to pocketDepth
            pockets.Add((start, end, 0, pocketDepth));
        }

        return pockets;
    }

    private static int CalculateTenonCount(double usableLength, JoineryConfig config)
    {
        // Simple heuristic: 1 tenon for short edges, more for longer ones
        // Mirrors existing GapCount logic but isolated
        if (usableLength < 10) return 1;
        if (usableLength < 20) return 2;
        if (usableLength < 30) return 3;
        return 4;
    }
}
