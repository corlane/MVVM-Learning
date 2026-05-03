using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Collections.Generic;

/// <summary>
/// Centralized calculator for determining hinge bore positions across cabinet end panels.
/// Serves as the single source of truth for both manufacturing data generation and 3D WYSIWYG rendering.
/// </summary>
/// <remarks>
/// Calculates hinge positions based on cabinet height, toe kick height, and optional drawer stretcher adjustments.
/// Returns a flat list of HingeBore records, where each hinge is represented by two bores (top and bottom)
/// spaced 1.26" apart vertically. Horizontal position is fixed at 1.456" from the front edge.
/// </remarks>
internal static class HingeHolesCalculator
{
    public record HingeBore(double X, double Y);

    public static List<HingeBore> Compute(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double materialThickness34)
    {
        var bores = new List<HingeBore>();

        const double hingeBoreSpacing = 1.26;
        const double hingeXFromFront = 1.456;
        const double hingeCenterInset = 2.5197;
        const double maxHingeCenterSpacing = 40.0;

        double depth = dim.Depth;
        double height = dim.Height;
        double tk_Height = dim.TKHeight;
        double opening1Height = dim.Opening1Height;
        double topReveal = dim.DoorTopReveal;
        double bottomReveal = dim.DoorBottomReveal;
        double doorGap = dim.BaseDoorGap;

        double hingeX = depth - hingeXFromFront;

        double topCenterY = height - hingeCenterInset;
        double bottomCenterY = tk_Height + bottomReveal + hingeCenterInset;

        if (baseCab.DrwCount == 1)
        {
            double drawerStretcherBottomY = height - opening1Height - (2 * materialThickness34);
            double drawerStretcherCenterY = drawerStretcherBottomY + (materialThickness34 / 2); // this places the reference point in the center of the drawer stretcher
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

        for (int h = 0; h < hingeCount; h++)
        {
            double t = hingeCount == 1 ? 0 : (double)h / (hingeCount - 1);
            double hingeCenterY = bottomCenterY + (spanY * t);

            double y1 = hingeCenterY - (hingeBoreSpacing / 2);
            double y2 = hingeCenterY + (hingeBoreSpacing / 2);

            bores.Add(new HingeBore(hingeX, y1));
            bores.Add(new HingeBore(hingeX, y2));
        }

        return bores;
    }
}
