using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class PartOutlineBuilder
{
    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeDepthDirectionScrewHoles(
        double partDepth, double mortiseBottomY, TenonFlushFace flushFace, LockDadoSettings s,
        double xOffset = 0, bool forceTwoTenons = false)
    {
        double mt34 = MaterialDefaults.Thickness34;
        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + (mt34 - s.TenonThickness),
            _ => mortiseBottomY
        };
        double holeCenterY = slotBottomY + (s.MortiseSlotHeight / 2.0);

        var tenons = ComputeTenonRanges(partDepth, s, forceTwoTenons, blindStart: null, blindStop: null);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterX = (tenons[i].EndY + tenons[i + 1].StartY) / 2.0 + xOffset;
            holes.Add((gapCenterX, holeCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeHeightDirectionScrewHoles(
        double edgeLength, double xPosition, double bottomY, TenonFlushFace flushFace, LockDadoSettings s,
        bool forceTwoTenons = false)
    {
        double holeCenterX = flushFace switch
        {
            TenonFlushFace.Back => xPosition - (s.TenonThickness / 2.0),
            TenonFlushFace.InteriorFront => xPosition + (s.MortiseSlotHeight / 2.0),
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        var tenons = ComputeTenonRanges(edgeLength, s, forceTwoTenons, blindStart: null, blindStop: null);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterY = bottomY + (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
            holes.Add((holeCenterX, gapCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }
}