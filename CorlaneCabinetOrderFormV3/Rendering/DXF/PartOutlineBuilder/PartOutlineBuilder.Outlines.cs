using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Part outline geometry generation for DXF export (polyline vertices).
/// 
/// Generates closed polyline vertex lists for cabinet parts with optional tenon joinery.
/// These outlines are rendered to the PART_OUTLINE DXF layer for CNC cutting.
/// 
/// Basic Outlines:
///   Rectangle()
///     Simple rectangular outline (length × depth).
///     Used for non-joinery parts (e.g., mortise panels exported as plain rectangles).
///     Vertices: (0,0) → (length,0) → (length,depth) → (0,depth) [closed]
///   
///   EndPanelWithToeKick()
///     End panel outline with rectangular toekick notch at bottom.
///     Creates a recessed opening where toekick component mounts into the end panel.
///     
///     Notch geometry:
///       • Position: inset 3" from left/right edges
///       • Height: tkDepth (toekick thickness)
///       • Depth: tkHeight (toekick height below panel bottom)
///       • Fillet: small 0.5" radius at notch corners (simplified as Y-step)
///     
///     Vertices trace: right side (full height) → top-left (full height) →
///       left side (to notch top) → notch left corner → notch bottom-left →
///       notch bottom-center → notch bottom-right → notch right corner → right side (to notch)
///   
/// Tenon Outlines (Primary Method):
///   BuildPanelWithTenons()
///     Renders part perimeter with tenon protrusions on specified edges.
///     Tenons extend outward from panel edges to mate with mortises on adjacent parts.
///     
///     Inputs:
///       • length, depth      — panel dimensions (inches)
///       • LockDadoSettings s — blind depths, gap widths, DadoDepth (tenon protrusion)
///       • tenonEdges         — EdgeDesignators flags (Bottom, Right, Top, Left)
///       • forceTwoTenons     — override tenon count to exactly 2 (Toekick, Top Stretcher)
///     
///     Algorithm (traces perimeter counter-clockwise from origin):
///       1. Start at (0, 0) — bottom-left corner
///       
///       2. Bottom edge (if tenonEdges.Bottom):
///          • Compute tenon ranges via ComputeTenonRanges()
///          • For each tenon: add protrusion vertices extending downward (negative Y)
///          • Tenon protrusion: (tStart, 0) → (tStart, -DadoDepth) → (tEnd, -DadoDepth) → (tEnd, 0)
///          • Skip blind pockets at edges (BlindStart, BlindStop)
///       
///       3. Right edge (if tenonEdges.Right):
///          • Similar logic, but tenons extend rightward (positive X)
///          • Protrusion: (length, tStart) → (length+DadoDepth, tStart) → ... → (length, tEnd)
///       
///       4. Top edge (if tenonEdges.Top):
///          • Traverse tenon list in REVERSE to maintain winding order
///          • Tenons extend upward (positive Y)
///          • Protrusion: (tEnd, depth) → (tEnd, depth+DadoDepth) → ... → (tStart, depth)
///       
///       5. Left edge (if tenonEdges.Left):
///          • Traverse tenon list in REVERSE
///          • Tenons extend leftward (negative X)
///          • Protrusion: (0, tEnd) → (-DadoDepth, tEnd) → ... → (0, tStart)
///       
///       6. Return closed path (final vertex connects back to start)
///     
///     Tenon Geometry Detail:
///       • Each tenon is a rectangular protrusion with 4 vertices
///       • Depth: s.DadoDepth (typically 0.75"–1.0")
///       • Blind pockets prevent tenons from reaching panel edges
///         (e.g., no tenon overlap at inside corner joints)
///       • Edge-specific blind depths via s.ResolveBlindStart/Stop(EdgeDesignators)
///     
///     Coordinate System:
///       • Origin (0, 0) at bottom-left
///       • X: left-to-right (0 to length)
///       • Y: bottom-to-top (0 to depth)
///       • Tenons extend beyond panel bounds (negative X/Y or beyond length/depth)
///       • Closed polyline (last vertex implicitly connects to first)
///     
///     Winding Order:
///       Counter-clockwise when viewed from front (Z facing viewer).
///       Top/Left edges traversed in reverse to maintain consistent winding
///       (prevents DXF CAM software from misinterpreting polygon direction).
/// 
/// Placeholder:
///   MortisePanel()
///     Currently returns Rectangle() — reserved for future mortise panel outlines
///     (currently end panels use ExportEndPanel() for full mortise geometry).
/// </summary>

internal static partial class PartOutlineBuilder
{
    internal static List <Vector2> Rectangle(double length, double depth) =>
    [
        Vertex(0, 0), Vertex(length, 0),
        Vertex(length, depth), Vertex(0, depth)
    ];

    internal static List <Vector2> EndPanelWithToeKick(double depth, double height, double tkHeight, double tkDepth)
    {
        return
        [
            Vertex(depth, tkHeight), Vertex(depth, height),
            Vertex(0, height), Vertex(0, 0),
            Vertex(3, 0), Vertex(3, 0.5),
            Vertex(depth - tkDepth - 3, 0.5), Vertex(depth - tkDepth - 3, 0),
            Vertex(depth - tkDepth, 0), Vertex(depth - tkDepth, tkHeight)
        ];
    }

    internal static List <Vector2> BuildPanelWithTenons(
        double length, double depth, LockDadoSettings s,
        EdgeDesignators tenonEdges = EdgeDesignators.None, bool forceTwoTenons = false)
    {
        double dd = s.DadoDepth;
        double blindStart = s.BlindStart;
        double blindEnd = depth - s.BlindStop;
        var verts = new List<Vector2>();

        verts.Add(Vertex(0, 0));
        if (tenonEdges.HasFlag(EdgeDesignators.Bottom))
        {
            var tenons = ComputeTenonRanges(length, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Bottom), s.ResolveBlindStop(EdgeDesignators.Bottom));
            verts.Add(Vertex(blindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                verts.Add(Vertex(tStart, 0));
                verts.Add(Vertex(tStart, -dd));
                verts.Add(Vertex(tEnd, -dd));
                verts.Add(Vertex(tEnd, 0));
            }
        }
        verts.Add(Vertex(length, 0));

        if (tenonEdges.HasFlag(EdgeDesignators.Right))
        {
            var tenons = ComputeTenonRanges(depth, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Right), s.ResolveBlindStop(EdgeDesignators.Right));
            foreach (var (tStart, tEnd) in tenons)
            {
                verts.Add(Vertex(length, tStart));
                verts.Add(Vertex(length + dd, tStart));
                verts.Add(Vertex(length + dd, tEnd));
                verts.Add(Vertex(length, tEnd));
            }
            verts.Add(Vertex(length, blindEnd));
        }

        verts.Add(Vertex(length, depth));
        if (tenonEdges.HasFlag(EdgeDesignators.Top))
        {
            var tenons = ComputeTenonRanges(length, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Top), s.ResolveBlindStop(EdgeDesignators.Top));
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                verts.Add(Vertex(tEnd, depth));
                verts.Add(Vertex(tEnd, depth + dd));
                verts.Add(Vertex(tStart, depth + dd));
                verts.Add(Vertex(tStart, depth));
            }
            verts.Add(Vertex(blindStart, depth));
        }
        verts.Add(Vertex(0, depth));

        if (tenonEdges.HasFlag(EdgeDesignators.Left))
        {
            var tenons = ComputeTenonRanges(depth, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Left), s.ResolveBlindStop(EdgeDesignators.Left));
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                verts.Add(Vertex(0, tEnd));
                verts.Add(Vertex(-dd, tEnd));
                verts.Add(Vertex(-dd, tStart));
                verts.Add(Vertex(0, tStart));
            }
            verts.Add(Vertex(0, blindStart));
        }

        return verts;
    }

    internal static List<Vector2> MortisePanel(double length, double depth) => Rectangle(length, depth);
}