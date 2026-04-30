using CorlaneCabinetOrderFormV3.Models;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Part export methods for DxfExporter.
/// 
/// Responsible for DXF file generation for individual cabinet parts. Exports include:
///   • Part outline (polyline boundary)
///   • Tenon geometry (for joinery-bearing parts)
///   • Tenon thinning pockets (blind pockets on edges)
///   • Grain direction indicator (dashed arrow along X axis)
///   • Part labels (name, quantity, dimensions, material, edge band, notes)
/// 
/// Methods:
///   ExportPart()
///     Exports standard cabinet parts with tenon edges determined by part name
///     (e.g., Toekick, Back, Deck, Top Stretcher, Drawer Stretcher).
///     Tenon blind depths configured per part type via LockDadoSettings.
///   
///   ExportEndPanel()
///     Exports end panels (Left End / Right End) with full mortise pocket and pilot hole geometry.
///     Applies coordinate transformations: 90° CW rotation (so grain runs horizontally)
///     and X-axis mirroring for Left End panels to place mortises/holes correctly.
///     Handles toekick notches when present. Requires pre-computed MortiseSpec list
///     from MortiseSpecBuilder.BuildForBaseStandard().
/// 
/// Coordinate System:
///   • Original end panel: X = cabinet depth, Y = cabinet height
///   • After CW90 rotation: X = height (grain direction), Y = depth
///   • Left End panels are X-mirrored after rotation to position geometry correctly
/// </summary>


internal static partial class DxfExporter
{
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

        // For toekick: left/right tenons use BlindStart=0, BlindStop=0
        // Top tenons keep default blind values
        if (kind == DxfPartKind.TenonTopLeftRight)
        {
            effectiveSettings = effectiveSettings with
            {
                BlindStartLeft = 0,
                BlindStopLeft = 0,
                BlindStartRight = 0,
                BlindStopRight = 0,
            };
        }

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
            AddRectangle(doc, LayerTenonThinningPocket, x1, x2, y1, y2);

        AddGrainArrow(doc, length, depth);
        AddLabels(doc, part);

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
}