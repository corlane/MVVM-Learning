using CorlaneCabinetOrderFormV3.Models;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// DXF document creation and entity addition helpers for DxfExporter.
/// 
/// Manages DXF document setup, layer configuration, and geometric/text entity creation.
/// All exported parts use these helpers to build consistent, machine-ready DXF files.
/// 
/// Document Setup:
///   CreateDocument()
///     Initializes a new DxfDocument with all required CNC layers pre-configured
///     with specific ACI colors and naming. Colors aid visual differentiation in CAM software.
///     Layers are indexed by name constant (e.g., LayerOutline) for consistent entity placement.
/// 
/// Geometric Primitives:
///   AddClosedPolyline()
///     Renders part boundaries as closed 2D polylines. Used for:
///       • Part outline (PART_OUTLINE layer)
///       • End panel outlines with toekick notches
///     Vertices provided as List<Vector2> in inches.
///   
///   AddRectangle()
///     Renders axis-aligned pockets as closed 4-vertex polylines. Used for:
///       • Tenon thinning pockets (MACHINING_TENON_POCKET layer)
///       • Mortise pockets (MACHINING_MORTISE layer)
///     Coordinates: x1, x2 (X bounds), y1, y2 (Y bounds) in inches.
///   
///   AddCircle()
///     Renders screw pilot holes as circles. Used for:
///       • Screw holes on end panels (MACHINING_SCREW_HOLES layer)
///     Center and radius provided in inches.
/// 
/// Annotation:
///   AddGrainArrow()
///     Renders dashed line along X-axis midpoint (10%–90% of length).
///     Indicates grain direction for woodworker reference.
///   
///   AddLabels()
///     Renders stacked text annotations below part geometry:
///       • Part name × quantity (0.5" text height)
///       • Dimensions "L\" × W\" × T\"" (0.4" height)
///       • Material species (0.35" height)
///       • Edge band species + length (if present) (0.35" height)
///       • Notes (if present) (0.3" height)
///     Text positioned at X=0, starting Y=-1.2", decreasing by label height.
/// 
/// Layer Color Reference (ACI):
///   • 7 (white)  → PART_OUTLINE
///   • 1 (red)    → MACHINING_TENON_POCKET, MACHINING_MORTISE
///   • 4 (cyan)   → MACHINING_SCREW_HOLES
///   • 3 (green)  → GRAIN_DIRECTION
///   • 2 (yellow) → LABELS
/// </summary>

internal static partial class DxfExporter
{
    private static DxfDocument CreateDocument()
    {
        var doc = new DxfDocument();
        AddLayer(doc, LayerOutline, new AciColor(7));   // white
        AddLayer(doc, LayerTenonThinningPocket, new AciColor(1));   // red
        AddLayer(doc, LayerMortise, new AciColor(1));   // red
        AddLayer(doc, LayerThruHoles, new AciColor(4));   // cyan
        AddLayer(doc, LayerShelfHoles, new AciColor(4));   // cyan
        AddLayer(doc, LayerDrawerSlideHoles, new AciColor(4));   // cyan
        AddLayer(doc, LayerHingeHoles, new AciColor(4));   // cyan
        AddLayer(doc, LayerGrain, new AciColor(3));   // green
        AddLayer(doc, LayerLabels, new AciColor(2));   // yellow
        AddLayer(doc, LayerMortiseThru, new AciColor(1));   // red
        return doc;
    }

    private static void AddLayer(DxfDocument doc, string name, AciColor color)
        => doc.Layers.Add(new Layer(name) { Color = color });

    private static void AddClosedPolyline(DxfDocument doc, string layerName, List<Vector2> verts)
    {
        var poly = new Polyline2D(verts) { IsClosed = true, Layer = doc.Layers[layerName] };
        doc.Entities.Add(poly);
    }

    private static void AddRectangle(
        DxfDocument doc, string layerName,
        double x1, double x2, double y1, double y2)
    {
        var verts = new List<Vector2>
        {
            new((float)x1, (float)y1),
            new((float)x2, (float)y1),
            new((float)x2, (float)y2),
            new((float)x1, (float)y2),
        };
        var poly = new Polyline2D(verts) { IsClosed = true, Layer = doc.Layers[layerName] };
        doc.Entities.Add(poly);
    }

    private static void AddCircle(
        DxfDocument doc, string layerName,
        double centerX, double centerY, double radius)
    {
        doc.Entities.Add(new Circle(new Vector3((float)centerX, (float)centerY, 0), radius)
        {
            Layer = doc.Layers[layerName],
        });
    }

    private static void AddGrainArrow(DxfDocument doc, double length, double depth)
    {
        // Grain ALWAYS runs along X axis (left-to-right).
        var start = new Vector3((float)(length * 0.1), (float)(depth / 2.0), 0);
        var end = new Vector3((float)(length * 0.9), (float)(depth / 2.0), 0);

        doc.Entities.Add(new Line(start, end)
        {
            Layer = doc.Layers[LayerGrain],
            Linetype = Linetype.Dashed,
        });
    }

    private static void AddLabels(DxfDocument doc, PartListEntry part)
    {
        var layer = doc.Layers[LayerLabels];
        double y = -1.2;

        AddText(doc, layer, $"{part.PartName}  ×{part.Qty}", 0, y, 0.5);
        AddText(doc, layer, $"{part.Length}\" × {part.Width}\" × {part.Thickness}\"", 0, y - 0.9, 0.4);
        AddText(doc, layer, $"Material: {part.Species}", 0, y - 1.7, 0.35);

        if (!string.IsNullOrWhiteSpace(part.EdgeBandSpecies))
            AddText(doc, layer, $"EB: {part.EdgeBandSpecies}  {part.EdgeBandLength}\"", 0, y - 2.4, 0.35);

        if (!string.IsNullOrWhiteSpace(part.Notes))
            AddText(doc, layer, $"Notes: {part.Notes}", 0, y - 3.1, 0.3);
    }

    private static void AddText(
        DxfDocument doc, Layer layer,
        string text, double x, double y, double height)
    {
        doc.Entities.Add(new Text(text, new Vector3((float)x, (float)y, 0), height)
        {
            Layer = layer,
        });
    }
}