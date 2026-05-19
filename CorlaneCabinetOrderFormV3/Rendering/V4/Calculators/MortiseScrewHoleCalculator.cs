using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;
using System.Diagnostics;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class MortiseScrewHoleCalculator
{
    /// <summary>
    /// Computes through-holes centered in the gaps between tenon/mortise positions.
    /// Offset from the edge by half the material thickness.
    /// </summary>
    //internal static List<(double x, double y, double radius)> ComputeScrewHoles(
    //    double edgeLength,
    //    double partWidth,
    //    double partHeight,
    //    ScrewHoleEdge edge,
    //    JoineryConfig joinery,
    //    double additionalInset = 0,
    //    bool forceTwoTenons = false,
    //    double blindStartOverride = 2,
    //    double blindStopOverride = 2,
    //    double materialThickness34 = 0,
    //    double mStartAdditional = 0,
    //    double mEndAdditional = 0)

    //{
    //    var holes = new List<(double, double, double)>();
    //    var tenonRanges = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: forceTwoTenons, blindStartOverride: blindStartOverride, blindStopOverride: blindStopOverride, materialThickness34: materialThickness34);
    //    double radius = joinery.ScrewPilotHoleDiameter / 2.0;
    //    double edgeOffset = materialThickness34 / 2.0;

    //    // Iterate through gaps between consecutive tenons
    //    for (int i = 0; i < tenonRanges.Count - 1; i++)
    //    {
    //        double gapCenter = (tenonRanges[i].end + tenonRanges[i + 1].start) / 2.0;

    //        double holeX, holeY;
    //        switch (edge)
    //        {
    //            case ScrewHoleEdge.Left:
    //                holeX = edgeOffset + additionalInset;
    //                holeY = gapCenter;
    //                break;
    //            case ScrewHoleEdge.Right:
    //                holeX = partWidth - edgeOffset - additionalInset;
    //                holeY = gapCenter;
    //                break;
    //            case ScrewHoleEdge.Bottom:
    //                holeX = gapCenter;
    //                holeY = edgeOffset + additionalInset;
    //                break;
    //            case ScrewHoleEdge.Top:
    //                holeX = gapCenter;
    //                holeY = partHeight - edgeOffset - additionalInset;
    //                break;
    //            default:
    //                continue;
    //        }
    //        holes.Add((holeX, holeY, radius));
    //    }

    //    // Add holes at the start and end of each mortise run (If ForceTwoTenons is false)
    //    if (!forceTwoTenons)
    //    {
    //        double holeX, holeY;

    //        // First hole (Left edge for horizontal, bottom edge for vertical)
    //        switch (edge)
    //        {
    //            case ScrewHoleEdge.Left:
    //                holeX = edgeOffset + additionalInset;
    //                holeY = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Right:
    //                holeX = partWidth - edgeOffset - additionalInset;
    //                holeY = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Bottom:
    //                holeX = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
    //                holeY = edgeOffset + additionalInset;
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Top:
    //                holeX = blindStartOverride - joinery.MortiseOversize - (joinery.GapWidth / 2);
    //                holeY = partHeight - edgeOffset - additionalInset;
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //        }

    //        // Second hole (Right edge for horizontal, top edge for vertical)
    //        switch (edge)
    //        {
    //            case ScrewHoleEdge.Left:
    //                holeX = edgeOffset + additionalInset;
    //                holeY = partHeight - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Right:
    //                holeX = partWidth - edgeOffset - additionalInset;
    //                holeY = partHeight - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Bottom:
    //                holeX = partWidth - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
    //                holeY = edgeOffset + additionalInset;
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //            case ScrewHoleEdge.Top:
    //                holeX = partWidth - blindStopOverride + joinery.MortiseOversize + (joinery.GapWidth / 2);
    //                holeY = partHeight - edgeOffset - additionalInset;
    //                holes.Add((holeX, holeY, radius));
    //                break;
    //        }
    //    }
    //    return holes;
    //}








    internal static List<(double x, double y, double radius)> ComputeScrewHolesFromSpecs(
    ScrewHolePlacementSpec spec,
    JoineryConfig joinery)
    {
        var holes = new List<(double, double, double)>();
        var tenonRanges = TenonCalculator.ComputeTenonRanges(
            spec.EdgeLength, joinery,
            forceTwoTenons: spec.ForceTwoTenons,
            blindStartOverride: spec.BlindStartOverride,
            blindStopOverride: spec.BlindStopOverride,
            materialThickness34: spec.MaterialThickness34);

        double radius = joinery.ScrewPilotHoleDiameter / 2.0;
        double edgeOffset = spec.MaterialThickness34 / 2.0;

        // 1. Holes in gaps between tenons (unchanged)
        for (int i = 0; i < tenonRanges.Count - 1; i++)
        {
            double gapCenter = ((tenonRanges[i].end + tenonRanges[i + 1].start) / 2.0) + spec.OffsetAlongEdge;
            holes.Add(GetHoleCoords(spec, gapCenter, edgeOffset));
        }

        // 2. End holes (now controlled explicitly)
        if (spec.IncludeEndHoles && tenonRanges.Count > 0)
        {
            // Anchor to actual first/last tenon boundaries instead of 0/EdgeLength
            double startHolePos = tenonRanges[0].start - (joinery.GapWidth / 2) + spec.OffsetAlongEdge;
            double endHolePos = tenonRanges[^1].end + (joinery.GapWidth / 2) + spec.OffsetAlongEdge;

            holes.Add(GetHoleCoords(spec, startHolePos, edgeOffset));
            holes.Add(GetHoleCoords(spec, endHolePos, edgeOffset));
        }

        return holes;

        (double x, double y, double r) GetHoleCoords(ScrewHolePlacementSpec s, double posAlongEdge, double off)
        {
            return s.Edge switch
            {
                ScrewHoleEdge.Left => (off + s.OffsetFromEdge, posAlongEdge, radius),
                ScrewHoleEdge.Right => (s.PartWidth - off - s.OffsetFromEdge, posAlongEdge, radius),
                ScrewHoleEdge.Bottom => (posAlongEdge, off + s.OffsetFromEdge, radius),
                ScrewHoleEdge.Top => (posAlongEdge, s.PartHeight - off - s.OffsetFromEdge, radius),
                _ => (0, 0, radius)
            };
        }
    }

}
