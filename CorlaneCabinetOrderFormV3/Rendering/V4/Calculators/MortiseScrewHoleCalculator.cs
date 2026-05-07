using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class MortiseScrewHoleCalculator
{
    /// <summary>
    /// Computes through-holes centered in the gaps between tenon/mortise positions.
    /// Offset from the edge by half the material thickness.
    /// </summary>
    internal static List<(double x, double y, double radius)> ComputeScrewHoles(
        double edgeLength,
        double partWidth,
        double partHeight,
        ScrewHoleEdge edge,
        JoineryConfig joinery)
    {
        var holes = new List<(double, double, double)>();
        var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery);
        double radius = joinery.ScrewPilotHoleDiameter / 2.0;
        double edgeOffset = joinery.Thickness34 / 2.0;

        // Iterate through gaps between consecutive tenons
        for (int i = 0; i < tenonRanges.Count - 1; i++)
        {
            double gapCenter = (tenonRanges[i].end + tenonRanges[i + 1].start) / 2.0;

            double holeX, holeY;
            switch (edge)
            {
                case ScrewHoleEdge.Left:
                    holeX = edgeOffset;
                    holeY = gapCenter;
                    break;
                case ScrewHoleEdge.Right:
                    holeX = partWidth - edgeOffset;
                    holeY = gapCenter;
                    break;
                case ScrewHoleEdge.Bottom:
                    holeX = gapCenter;
                    holeY = edgeOffset;
                    break;
                case ScrewHoleEdge.Top:
                    holeX = gapCenter;
                    holeY = partHeight - edgeOffset;
                    break;
                default:
                    continue;
            }
            holes.Add((holeX, holeY, radius));
        }
        return holes;
    }
}
