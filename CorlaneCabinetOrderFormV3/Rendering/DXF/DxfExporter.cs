using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Exports one DXF file per cabinet part, with separate CNC layers for:
///   PART_OUTLINE           — closed cut boundary
///   MACHINING_TENON_POCKET — thinning pocket on tenon-bearing panels
///   MACHINING_MORTISE      — discrete mortise pockets on end panels
///   MACHINING_SCREW_HOLES  — CNC pilot holes on end panels
///   GRAIN_DIRECTION        — dashed centerline arrow
///   LABELS                 — part name, species, thickness, EB info
/// </summary>
internal static class DxfExporter
{
    // ── Layer name constants ──────────────────────────────────────────────────

    private const string LayerOutline = "outline z18p6";
    private const string LayerTenonPocket = "pocket z9p0";
    private const string LayerMortise = "pocket z6p35";
    private const string LayerScrewHoles = "drill z12p0";
    private const string LayerGrain = "GRAIN_DIRECTION";
    private const string LayerLabels = "LABELS";

    // ── Public entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Exports all parts for every cabinet in the job to individual DXF files
    /// in <paramref name="outputFolder"/>. End panels on base standard cabinets
    /// get full mortise and pilot-hole geometry; all other parts get tenon
    /// outlines or plain rectangles as appropriate.
    /// </summary>
    internal static void ExportAll(
        string outputFolder,
        IEnumerable<CabinetModel> cabinets,
        LockDadoSettings? joinery = null)
    {
        var s = joinery ?? LockDadoSettings.Default;
        Directory.CreateDirectory(outputFolder);

        int index = 1;
        foreach (var cab in cabinets)
        {
            string label = PartsListBuilder.FormatLabel(cab, index++);
            var parts = PartsListBuilder.BuildForCabinet(cab, label);

            // Build mortise specs once per cabinet (only applicable to base standard)
            List<MortiseSpec>? mortiseSpecs = null;
            BaseCabinetDimensions dim = default;
            if (cab is BaseCabinetModel baseCab &&
                (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase)))
            {
                dim = BaseCabinetDimensions.From(baseCab);
                mortiseSpecs = MortiseSpecBuilder.BuildForBaseStandard(baseCab, dim, s);
            }

            foreach (var part in parts)
            {
                string safeName = SanitizeFileName($"{label} — {part.PartName}");
                string path = Path.Combine(outputFolder, safeName + ".dxf");

                var kind = ResolvePartKind(part.PartName);

                if (kind == DxfPartKind.MortisePanel && mortiseSpecs is not null) // Add mortieses 
                    ExportEndPanel(path, part, mortiseSpecs, s,
                        tkHeight: dim.TKHeight,
                        tkDepth: dim.TKDepth);
                else
                    ExportPart(path, part, s); // Add Tenons or plain outline as appropriate
            }
        }
    }

    /// <summary>
    /// Exports all parts (already built) to individual DXF files.
    /// End panels are exported as plain rectangles — use the CabinetModel
    /// overload above to get full mortise geometry on end panels.
    /// </summary>
    internal static void ExportAll(
        string outputFolder,
        IEnumerable<PartListEntry> parts,
        LockDadoSettings? joinery = null)
    {
        var s = joinery ?? LockDadoSettings.Default;
        Directory.CreateDirectory(outputFolder);

        foreach (var part in parts)
        {
            string safeName = SanitizeFileName($"{part.CabinetLabel} — {part.PartName}");
            string path = Path.Combine(outputFolder, safeName + ".dxf");
            ExportPart(path, part, s);
        }
    }

    /// <summary>Exports a single part to a DXF file.</summary>
    internal static void ExportPart(
        string filePath,
        PartListEntry part,
        LockDadoSettings? joinery = null)
    {
        var s = joinery ?? LockDadoSettings.Default;
        var doc = CreateDocument();
        double length = part.LengthIn;
        double depth = part.WidthIn;
        var kind = ResolvePartKind(part.PartName);

        // ── Resolve part-specific customizations ──────────────────────────────
        var (tenonEdges, effectiveSettings, forceTwoTenons) = part.PartName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" =>
                (EdgeDesignators.Top | EdgeDesignators.Left | EdgeDesignators.Right,
                 s with { BlindStart = 0, BlindStop = 0 },
                 true),
            "Top Stretcher (Front)" or "Drawer Stretcher" =>
                (EdgeDesignators.LeftRight, s with { BlindStart = 1.25, BlindStop = 1.25 }, true),
            "Back" =>
                (EdgeDesignators.TopBottom, s, false),
            "Deck" =>
                (EdgeDesignators.LeftRight, s, false),
            _ => (EdgeDesignators.None, s, false),
        };

        // ── Outline ───────────────────────────────────────────────────────────
        var outline = kind switch
        {
            DxfPartKind.MortisePanel => PartOutlineBuilder.Rectangle(length, depth),
            DxfPartKind.TenonLeftAndRight => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            DxfPartKind.TenonTopAndBottom => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            DxfPartKind.TenonTopLeftRight => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            _ => PartOutlineBuilder.Rectangle(length, depth),
        };
        AddClosedPolyline(doc, LayerOutline, outline);

        // ── Tenon thinning pockets ────────────────────────────────────────────
        foreach (var (x1, x2, y1, y2) in PartOutlineBuilder.ComputeTenonThinningPockets(length, depth, effectiveSettings, tenonEdges, forceTwoTenons))
            AddRectangle(doc, LayerTenonPocket, x1, x2, y1, y2);



        AddGrainArrow(doc, length, depth); AddLabels(doc, part);

        doc.Save(filePath);
    }

    /// <summary>
    /// Exports an end panel with full mortise pocket and pilot hole geometry.
    /// Call this instead of ExportPart() for Left End / Right End parts,
    /// passing the MortiseSpec list from MortiseSpecBuilder.BuildForBaseStandard().
    /// </summary>
    internal static void ExportEndPanel(
        string filePath,
        PartListEntry part,
        IEnumerable<MortiseSpec> mortiseSpecs,
        LockDadoSettings? joinery = null,
        double tkHeight = 0,
        double tkDepth = 0)
    {
        var s = joinery ?? LockDadoSettings.Default;
        var doc = CreateDocument();

        // End panel original coords: X = cabinet depth, Y = cabinet height.
        // Rotate 90° CW so height (grain) runs along X axis.
        double oldWidth = part.WidthIn;   // original X-extent = cabinet depth

        // After CW90: new X-extent = part.LengthIn (height), new Y-extent = part.WidthIn (depth)
        double newLength = part.LengthIn;
        double newDepth = part.WidthIn;

        bool isLeft = part.PartName == "Left End";

        // Outline — notched if toekick present, otherwise plain rectangle
        var rawOutline = tkHeight > 0
            ? PartOutlineBuilder.EndPanelWithToeKick(part.WidthIn, part.LengthIn, tkHeight, tkDepth)
            : PartOutlineBuilder.Rectangle(part.WidthIn, part.LengthIn);
        var rotatedOutline = RotateCW90(rawOutline, oldWidth);
        if (isLeft) rotatedOutline = MirrorX(rotatedOutline, newLength);
        AddClosedPolyline(doc, LayerOutline, rotatedOutline);

        // Mortise pockets and pilot holes — rotate (and mirror for Left End) to match
        foreach (var spec in mortiseSpecs)
        {
            foreach (var (x1, x2, y1, y2) in spec.Pockets)
            {
                var (rx1, rx2, ry1, ry2) = RotatePocketCW90(x1, x2, y1, y2, oldWidth);
                if (isLeft) (rx1, rx2) = (newLength - rx2, newLength - rx1);
                AddRectangle(doc, LayerMortise, rx1, rx2, ry1, ry2);
            }

            foreach (var (cx, cy, dia) in spec.ScrewHoles)
            {
                var (rcx, rcy, _) = RotateHoleCW90(cx, cy, dia, oldWidth);
                if (isLeft) rcx = newLength - rcx;
                AddCircle(doc, LayerScrewHoles, rcx, rcy, dia / 2.0);
            }
        }

        AddGrainArrow(doc, newLength, newDepth);
        AddLabels(doc, part);

        doc.Save(filePath);
    }

    // ── DXF helpers ───────────────────────────────────────────────────────────

    private static DxfDocument CreateDocument()
    {
        var doc = new DxfDocument();
        AddLayer(doc, LayerOutline, new AciColor(7));   // white
        AddLayer(doc, LayerTenonPocket, new AciColor(1));   // red
        AddLayer(doc, LayerMortise, new AciColor(1));   // red
        AddLayer(doc, LayerScrewHoles, new AciColor(4));   // cyan
        AddLayer(doc, LayerGrain, new AciColor(3));   // green
        AddLayer(doc, LayerLabels, new AciColor(2));   // yellow
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

    // ── Filename helper ───────────────────────────────────────────────────────

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}