using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

internal static class PartGeometryExtensions
{
    /// <summary>
    /// Mirrors all geometry across the vertical centerline (X-axis midpoint).
    /// X' = Width - X, Y unchanged.
    /// </summary>
    internal static PartGeometry MirrorAcrossVerticalCenterline(this PartGeometry geometry, double panelWidth)
    {
        var mirroredOutline = geometry.OutlineVertices
            .Select(v => new Vector2(panelWidth - v.X, v.Y))
            .ToList();

        var mirroredThinningPockets = geometry.TenonThinningPockets
            .Select(p => (panelWidth - p.x2, panelWidth - p.x1, p.y1, p.y2))
            .ToList();

        var mirroredMortisePockets = geometry.MortisePockets
            .Select(p => (panelWidth - p.x2, panelWidth - p.x1, p.y1, p.y2))
            .ToList();

        var mirroredHoles = geometry.Holes
            .Select(h => (panelWidth - h.x, h.y, h.radius))
            .ToList();

        var mirroredHolesThru = geometry.HolesThru
            .Select(h => (panelWidth - h.x, h.y, h.radius))
            .ToList();

        return new PartGeometry(
            PartInfo: geometry.PartInfo,
            OutlineVertices: mirroredOutline,
            TenonThinningPockets: mirroredThinningPockets,
            MortisePockets: mirroredMortisePockets,
            Holes: mirroredHoles,
            HolesThru: mirroredHolesThru
        );
    }
}
