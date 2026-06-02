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

        //var mirroredThinningPockets = geometry.TenonThinningPockets
        //    .Select(p => (panelWidth - p.x2, panelWidth - p.x1, p.y1, p.y2))
        //    .ToList();
        var mirroredThinningPockets = geometry.TenonThinningPockets
            .Select(p => (panelWidth - p.x1, panelWidth - p.x2, p.y1, p.y2))
            .ToList();

        var mirroredMortisePockets = geometry.MortisePockets
            .Select(p => (panelWidth - p.x2, panelWidth - p.x1, p.y1, p.y2))
            .ToList();

        var mirroredMortisePocketsThru = geometry.MortisePocketsThru
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
            MortisePocketsThru: mirroredMortisePocketsThru,
            Holes: mirroredHoles,
            HolesThru: mirroredHolesThru
        );
    }

    /// <summary>
    /// Rotates all geometry clockwise around a pivot point so that the line from p0 to p1
    /// becomes horizontal along the X-axis, with p0 remaining at its original location.
    /// Used for Angle Front cabinet Top & Deck panels.
    /// 
    /// NOTE: Thinning pockets are NOT rotated — they remain as axis-aligned lines from their
    /// pre-rotation computation so the DXF renderer draws them correctly as single-line segments.
    /// </summary>
    internal static PartGeometry RotateAngleParts(
        this PartGeometry geometry,
        double pivotX,
        double pivotY,
        double angleRadians)
    {
        double cosA = Math.Cos(angleRadians);
        double sinA = Math.Sin(angleRadians);

        var rotatedOutline = geometry.OutlineVertices
            .Select(v => RotatePoint(pivotX, pivotY, cosA, sinA, v.X, v.Y))
            .ToList();

        // Thinning pockets are NOT rotated — kept as axis-aligned lines for correct DXF rendering
        var rotatedThinningPockets = geometry.TenonThinningPockets
            .Select(p => RotatePocket(pivotX, pivotY, cosA, sinA, p.x1, p.y1, p.x2, p.y2))
            .ToList();

        var rotatedMortisePockets = geometry.MortisePockets
            .Select(p => RotatePocket(pivotX, pivotY, cosA, sinA, p.x1, p.y1, p.x2, p.y2))
            .ToList();

        var rotatedMortisePocketsThru = geometry.MortisePocketsThru
            .Select(p => RotatePocket(pivotX, pivotY, cosA, sinA, p.x1, p.y1, p.x2, p.y2))
            .ToList();

        var rotatedHoles = geometry.Holes
            .Select(h => {
                var rp = RotatePoint(pivotX, pivotY, cosA, sinA, h.x, h.y);
                return (rp.X, rp.Y, h.radius);
            })
            .ToList();

        var rotatedHolesThru = geometry.HolesThru
            .Select(h => {
                var rp = RotatePoint(pivotX, pivotY, cosA, sinA, h.x, h.y);
                return (rp.X, rp.Y, h.radius);
            })
            .ToList();

        return new PartGeometry(
            PartInfo: geometry.PartInfo,
            OutlineVertices: rotatedOutline,
            TenonThinningPockets: rotatedThinningPockets,
            MortisePockets: rotatedMortisePockets,
            MortisePocketsThru: rotatedMortisePocketsThru,
            Holes: rotatedHoles,
            HolesThru: rotatedHolesThru
        );

        static Vector2 RotatePoint(double cx, double cy, double cosA, double sinA, double x, double y) =>
            new(
                cx + (x - cx) * cosA + (y - cy) * sinA,
                cy - (x - cx) * sinA + (y - cy) * cosA);

        static (double x1, double x2, double y1, double y2) RotatePocket(
            double cx, double cy, double cosA, double sinA,
            double x1, double y1, double x2, double y2)
        {
            var p1 = RotatePoint(cx, cy, cosA, sinA, x1, y1);
            var p2 = RotatePoint(cx, cy, cosA, sinA, x2, y2);
            return (p1.X, p2.X, p1.Y, p2.Y);
        }
    }

}
