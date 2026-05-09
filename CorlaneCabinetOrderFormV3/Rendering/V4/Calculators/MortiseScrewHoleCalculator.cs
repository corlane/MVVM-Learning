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
        JoineryConfig joinery,
        double additionalInset = 0,
        bool forceTwoTenons = false,
        double blindStartOverride = 2,
        double blindStopOverride = 2)

    {
        var holes = new List<(double, double, double)>();
        //var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery);
        var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: forceTwoTenons, blindStartOverride: blindStartOverride, blindStopOverride: blindStopOverride);
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
                    holeX = edgeOffset + additionalInset;
                    holeY = gapCenter;
                    break;
                case ScrewHoleEdge.Right:
                    holeX = partWidth - edgeOffset - additionalInset;
                    holeY = gapCenter;
                    break;
                case ScrewHoleEdge.Bottom:
                    holeX = gapCenter;
                    holeY = edgeOffset + additionalInset;
                    break;
                case ScrewHoleEdge.Top:
                    holeX = gapCenter;
                    holeY = partHeight - edgeOffset - additionalInset;
                    break;
                default:
                    continue;
            }
            holes.Add((holeX, holeY, radius));
        }

        // Add holes at the start and end of each mortise run (If ForceTwoTenons is false)
        if (!forceTwoTenons)
        {
            double holeX, holeY;

            // First hole (Left edge for horizontal, bottom edge for vertical)
            switch (edge)
            {
                case ScrewHoleEdge.Left:
                    holeX = edgeOffset + additionalInset;
                    holeY = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Right:
                    holeX = partWidth - edgeOffset - additionalInset;
                    holeY = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Bottom:
                    holeX = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
                    holeY = edgeOffset + additionalInset;
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Top:
                    holeX = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
                    holeY = partHeight - edgeOffset - additionalInset;
                    holes.Add((holeX, holeY, radius));
                    break;
            }

            // Second hole (Right edge for horizontal, top edge for vertical)
            switch (edge)
            {
                case ScrewHoleEdge.Left:
                    holeX = edgeOffset + additionalInset;
                    holeY = partHeight - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Right:
                    holeX = partWidth - edgeOffset - additionalInset;
                    holeY = partHeight - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Bottom:
                    holeX = partWidth - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
                    holeY = edgeOffset + additionalInset;
                    holes.Add((holeX, holeY, radius));
                    break;
                case ScrewHoleEdge.Top:
                    holeX = partWidth - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
                    holeY = partHeight - edgeOffset - additionalInset;
                    holes.Add((holeX, holeY, radius));
                    break;
            }

        }
        return holes;
    }
}
