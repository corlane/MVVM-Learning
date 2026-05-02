//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Services;
//using netDxf;
//using netDxf.Entities;
//using netDxf.Tables;
//using System.IO;
//using System.Windows.Documents;
//using static ShelfHoleCalculator;

//namespace CorlaneCabinetOrderFormV3.Rendering;

///// <summary>
///// Entry points and layer configuration for DxfExporter.
///// 
///// Orchestrates DXF file generation across cabinets and parts. Exports one file per cabinet part
///// with separate CNC layers:
/////   • PART_OUTLINE (outline z18p6)           — closed cut boundary (white, z=18.6mm)
/////   • MACHINING_TENON_POCKET (pocket z9p0)   — thinning pockets on tenon edges (red, z=9.0mm)
/////   • MACHINING_MORTISE (pocket z6p35)       — discrete mortise pockets on end panels (red, z=6.35mm)
/////   • MACHINING_SCREW_HOLES (drill z12p0)    — CNC pilot holes on end panels (cyan, z=12.0mm)
/////   • GRAIN_DIRECTION                        — dashed centerline arrow (green)
/////   • LABELS                                 — part name, quantity, dimensions, material, EB, notes (yellow)
///// 
///// Public Methods:
/////   ExportAll(outputFolder, CabinetModel[], joinery?)
/////     Full export with mortise geometry for base standard/drawer cabinets.
/////     For each cabinet: iterates parts, builds MortiseSpec list once, routes to ExportEndPanel()
/////     for Left/Right End parts or ExportPart() for all others. Uses PartsListBuilder
/////     to generate part lists and cabinet labels.
/////   
/////   ExportAll(outputFolder, PartListEntry[], joinery?)
/////     Simplified overload for pre-built part lists. Exports all parts as ExportPart()
/////     (no mortise geometry). Useful when PartListEntry already computed elsewhere.
///// 
///// Constants:
/////   Layer name constants map CNC operations to machine depths and colors for import
/////   into CAM software (e.g., Aspire, VCarve, SheetCAM).
///// </summary>

//internal static partial class DxfExporter
//{
//    // ── Layer name constants ──────────────────────────────────────────────────

//    private const string LayerOutline = "outline z18p6";
//    private const string LayerTenonThinningPocket = "pocket z9p0";
//    private const string LayerMortise = "pocket z6p35";
//    private const string LayerScrewHoles = "drill z12p0";
//    private const string LayerGrain = "GRAIN_DIRECTION";
//    private const string LayerLabels = "LABELS";
//    private const string LayerShelfHoles = "drill z12p0";

//    // ── Public entry points ───────────────────────────────────────────────────

//    /// <summary>
//    /// Exports all parts for every cabinet in the job to individual DXF files
//    /// in <paramref name="outputFolder"/>. End panels on base standard cabinets
//    /// get full mortise and pilot-hole geometry; all other parts get tenon
//    /// outlines or plain rectangles as appropriate.
//    /// </summary>
//    internal static void ExportAll(
//        string outputFolder,
//        IEnumerable<CabinetModel> cabinets,
//        LockDadoSettings? joinery = null)

//    {
//        var s = joinery ?? LockDadoSettings.Default;
//        Directory.CreateDirectory(outputFolder);

//        int index = 1;
//        foreach (var cab in cabinets)
//        {
//            string label = PartsListBuilder.FormatLabel(cab, index++);
//            var parts = PartsListBuilder.BuildForCabinet(cab, label);

//            // Build mortise specs once per cabinet (currently only applicable to base standard & base drawer)
//            List<MortiseSpec>? mortiseSpecs = null;
//            List<ShelfHoleCalculator.ShelfHole>? shelfHoles = null;
//            BaseCabinetDimensions dim = default;

//            if (cab is BaseCabinetModel baseCab &&
//                (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) ||
//                 string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase)))
//            {
//                dim = BaseCabinetDimensions.From(baseCab);
//                mortiseSpecs = MortiseSpecBuilder.BuildForBaseStandard(baseCab, dim, s);
//                if (baseCab.DrillShelfHoles)
//                {
//                    shelfHoles = ShelfHoleCalculator.ComputeShelfHoles(baseCab, dim);
//                }
//            }

//            foreach (var part in parts)
//            {
//                string safeName = SanitizeFileName($"{label} — {part.PartName}");
//                string path = Path.Combine(outputFolder, safeName + ".dxf");

//                var kind = ResolvePartKind(part.PartName);

//                if (kind == DxfPartKind.MortisePanel && mortiseSpecs is not null)
//                    ExportEndPanel(path, part, mortiseSpecs, s,
//                        tkHeight: dim.TKHeight,
//                        tkDepth: dim.TKDepth,
//                        shelfHoles);
//                else
//                    ExportPart(path, part, s);
//            }
//        }
//    }
//}











using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System.IO;
using System.Windows.Documents;
using static ShelfHoleCalculator;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class DxfExporter
{
    private const string LayerOutline = "outline z18p6";
    private const string LayerTenonThinningPocket = "pocket z9p0";
    private const string LayerMortise = "pocket z6p35";
    private const string LayerScrewHoles = "drill z12p0";
    private const string LayerGrain = "GRAIN_DIRECTION";
    private const string LayerLabels = "LABELS";
    private const string LayerShelfHoles = "drill z12p0";
    private const string LayerDrawerSlides = "drill z12p0"; // NEW: Dedicated layer for slide holes

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

            List<MortiseSpec>? mortiseSpecs = null;
            List<ShelfHoleCalculator.ShelfHole>? shelfHoles = null;
            List<DrawerSlideHolesCalculator.DrawerSlideHole>? drawerSlideHoles = null; // NEW
            BaseCabinetDimensions dim = default;

            if (cab is BaseCabinetModel baseCab &&
                (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase)))
            {
                dim = BaseCabinetDimensions.From(baseCab);
                mortiseSpecs = MortiseSpecBuilder.BuildForBaseStandard(baseCab, dim, s);

                // ---------Shelf Holes
                if (baseCab.DrillShelfHoles)
                {
                    shelfHoles = ShelfHoleCalculator.ComputeShelfHoles(baseCab, dim);
                }

                // Compute drawer slide holes if cabinet has drawers
                if (baseCab.DrwCount > 0)
                {
                    // TODO: Replace placeholder x1..x6 with actual slide mounting depths from your UI/Settings
                    drawerSlideHoles = DrawerSlideHolesCalculator.Compute(
                        baseCab, dim,
                        x1: dim.Depth - (32/25.4), x2: dim.Depth - (64/25.4), x3: dim.Depth - (128/25.4), x4: dim.Depth - (256/25.4), x5: dim.Depth - (288/25.4), x6: dim.Depth - (320/25.4),
                        yOffsetFromBottom: 1.5);
                }
            }

            foreach (var part in parts)
            {
                string safeName = SanitizeFileName($"{label} — {part.PartName}");
                string path = Path.Combine(outputFolder, safeName + ".dxf");

                var kind = ResolvePartKind(part.PartName);

                if (kind == DxfPartKind.MortisePanel && mortiseSpecs is not null)
                    ExportEndPanel(path, part, mortiseSpecs, s,
                        tkHeight: dim.TKHeight,
                        tkDepth: dim.TKDepth,
                        shelfHoles,
                        drawerSlideHoles); // NEW
                else
                    ExportPart(path, part, s);
            }
        }
    }
}
