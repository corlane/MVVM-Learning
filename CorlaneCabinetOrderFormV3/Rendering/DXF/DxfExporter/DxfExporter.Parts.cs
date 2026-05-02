using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class DxfExporter
{
    internal static void ExportPart(
        string filePath,
        PartListEntry part,
        LockDadoSettings? joinery = null)
    {
        // ... [Keep your existing ExportPart method exactly as is] ...
        var s = joinery ?? new LockDadoSettings();
        var doc = CreateDocument();
        double length = part.LengthIn;
        double depth = part.WidthIn;
        var kind = ResolvePartKind(part.PartName);

        var (tenonEdges, effectiveSettings, forceTwoTenons) = part.PartName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" =>
                (EdgeDesignators.Top | EdgeDesignators.Left | EdgeDesignators.Right,
                 s with { BlindStartLeft = 0, BlindStopLeft = 0, BlindStartRight = 0, BlindStopRight = 0, BlindStartTop = 1.5, BlindStopTop = 1.5 },
                 true),
            "Top Stretcher (Front)" or "Drawer Stretcher" =>
                (EdgeDesignators.LeftRight, s with { BlindStartLeft = 1.25, BlindStopLeft = 1.25, BlindStartRight = 1.25, BlindStopRight = 1.25 }, true),
            "Back" => (EdgeDesignators.TopBottom, s, false),
            "Deck" => (EdgeDesignators.LeftRight, s, false),
            _ => (EdgeDesignators.None, s, false),
        };

        var outline = kind switch
        {
            DxfPartKind.MortisePanel => PartOutlineBuilder.Rectangle(length, depth),
            DxfPartKind.TenonLeftAndRight => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            DxfPartKind.TenonTopAndBottom => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            DxfPartKind.TenonTopLeftRight => PartOutlineBuilder.BuildPanelWithTenons(length, depth, effectiveSettings, tenonEdges, forceTwoTenons),
            _ => PartOutlineBuilder.Rectangle(length, depth),
        };
        AddClosedPolyline(doc, LayerOutline, outline);
        AddClosedPolyline(doc, LayerOutline, outline);

        bool skipThinningPockets = part.PartName == "Drawer Stretcher";
        if (!skipThinningPockets)
        {
            foreach (var (x1, x2, y1, y2) in PartOutlineBuilder.ComputeTenonThinningPockets(length, depth, effectiveSettings, tenonEdges, forceTwoTenons))
                AddRectangle(doc, LayerTenonThinningPocket, x1, x2, y1, y2);
        }

        AddGrainArrow(doc, length, depth);
        AddLabels(doc, part);
        doc.Save(filePath);
    }

    // UPDATED SIGNATURE: Added drawerSlideHoles parameter
    internal static void ExportEndPanel(
        string filePath,
        PartListEntry part,
        IEnumerable<MortiseSpec> mortiseSpecs,
        LockDadoSettings? joinery = null,
        double tkHeight = 0,
        double tkDepth = 0,
        List<ShelfHoleCalculator.ShelfHole>? shelfHoles = null,
        List<DrawerSlideHolesCalculator.DrawerSlideHole>? drawerSlideHoles = null)
    {
        var s = joinery ?? LockDadoSettings.Default;
        var doc = CreateDocument();

        double oldWidth = part.WidthIn;
        double newLength = part.LengthIn;
        double newDepth = part.WidthIn;
        bool isLeft = part.PartName == "Left End";

        var rawOutline = tkHeight > 0
            ? PartOutlineBuilder.EndPanelWithToeKick(part.WidthIn, part.LengthIn, tkHeight, tkDepth)
            : PartOutlineBuilder.Rectangle(part.WidthIn, part.LengthIn);
        var rotatedOutline = RotateCW90(rawOutline, oldWidth);
        if (isLeft) rotatedOutline = MirrorX(rotatedOutline, newLength);
        AddClosedPolyline(doc, LayerOutline, rotatedOutline);

        // ── Mortise Pockets and Screw Holes ───────────────────────────────────
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
                AddCircle(doc, LayerThruHoles, rcx, rcy, dia / 2.0);
            }
        }

        // ── Shelf Holes ───────────────────────────────────────────────────────
        if (shelfHoles is not null)
        {
            const double shelfHoleDia = 0.1968; // 5mm standard shelf pin
            foreach (var hole in shelfHoles)
            {
                var (rcx, rcy, _) = RotateHoleCW90(hole.X, hole.Y, shelfHoleDia, oldWidth);
                if (isLeft) rcx = newLength - rcx;
                AddCircle(doc, LayerShelfHoles, rcx, rcy, shelfHoleDia / 2.0);
            }
        }

        // ── Drawer Slide Holes ────────────────────────────────────────────────
        if (drawerSlideHoles is not null)
        {
            const double slideHoleDia = 0.1968; // 5mm

            foreach (var hole in drawerSlideHoles)
            {
                // Apply same CW90 rotation and Left-End mirroring as shelf holes
                var (rcx, rcy, _) = RotateHoleCW90(hole.X, hole.Y, slideHoleDia, oldWidth);
                if (isLeft) rcx = newLength - rcx;
                AddCircle(doc, LayerDrawerSlideHoles, rcx, rcy, slideHoleDia / 2.0);
            }
        }

        AddGrainArrow(doc, newLength, newDepth);
        AddLabels(doc, part);
        doc.Save(filePath);
    }
}
