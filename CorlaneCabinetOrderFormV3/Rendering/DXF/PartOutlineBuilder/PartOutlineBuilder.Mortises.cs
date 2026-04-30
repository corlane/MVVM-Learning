using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class PartOutlineBuilder
{
    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeDepthDirectionMortisePockets(
        double partDepth, double mortiseBottomY, TenonFlushFace flushFace, LockDadoSettings s,
        double xOffset = 0, bool forceTwoTenons = false)
    {
        s ??= new LockDadoSettings();
        double mt34 = MaterialDefaults.Thickness34;
        double slotHeight = s.MortiseSlotHeight;
        double usableStart = s.BlindStart;
        double usableEnd = partDepth - s.BlindStop;

        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + (mt34 - s.TenonThickness),
            _ => mortiseBottomY
        };
        double slotTopY = slotBottomY + slotHeight;

        var tenons = ComputeTenonRanges(partDepth, s, forceTwoTenons, blindStart: null, blindStop: null);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double x1 = Math.Max(tStart - s.MortiseOversize, usableStart) + xOffset;
            double x2 = Math.Min(tEnd + s.MortiseOversize, usableEnd) + xOffset;
            pockets.Add((x1, x2, slotBottomY, slotTopY));
        }

        return pockets;
    }

    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeHeightDirectionMortisePockets(
        double edgeLength, double xPosition, double bottomY, TenonFlushFace flushFace, LockDadoSettings s,
        bool forceTwoTenons = false)
    {
        double slotX1 = flushFace switch
        {
            TenonFlushFace.Back => xPosition,
            TenonFlushFace.Front => xPosition,
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        double slotX2 = flushFace switch
        {
            TenonFlushFace.Back => xPosition + s.MortiseSlotHeight,
            TenonFlushFace.Front => xPosition + s.MortiseSlotHeight,
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        var tenons = ComputeTenonRanges(edgeLength, s, forceTwoTenons, blindStart: null, blindStop: null);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double y1 = bottomY + Math.Max(tStart - s.MortiseOversize, s.BlindStart);
            double y2 = bottomY + Math.Min(tEnd + s.MortiseOversize, edgeLength - s.BlindStop);
            pockets.Add((slotX1, slotX2, y1, y2));
        }

        return pockets;
    }
}