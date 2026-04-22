using CorlaneCabinetOrderFormV3.Services;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Generates closed 2D polygons and mortise/hole data for cabinet part DXF export.
///
/// Tenon panel local coords (face up, laid flat):
///   Origin = front-left corner
///   X      = length  (interior-width direction)
///   Y      = depth   (front=0 → back=depthIn)
///   Tenons protrude ±X from the short left/right edges within [BlindStart, depth-BlindStop].
///
/// End panel local coords (inside face, laid flat):
///   X      = depth direction  (front=0 → back=depthIn)
///   Y      = height direction (bottom=0 → top=heightIn)
/// </summary>
internal static class PartOutlineBuilder
{
    /// <summary>Casts double coords to netDxf.Vector2 (which requires float) in one place.</summary>
    private static Vector2 V(double x, double y) => new((float)x, (float)y);

    // ── Rectangle ─────────────────────────────────────────────────────────────

    internal static List<Vector2> Rectangle(double length, double depth)
    {
        return
        [
            V(0,      0),
            V(length, 0),
            V(length, depth),
            V(0,      depth),
        ];
    }

    // ── Tenon panel: comb on BOTH short edges ─────────────────────────────────

    internal static List<Vector2> TenonBothEnds(
        double length, double depth, LockDadoSettings s)
    {
        double dd = s.DadoDepth;
        double blindStart = s.BlindStart;
        double blindEnd = depth - s.BlindStop;
        var tenons = ComputeTenonRanges(depth, s);
        var verts = new List<Vector2>();

        // Front edge (Y=0): left → right, no joinery
        verts.Add(V(0, 0));
        verts.Add(V(length, 0));

        // Right edge (X=length): front → back, comb in usable zone
        verts.Add(V(length, blindStart));
        foreach (var (tStart, tEnd) in tenons)
        {
            verts.Add(V(length, tStart));
            verts.Add(V(length + dd, tStart));   // protrude right
            verts.Add(V(length + dd, tEnd));
            verts.Add(V(length, tEnd));
        }
        verts.Add(V(length, blindEnd));
        verts.Add(V(length, depth));

        // Back edge (Y=depth): right → left, no joinery
        verts.Add(V(0, depth));

        // Left edge (X=0): back → front, comb in usable zone (reversed)
        verts.Add(V(0, blindEnd));
        for (int i = tenons.Count - 1; i >= 0; i--)
        {
            var (tStart, tEnd) = tenons[i];
            verts.Add(V(0, tEnd));
            verts.Add(V(-dd, tEnd));
            verts.Add(V(-dd, tStart));
            verts.Add(V(0, tStart));
        }
        verts.Add(V(0, blindStart));

        return verts;
    }

    // ── End panel ─────────────────────────────────────────────────────────────

    internal static List<Vector2> MortisePanel(double length, double depth)
        => Rectangle(length, depth);

    // ── Mortise pockets: depth-direction joints ───────────────────────────────

    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeDepthDirectionMortisePockets(
        double partDepth,
        double mortiseBottomY,
        TenonFlushFace flushFace,
        LockDadoSettings s)
    {
        double mt34 = MaterialDefaults.Thickness34;
        double slotHeight = s.MortiseSlotHeight;
        double usableStart = s.BlindStart;
        double usableEnd = partDepth - s.BlindStop;

        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + (mt34 - s.TenonThickness),
            _ => mortiseBottomY,
        };
        double slotTopY = slotBottomY + slotHeight;

        var tenons = ComputeTenonRanges(partDepth, s);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double x1 = Math.Max(tStart - s.MortiseOversize, usableStart);
            double x2 = Math.Min(tEnd + s.MortiseOversize, usableEnd);
            pockets.Add((x1, x2, slotBottomY, slotTopY));
        }

        return pockets;
    }

    // ── Mortise pockets: height-direction joint (toekick) ─────────────────────

    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeHeightDirectionMortisePockets(
        double tkHeight,
        double tkDepth,
        double cabinetDepth,
        LockDadoSettings s)
    {
        double toekickEdgeLen = tkHeight - 0.5;
        double toekickBottomY = 0.5;
        double toekickBackX = cabinetDepth - tkDepth;
        double slotX1 = toekickBackX - s.TenonThickness;
        double slotX2 = toekickBackX + s.TenonClearance;

        var tenons = ComputeTenonRanges(toekickEdgeLen, s);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double y1 = toekickBottomY + Math.Max(tStart - s.MortiseOversize, s.BlindStart);
            double y2 = toekickBottomY + Math.Min(tEnd + s.MortiseOversize, toekickEdgeLen - s.BlindStop);
            pockets.Add((slotX1, slotX2, y1, y2));
        }

        return pockets;
    }

    // ── Screw pilot holes: depth-direction joints ─────────────────────────────

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeDepthDirectionScrewHoles(
        double partDepth,
        double mortiseBottomY,
        TenonFlushFace flushFace,
        LockDadoSettings s)
    {
        double mt34 = MaterialDefaults.Thickness34;
        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + (mt34 - s.TenonThickness),
            _ => mortiseBottomY,
        };
        double holeCenterY = slotBottomY + (s.MortiseSlotHeight / 2.0);

        var tenons = ComputeTenonRanges(partDepth, s);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterX = (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
            holes.Add((gapCenterX, holeCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }

    // ── Screw pilot holes: height-direction joint (toekick) ───────────────────

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeHeightDirectionScrewHoles(
        double tkHeight,
        double tkDepth,
        double cabinetDepth,
        LockDadoSettings s)
    {
        double toekickEdgeLen = tkHeight - 0.5;
        double toekickBottomY = 0.5;
        double toekickBackX = cabinetDepth - tkDepth;
        double holeCenterX = toekickBackX - (s.TenonThickness / 2.0);

        var tenons = ComputeTenonRanges(toekickEdgeLen, s);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterY = toekickBottomY + (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
            holes.Add((holeCenterX, gapCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }

    // ── Tenon thinning pockets ────────────────────────────────────────────────

    internal static (double X1, double X2, double Y1, double Y2)[] ComputeTenonThinningPockets(
        double length,
        double depth,
        LockDadoSettings s)
    {
        double pocketY1 = s.BlindStart - s.TenonPocketOversize;
        double pocketY2 = depth - s.BlindStop + s.TenonPocketOversize;
        double dd = s.DadoDepth;

        var leftPocket = (X1: 0.0, X2: dd, Y1: pocketY1, Y2: pocketY2);
        var rightPocket = (X1: length - dd, X2: length, Y1: pocketY1, Y2: pocketY2);

        return [leftPocket, rightPocket];
    }

    // ── Core tenon layout algorithm ───────────────────────────────────────────

    internal static List<(double StartY, double EndY)> ComputeTenonRanges(
        double edgeLength, LockDadoSettings s)
    {
        double usableStart = s.BlindStart;
        double usableEnd = edgeLength - s.BlindStop;
        double usableLength = usableEnd - usableStart;

        int gapCount = s.GapCount(edgeLength);
        int tenonCount = gapCount + 1;
        double totalGapLen = gapCount * s.GapWidth;
        double tenonWidth = (usableLength - totalGapLen) / tenonCount;

        var ranges = new List<(double, double)>(tenonCount);
        double y = usableStart;
        for (int i = 0; i < tenonCount; i++)
        {
            ranges.Add((y, y + tenonWidth));
            y += tenonWidth;
            if (i < gapCount) y += s.GapWidth;
        }

        return ranges;
    }
}