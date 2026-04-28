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
    private static Vector2 Vertex(double x, double y) => new((float)x, (float)y);

    // ── Rectangle ─────────────────────────────────────────────────────────────

    internal static List<Vector2> Rectangle(double length, double depth)
    {
        return
        [
            Vertex(0,      0),
            Vertex(length, 0),
            Vertex(length, depth),
            Vertex(0,      depth),
        ];
    }

    // ── End Panel With Toekick ─────────────────────────────────────────────────────────────

    internal static List<Vector2> EndPanelWithToeKick(double depth, double height, double tkHeight, double tkDepth)
    {
        // Matches BaseCabinetBuilder.Standard.EndPanels.cs BuildEndPanels(HasTK=true)
        // X = depth direction (front=0 → back=depth), Y = height direction
        return
        [
            Vertex(depth,               tkHeight),
            Vertex(depth,               height),
            Vertex(0,                   height),
            Vertex(0,                   0),
            Vertex(3,                   0),
            Vertex(3,                   0.5),
            Vertex(depth - tkDepth - 3, 0.5),
            Vertex(depth - tkDepth - 3, 0),
            Vertex(depth - tkDepth,     0),
            Vertex(depth - tkDepth,     tkHeight),
        ];
    }


    // ── Tenon panel: comb on BOTH short edges ─────────────────────────────────

    internal static List<Vector2> TenonBothEnds(
        double length, double depth, LockDadoSettings s, bool forceTwoTenons = false)
    {
        double dd = s.DadoDepth;
        double blindStart = s.BlindStart;
        double blindEnd = depth - s.BlindStop;
        var tenons = ComputeTenonRanges(depth, s, forceTwoTenons);
        var verts = new List<Vector2>();

        // Front edge (Y=0): left → right, no joinery
        verts.Add(Vertex(0, 0));
        verts.Add(Vertex(length, 0));

        // Right edge (X=length): front → back, comb in usable zone
        verts.Add(Vertex(length, blindStart));
        foreach (var (tStart, tEnd) in tenons)
        {
            verts.Add(Vertex(length, tStart));
            verts.Add(Vertex(length + dd, tStart));   // protrude right
            verts.Add(Vertex(length + dd, tEnd));
            verts.Add(Vertex(length, tEnd));
        }
        verts.Add(Vertex(length, blindEnd));
        verts.Add(Vertex(length, depth));

        // Back edge (Y=depth): right → left, no joinery
        verts.Add(Vertex(0, depth));

        // Left edge (X=0): back → front, comb in usable zone (reversed)
        verts.Add(Vertex(0, blindEnd));
        for (int i = tenons.Count - 1; i >= 0; i--)
        {
            var (tStart, tEnd) = tenons[i];
            verts.Add(Vertex(0, tEnd));
            verts.Add(Vertex(-dd, tEnd));
            verts.Add(Vertex(-dd, tStart));
            verts.Add(Vertex(0, tStart));
        }
        verts.Add(Vertex(0, blindStart));

        return verts;
    }

    internal static List<Vector2> TenonTopAndBottom(
    double length, double height, LockDadoSettings s, bool forceTwoTenons = false)
    {
        double dadoDepth = s.DadoDepth;
        double blindStart = s.BlindStart;
        double blindEnd = length - s.BlindStop;
        var tenons = ComputeTenonRanges(length, s, forceTwoTenons);
        var verts = new List<Vector2>();

        // Bottom edge (Y=0): left → right, comb in usable zone
        verts.Add(Vertex(0, 0));
        verts.Add(Vertex(blindStart, 0));   // protrude bottom

        foreach (var (tStart, tEnd) in tenons)
        {
            verts.Add(Vertex(tStart, 0));
            verts.Add(Vertex(tStart, -dadoDepth));      // retract to edge
            verts.Add(Vertex(tEnd, -dadoDepth));
            verts.Add(Vertex(tEnd, 0));       // protrude bottom again
        }

        verts.Add(Vertex(blindEnd, 0));
        verts.Add(Vertex(length, 0));

        // Right edge: go up to top-right corner
        verts.Add(Vertex(length, height));

        // Top edge (traverse right → left): comb in usable zone (reverse order)
        verts.Add(Vertex(blindEnd, height));   // first usable boundary from the right

        for (int i = tenons.Count - 1; i >= 0; i--)
        {
            var (tStart, tEnd) = tenons[i];
            verts.Add(Vertex(tEnd, height));
            verts.Add(Vertex(tEnd, height + dadoDepth));      // retract to edge (towards outside)
            verts.Add(Vertex(tStart, height + dadoDepth));
            verts.Add(Vertex(tStart, height));       // back to top face
        }

        verts.Add(Vertex(blindStart, height));
        verts.Add(Vertex(0, height));

        // Left edge: will naturally close back to the starting bottom-left vertex

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
        LockDadoSettings s,
        double xOffset = 0,
        bool forceTwoTenons = false)
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

        var tenons = ComputeTenonRanges(partDepth, s, forceTwoTenons);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double x1 = Math.Max(tStart - s.MortiseOversize, usableStart) + xOffset;
            double x2 = Math.Min(tEnd + s.MortiseOversize, usableEnd) + xOffset;
            pockets.Add((x1, x2, slotBottomY, slotTopY));
        }

        return pockets;
    }


    // ── Mortise pockets: height-direction joint (toekick) ─────────────────────

    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeHeightDirectionMortisePockets(
        double edgeLength,
        double xPosition,
        double bottomY,
        TenonFlushFace flushFace,
        LockDadoSettings s,
        bool forceTwoTenons = false)
    {
        double slotX1 = flushFace switch
        {
            TenonFlushFace.Back => xPosition - s.TenonThickness,
            TenonFlushFace.InteriorFront => xPosition,
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        double slotX2 = flushFace switch
        {
            TenonFlushFace.Back => xPosition + s.TenonClearance,
            TenonFlushFace.InteriorFront => xPosition + s.MortiseSlotHeight,
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        var tenons = ComputeTenonRanges(edgeLength, s, forceTwoTenons);
        var pockets = new List<(double, double, double, double)>(tenons.Count);

        foreach (var (tStart, tEnd) in tenons)
        {
            double y1 = bottomY + Math.Max(tStart - s.MortiseOversize, s.BlindStart);
            double y2 = bottomY + Math.Min(tEnd + s.MortiseOversize, edgeLength - s.BlindStop);
            pockets.Add((slotX1, slotX2, y1, y2));
        }

        return pockets;
    }

    // ── Screw pilot holes: depth-direction joints ─────────────────────────────

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeDepthDirectionScrewHoles(
        double partDepth,
        double mortiseBottomY,
        TenonFlushFace flushFace,
        LockDadoSettings s,
        double xOffset = 0,
        bool forceTwoTenons = false)
    {
        double mt34 = MaterialDefaults.Thickness34;
        double slotBottomY = flushFace switch
        {
            TenonFlushFace.Top => mortiseBottomY + (mt34 - s.TenonThickness),
            _ => mortiseBottomY,
        };
        double holeCenterY = slotBottomY + (s.MortiseSlotHeight / 2.0);

        var tenons = ComputeTenonRanges(partDepth, s, forceTwoTenons);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterX = (tenons[i].EndY + tenons[i + 1].StartY) / 2.0 + xOffset;
            holes.Add((gapCenterX, holeCenterY, s.ScrewPilotHoleDiameter));
        }

        return holes;
    }


    // ── Screw pilot holes: height-direction joint (toekick) ───────────────────

    internal static List<(double CenterX, double CenterY, double Diameter)> ComputeHeightDirectionScrewHoles(
        double edgeLength,
        double xPosition,
        double bottomY,
        TenonFlushFace flushFace,
        LockDadoSettings s,
        bool forceTwoTenons = false)
    {
        double holeCenterX = flushFace switch
        {
            TenonFlushFace.Back => xPosition - (s.TenonThickness / 2.0),
            TenonFlushFace.InteriorFront => xPosition + (s.MortiseSlotHeight / 2.0),
            _ => throw new ArgumentOutOfRangeException(nameof(flushFace), flushFace, "Height-direction joints must use Back or InteriorFront.")
        };

        var tenons = ComputeTenonRanges(edgeLength, s, forceTwoTenons);
        var holes = new List<(double, double, double)>();

        for (int i = 0; i < tenons.Count - 1; i++)
        {
            double gapCenterY = bottomY + (tenons[i].EndY + tenons[i + 1].StartY) / 2.0;
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
        double dd = s.DadoDepth + 0.03937;

        var leftPocket = (X1: -dd, X2: 0.0, Y1: pocketY1, Y2: pocketY2);
        var rightPocket = (X1: length, X2: length + dd, Y1: pocketY1, Y2: pocketY2);

        return [leftPocket, rightPocket];
    }

    // ── Core tenon layout algorithm ───────────────────────────────────────────

    internal static List<(double StartY, double EndY)> ComputeTenonRanges(
        double edgeLength, LockDadoSettings s, bool forceTwoTenons = false)
    {
        double usableStart = s.BlindStart;
        double usableEnd = edgeLength - s.BlindStop;
        double usableLength = usableEnd - usableStart;


        int gapCount = s.GapCount(edgeLength);
        int tenonCount = gapCount + 1;
        double totalGapLen = gapCount * s.GapWidth;
        double tenonWidth = (usableLength - totalGapLen) / tenonCount;

        // DrawerStretcher / TopStretcherFront always get exactly 2 tenons with
        // the screw-access gap centred in the usable zone.
        if (forceTwoTenons)
        {
            tenonWidth = (usableLength - s.GapWidth) / 2.0;
            return
            [
                (usableStart,                    usableStart + tenonWidth),
                (usableStart + tenonWidth + s.GapWidth, usableEnd),
            ];
        }

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