using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Calculates hinge bore positions for DXF end-panel output.
/// DXF Coordinate space for Left End (90° CCW rotated):
///   X=0 is cabinet top, increases downward
///   Y=0 is cabinet front face, increases toward back
/// </summary>
internal static class HingeHoleCalculator
{
    private const double HoleRadius = 0.19685 / 2.0;
    private const double HingeBoreSpacing = 1.26;
    private const double HingeFromFront = 1.456;
    private const double HingeCenterInset = 2.5197;
    private const double MaxHingeSpacing = 40.0;

    // ── Base cabinet ──
    internal static List<(double x, double y, double radius)> Compute(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double panelDepth,
        double materialThickness34)
    {
        double topHingeX = baseCab.DrwCount == 1
            ? materialThickness34 + dim.Opening1Height + (dim.BaseDoorGap / 2.0) + HingeCenterInset
            : dim.DoorTopReveal + HingeCenterInset;

        double bottomHingeX = dim.Height - dim.TKHeight - dim.DoorBottomReveal - HingeCenterInset;
        return BuildHoleList(topHingeX, bottomHingeX, HingeFromFront);
    }

    // ── Upper cabinet ──
    internal static List<(double x, double y, double radius)> Compute(
        UpperCabinetModel upperCab,
        UpperCabinetDimensions dim,
        double panelDepth)
    {
        double topHingeX = dim.DoorTopReveal + HingeCenterInset;
        double bottomHingeX = dim.Height - dim.DoorBottomReveal - HingeCenterInset;
        return BuildHoleList(topHingeX, bottomHingeX, HingeFromFront);
    }

    // ── Shared hole generation ──
    private static List<(double x, double y, double radius)> BuildHoleList(double topHingeX, double bottomHingeX, double hingeY)
    {
        var centersX = new List<double> { topHingeX, bottomHingeX };

        // Insert middle hinge if spacing exceeds 40"
        if (Math.Abs(topHingeX - bottomHingeX) > MaxHingeSpacing)
        {
            centersX.Insert(1, (topHingeX + bottomHingeX) / 2.0);
        }

        var holes = new List<(double x, double y, double radius)>();
        foreach (var cx in centersX)
        {
            holes.Add((cx - HingeBoreSpacing / 2.0, hingeY, HoleRadius));
            holes.Add((cx + HingeBoreSpacing / 2.0, hingeY, HoleRadius));
        }

        return holes;
    }
}