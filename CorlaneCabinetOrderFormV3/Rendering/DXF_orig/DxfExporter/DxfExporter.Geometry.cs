using netDxf;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Geometry transformation and part classification helpers for DxfExporter.
/// 
/// Handles coordinate system conversions and part type resolution to route parts
/// to correct export pipelines with appropriate tenon/mortise configurations.
/// 
/// Part Classification:
///   DxfPartKind enum categorizes parts by joinery requirements:
///     • Plain               — no tenons (fallback for unlisted parts)
///     • TenonLeftAndRight   — tenons on left/right edges (Deck, Top, Nailer, etc.)
///     • TenonTopAndBottom   — tenons on top/bottom edges (Back)
///     • TenonTopLeftRight   — tenons on top, left, right edges (Toekick variants)
///     • MortisePanel        — mortise pockets + screw holes on end panels (Left/Right End)
///   
///   ResolvePartKind()
///     Maps PartListEntry.PartName to DxfPartKind via pattern matching.
///     Used by ExportPart() to select outline builder and tenon edge designators,
///     and by ExportAll() to route end panels to ExportEndPanel() for full mortise geometry.
///     Handles part name variants (e.g., "Drawer Stretcher" prefix matching).
/// 
/// Coordinate Transformations (for end panels):
///   Original end panel axes:
///     • X = cabinet depth (left-to-right)
///     • Y = cabinet height (bottom-to-top, grain direction)
///   
///   After export transformations (so grain runs horizontally in DXF):
///     • CW90 rotation + optional X-axis mirroring
///     • Grain now runs along X axis (left-to-right)
///     • Mortises and holes repositioned to match rotated panel orientation
///   
///   Transformation Helpers:
///     RotateCW90(pts, oldWidth)
///       Rotates polyline vertices 90° clockwise.
///       Formula: (x, y) → (y, oldWidth - x)
///       Keeps geometry in positive quadrant; oldWidth = original X-extent (cabinet depth).
///   
///     RotatePocketCW90(x1, x2, y1, y2, oldWidth)
///       Rotates axis-aligned rectangle bounds for mortise pockets.
///       Formula: (x1, x2, y1, y2) → (y1, y2, oldWidth - x2, oldWidth - x1)
///   
///     RotateHoleCW90(cx, cy, dia, oldWidth)
///       Rotates screw hole center point; diameter unchanged.
///       Formula: (cx, cy) → (cy, oldWidth - cx)
///   
///     MirrorX(pts, panelLength)
///       Mirrors polyline across X-axis midpoint (for Left End panels).
///       Formula: x' = panelLength - x
///       Ensures Left End mortises/holes mirror Right End geometry.
/// 
/// Utilities:
///   SanitizeFileName()
///     Removes invalid filename characters (from Path.GetInvalidFileNameChars()).
///     Enables safe export of cabinet labels containing special characters (e.g., em-dashes, accents).
/// </summary>

internal static partial class DxfExporter
{
    // ── Part classification ───────────────────────────────────────────────────

    private enum DxfPartKind { Plain, TenonLeftAndRight, TenonTopAndBottom, TenonTopLeftRight, MortisePanel }

    private static DxfPartKind ResolvePartKind(string partName)
    {
        if (partName is "Left End" or "Right End")
            return DxfPartKind.MortisePanel;

        if (partName is "Toekick" or "Toekick (Left)" or "Toekick (Right)")
            return DxfPartKind.TenonTopLeftRight;

        if (partName is "Deck"
                     or "Top"
                     or "Top Stretcher (Front)"
                     or "Nailer"
                     or "Sink Stretcher"
            || partName.StartsWith("Drawer Stretcher", StringComparison.OrdinalIgnoreCase))
            return DxfPartKind.TenonLeftAndRight;

        if (partName is "Back")
            return DxfPartKind.TenonTopAndBottom;

        return DxfPartKind.Plain;
    }

    // ── CW 90° rotation helpers (keeps geometry in positive quadrant) ──────────
    // Transform: (x, y) → (y, oldWidth - x), where oldWidth = original X-extent.

    private static List<Vector2> RotateCW90(List<Vector2> pts, double oldWidth)
        => pts.Select(p => new Vector2(p.Y, (float)oldWidth - p.X)).ToList();

    private static (double X1, double X2, double Y1, double Y2) RotatePocketCW90(
        double x1, double x2, double y1, double y2, double oldWidth)
        => (y1, y2, oldWidth - x2, oldWidth - x1);

    private static (double CX, double CY, double Dia) RotateHoleCW90(
        double cx, double cy, double dia, double oldWidth)
        => (cy, oldWidth - cx, dia);

    private static List<Vector2> MirrorX(List<Vector2> pts, double panelLength)
        => pts.Select(p => new Vector2((float)(panelLength - p.X), p.Y)).ToList();

    // ── Filename helper ───────────────────────────────────────────────────────

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}