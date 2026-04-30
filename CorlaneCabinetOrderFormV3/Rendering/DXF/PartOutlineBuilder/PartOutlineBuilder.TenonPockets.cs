namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Tenon thinning pocket computation for CNC machining.
/// 
/// Generates rectangular pockets on the flat face of cabinet parts surrounding tenon protrusions.
/// Thinning pockets reduce material weight, improve visual appearance, and facilitate CNC routing.
/// Results are rendered to the MACHINING_TENON_POCKET DXF layer for standard parts.
/// 
/// Purpose of Tenon Thinning Pockets:
///   • Weight reduction: removes material around tenon geometry (non-structural zones)
///   • Aesthetics: softens the appearance of thick edge tenons on visible surfaces
///   • CNC efficiency: single routing pass around perimeter (faster than multi-step cutting)
///   • Joinery accommodation: creates recessed areas for edge banding or adjacent components
/// 
/// Pocket Geometry:
///   ComputeTenonThinningPockets()
///     Returns axis-aligned rectangular pockets for each tenon-bearing edge.
///     
///     Algorithm:
///       1. For each edge direction (Left, Right, Top, Bottom):
///          • Check if that edge has tenons (via EdgeDesignators flags)
///          • Determine pocket X/Y bounds based on blind pockets and oversize margin
///       
///       2. Left/Right edges (vertical tenons):
///          • Pocket height spans entire depth minus blind pockets
///          • pocketY1 = s.BlindStart - s.TenonPocketOversize (bottom margin)
///          • pocketY2 = depth - s.BlindStop + s.TenonPocketOversize (top margin)
///          • Pocket width = DadoDepth + 0.03937 (tenon protrusion + margin)
///          • Left pocket: X-range = (-dd, 0.0) — extends left from panel edge
///          • Right pocket: X-range = (length, length + dd) — extends right from panel edge
///       
///       3. Top/Bottom edges (horizontal tenons):
///          • Pocket width spans entire length minus blind pockets
///          • pocketX1 = s.BlindStart - s.TenonPocketOversize (left margin)
///          • pocketX2 = length - s.BlindStop + s.TenonPocketOversize (right margin)
///          • Pocket height = DadoDepth + 0.03937
///          • Bottom pocket: Y-range = (-dd, 0.0) — extends below panel edge
///          • Top pocket: Y-range = (depth, depth + dd) — extends above panel edge
///       
///     Output: List of (X1, X2, Y1, Y2) tuples
///       X1, X2: horizontal bounds (left to right, X1 < X2)
///       Y1, Y2: vertical bounds (bottom to top, Y1 < Y2)
///       Each tuple represents one axis-aligned rectangular pocket
/// 
/// Key Constants:
///   s.DadoDepth
///     Depth of tenon protrusion (typically 0.75"–1.0").
///     Pocket extends this distance outward from panel edge.
///   
///   s.BlindStart / s.BlindStop
///     Distances from edge ends where tenons begin/end (blind pocket spacing).
///     Prevents pockets from extending into inside corner joints.
///     Applied as margin: pocketY1 = BlindStart - TenonPocketOversize (extends beyond blind)
///   
///   s.TenonPocketOversize
///     Additional margin extending beyond the tenon protrusion.
///     Provides clearance for tool diameter and ensures full material removal.
///     Typical value: 0.125" (1/8")
///   
///   0.03937 constant
///     Unit conversion factor (1mm ≈ 0.03937 inches, or ~1/25.4).
///     Adds ~1mm (0.03937") to DadoDepth for CNC tool tip compensation.
///     Ensures material fully removed around tenon protrusion with tool deflection.
/// 
/// Coordinate System:
///   Origin (0, 0) at panel bottom-left.
///   Left pocket: extends to negative X (off panel)
///   Right pocket: extends to positive X (off panel)
///   Bottom pocket: extends to negative Y (off panel)
///   Top pocket: extends to positive Y (off panel)
///   Pockets are oriented outward from panel edges (not inward cutting).
/// 
/// Edge Independence:
///   Each edge direction is processed independently.
///   Panel can have pockets on any combination of edges (Left, Right, Top, Bottom).
///   No overlap checks — pockets on perpendicular edges do not intersect
///   (corners handled by panel geometry, not pocket logic).
/// 
/// Integration:
///   Used by ExportPart() to populate MACHINING_TENON_POCKET layer.
///   Each returned rectangle rendered as a closed polyline via AddRectangle().
///   Converted to CNC tool paths by CAM software (Aspire, VCarve, SheetCAM, etc.).
/// </summary>

internal static partial class PartOutlineBuilder
{
    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeTenonThinningPockets(
        double length, double depth, LockDadoSettings s,
        EdgeDesignators tenonEdges = EdgeDesignators.None, bool forceTwoTenons = false)
    {
        var pockets = new List<(double, double, double, double)>();
        double dd = s.DadoDepth + 0.03937;

        if (tenonEdges.HasFlag(EdgeDesignators.Left) || tenonEdges.HasFlag(EdgeDesignators.Right))
        {
            double pocketY1 = s.BlindStart - s.TenonPocketOversize;
            double pocketY2 = depth - s.BlindStop + s.TenonPocketOversize;

            if (tenonEdges.HasFlag(EdgeDesignators.Left))
                pockets.Add((-dd, 0.0, pocketY1, pocketY2));

            if (tenonEdges.HasFlag(EdgeDesignators.Right))
                pockets.Add((length, length + dd, pocketY1, pocketY2));
        }

        if (tenonEdges.HasFlag(EdgeDesignators.Top) || tenonEdges.HasFlag(EdgeDesignators.Bottom))
        {
            double pocketX1 = s.BlindStart - s.TenonPocketOversize;
            double pocketX2 = length - s.BlindStop + s.TenonPocketOversize;

            if (tenonEdges.HasFlag(EdgeDesignators.Bottom))
                pockets.Add((pocketX1, pocketX2, -dd, 0.0));

            if (tenonEdges.HasFlag(EdgeDesignators.Top))
                pockets.Add((pocketX1, pocketX2, depth, depth + dd));
        }

        return pockets;
    }
}