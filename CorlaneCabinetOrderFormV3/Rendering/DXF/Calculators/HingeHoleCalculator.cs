using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Calculates hinge bore positions for DXF end-panel output.
/// Logically identical to <see cref="HingeHolesCalculator"/>, but returns coordinates
/// rotated 90° CCW to match the DXF flat-panel coordinate space
/// (X runs along panel height, Y runs along panel depth).
/// </summary>
internal static class HingeHoleCalculator
{
    // 5mm diameter hole (matches other DXF hole calculators)
    private const double HoleRadius = 0.19685 / 2.0;

    // ── Base cabinet ──

    internal static List<(double x, double y, double radius)> Compute(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double panelDepth,
        double materialThickness34)
    {
        var bores = ComputeBaseBorePositions(baseCab, dim, panelDepth, materialThickness34);

        var holes = new List<(double x, double y, double radius)>();
        foreach (var b in bores)
        {
            // 90° CCW: 3D(X=depth, Y=height) → DXF(X=height, Y=depth)
            holes.Add((b.Y, b.X, HoleRadius));
        }

        return holes;
    }

    // ── Upper cabinet ──

    internal static List<(double x, double y, double radius)> Compute(
        UpperCabinetModel upperCab,
        UpperCabinetDimensions dim,
        double panelDepth)
    {
        var bores = ComputeUpperBorePositions(upperCab, dim, panelDepth);

        var holes = new List<(double x, double y, double radius)>();
        foreach (var b in bores)
        {
            holes.Add((b.Y, b.X, HoleRadius));
        }

        return holes;
    }

    // ── Base cabinet bore positions (3D space: X=depth, Y=height) ──

    private static List<(double X, double Y)> ComputeBaseBorePositions(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double panelDepth,
        double materialThickness34)
    {
        const double hingeBoreSpacing = 1.26;
        const double hingeXFromFront = 1.456;
        const double hingeCenterInset = 2.5197;
        const double maxHingeCenterSpacing = 40.0;

        double height = dim.Height;
        double tk_Height = dim.TKHeight;
        double opening1Height = dim.Opening1Height;
        double topReveal = dim.DoorTopReveal;
        double bottomReveal = dim.DoorBottomReveal;
        double doorGap = dim.BaseDoorGap;

        double hingeX = panelDepth - hingeXFromFront;

        double topCenterY = height - hingeCenterInset;
        double bottomCenterY = tk_Height + bottomReveal + hingeCenterInset;

        if (baseCab.DrwCount == 1)
        {
            double drawerStretcherBottomY = height - opening1Height - (2 * materialThickness34);
            double drawerStretcherCenterY = drawerStretcherBottomY + (materialThickness34 / 2);
            topCenterY = drawerStretcherCenterY - (doorGap / 2) - hingeCenterInset;
        }
        else
        {
            topCenterY = height - topReveal - hingeCenterInset;
        }

        if (topCenterY < bottomCenterY)
        {
            (topCenterY, bottomCenterY) = (bottomCenterY, topCenterY);
        }

        double spanY = Math.Max(0, topCenterY - bottomCenterY);
        int hingeCount = Math.Max(2, (int)Math.Ceiling(spanY / maxHingeCenterSpacing) + 1);

        var bores = new List<(double X, double Y)>();

        for (int h = 0; h < hingeCount; h++)
        {
            double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
            double hingeCenterY = bottomCenterY + (spanY * t);

            bores.Add((hingeX, hingeCenterY - (hingeBoreSpacing / 2)));
            bores.Add((hingeX, hingeCenterY + (hingeBoreSpacing / 2)));
        }

        return bores;
    }

    // ── Upper cabinet bore positions (3D space: X=depth, Y=height) ──

    private static List<(double X, double Y)> ComputeUpperBorePositions(
        UpperCabinetModel upperCab,
        UpperCabinetDimensions dim,
        double panelDepth)
    {
        const double hingeBoreSpacing = 1.26;
        const double hingeXFromFront = 1.456;
        const double hingeCenterInset = 2.5197;
        const double maxHingeCenterSpacing = 40.0;

        double height = dim.Height;

        double hingeX = panelDepth - hingeXFromFront;
        double topCenterY = height - hingeCenterInset;
        double bottomCenterY = hingeCenterInset;

        if (topCenterY < bottomCenterY)
        {
            (topCenterY, bottomCenterY) = (bottomCenterY, topCenterY);
        }

        double spanY = Math.Max(0, topCenterY - bottomCenterY);
        int hingeCount = Math.Max(2, (int)Math.Ceiling(spanY / maxHingeCenterSpacing) + 1);

        var bores = new List<(double X, double Y)>();

        for (int h = 0; h < hingeCount; h++)
        {
            double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
            double hingeCenterY = bottomCenterY + (spanY * t);

            bores.Add((hingeX, hingeCenterY - (hingeBoreSpacing / 2)));
            bores.Add((hingeX, hingeCenterY + (hingeBoreSpacing / 2)));
        }

        return bores;
    }
}