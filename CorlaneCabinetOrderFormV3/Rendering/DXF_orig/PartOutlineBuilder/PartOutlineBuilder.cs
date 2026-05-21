using CorlaneCabinetOrderFormV3.Services;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Part outline generation helpers for DXF export, focusing on tenon geometry calculation.
/// 
/// Computes the positions, sizes, and spacing of tenons on joinery-bearing part edges.
/// Results drive PartOutlineBuilder methods (BuildPanelWithTenons, ComputeTenonThinningPockets, etc.)
/// and control what geometry gets rendered to DXF layers.
/// 
/// Tenon Layout Algorithm (ComputeTenonRanges):
///   Divides an edge into alternating tenons and gaps, returning start/end positions for each tenon.
///   
///   Inputs:
///     • edgeLength          — total length of the edge in inches
///     • LockDadoSettings s  — blind depth, gap width/count, tenon height, etc.
///     • forceTwoTenons      — override: always 2 tenons (used for Toekick, Top Stretcher)
///     • blindStart/Stop     — optional override for blind pocket depths (edge-specific)
///   
///   Algorithm:
///     1. Calculate usable length after blind pockets:
///        usableStart = blindStart (default: s.BlindStart)
///        usableEnd = edgeLength - blindStop (default: s.BlindStop)
///        usableLength = usableEnd - usableStart
///     
///     2. Determine gap/tenon count:
///        gapCount = s.GapCount(edgeLength)    [e.g., 1 gap for short edges, 2–3 for longer]
///        tenonCount = gapCount + 1            [one more tenon than gaps]
///     
///     3. Calculate uniform tenon width:
///        totalGapLen = gapCount × s.GapWidth
///        tenonWidth = (usableLength - totalGapLen) / tenonCount
///     
///     4. Special case (forceTwoTenons = true):
///        Override to exactly 2 tenons with 1 gap centered:
///        tenonWidth = (usableLength - s.GapWidth) / 2.0
///        Returns: [(usableStart, t1_end), (t2_start, usableEnd)]
///        Used by Toekick and Top Stretcher where design requires exactly 2 mortises.
///     
///     5. General case (forceTwoTenons = false):
///        Populate evenly-spaced tenon ranges:
///        Iterate: add tenon range → advance by tenon width → skip gap width → repeat
///        Returns: list of (StartY, EndY) tuples in order from bottom to top
///   
///   Output (List<(double StartY, double EndY)>):
///     Each tuple represents one tenon's vertical extent on the edge.
///     Used to render:
///       • Tenon outline segments (via PartOutlineBuilder.BuildPanelWithTenons)
///       • Tenon thinning pockets (via PartOutlineBuilder.ComputeTenonThinningPockets)
/// 
/// Coordinate System:
///   • Y-axis: edge length (bottom-to-top orientation)
///   • StartY/EndY: always StartY < EndY (ascending order for consistent CNC tool paths)
/// 
/// Configuration (LockDadoSettings):
///   s.BlindStart, s.BlindStop   — depth of blind pockets from each end
///   s.GapWidth                  — width of gaps between tenons
///   s.GapCount(edgeLength)      — number of gaps (typically 1–3 based on edge length)
///   s.TenonHeight               — vertical extent of each tenon (tenon thickness in Y)
/// 
/// Helper:
///   Vertex()
///     Converts (double, double) to Vector2 for polyline construction.
/// </summary>

[Flags]
internal enum EdgeDesignators
{
    None = 0,
    Left = 1, Right = 2, Bottom = 4, Top = 8,
    LeftRight = Left | Right,
    TopBottom = Top | Bottom,
    All = Left | Right | Top | Bottom
}

internal static partial class PartOutlineBuilder
{
    private static Vector2 Vertex(double x, double y) => new((float)x, (float)y);

    internal static List<(double StartY, double EndY)> ComputeTenonRanges(
        double edgeLength, LockDadoSettings s, bool forceTwoTenons = false,
        double? blindStart = null, double? blindStop = null)
    {
        double usableStart = blindStart ?? s.BlindStart;
        double usableEnd = edgeLength - (blindStop ?? s.BlindStop);
        double usableLength = usableEnd - usableStart;

        int gapCount = s.GapCount(edgeLength);
        int tenonCount = gapCount + 1;
        double totalGapLen = gapCount * s.GapWidth;
        double tenonWidth = (usableLength - totalGapLen) / tenonCount;

        if (forceTwoTenons)
        {
            tenonWidth = (usableLength - s.GapWidth) / 2.0;
            return
            [
                (usableStart, usableStart + tenonWidth),
                (usableStart + tenonWidth + s.GapWidth, usableEnd)
            ];
        }

        var ranges = new List<(double, double)>(tenonCount);
        double y = usableStart;
        for (int i = 0; i < tenonCount; i++)
        {
            ranges.Add((y, y + tenonWidth));
            y += tenonWidth;
            if (i < gapCount) y += s.GapWidth;
        }

        return ranges;
    }
}