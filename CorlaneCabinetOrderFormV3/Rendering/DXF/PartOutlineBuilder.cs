using CorlaneCabinetOrderFormV3.Services;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

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