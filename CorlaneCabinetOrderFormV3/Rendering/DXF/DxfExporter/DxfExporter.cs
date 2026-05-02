using CorlaneCabinetOrderFormV3.Models;
using System.IO;

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
