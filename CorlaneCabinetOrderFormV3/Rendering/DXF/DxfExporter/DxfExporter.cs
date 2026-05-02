using CorlaneCabinetOrderFormV3.Models;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class DxfExporter
{
    private const string LayerOutline = "outline z18p6";
    private const string LayerTenonThinningPocket = "pocket z10p0";
    private const string LayerMortise = "pocket [3185] z9p0";
    private const string LayerThruHoles = "drill z18p8";
    private const string LayerShelfHoles = "drill z13p0";
    private const string LayerDrawerSlideHoles = "drill z13p0";
    private const string LayerHingeHoles = "drill z13p0";

    private const string LayerGrain = "GRAIN_DIRECTION";
    private const string LayerLabels = "LABELS";

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
            List<DrawerSlideHolesCalculator.DrawerSlideHole>? drawerSlideHoles = null;
            BaseCabinetDimensions dim = default;

            if (cab is BaseCabinetModel baseCab &&
                (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase)))
            {
                dim = BaseCabinetDimensions.From(baseCab);

                // --------- Mortises and Tenon Thinning Pockets
                mortiseSpecs = MortiseSpecBuilder.BuildForBaseStandard(baseCab, dim, s);

                // ---------Shelf Holes
                if (baseCab.DrillShelfHoles)
                {
                    shelfHoles = ShelfHoleCalculator.ComputeShelfHoles(baseCab, dim);
                }

                // Compute drawer slide holes if cabinet has drawers
                if (baseCab.DrwCount > 0)
                {
                    drawerSlideHoles = DrawerSlideHolesCalculator.Compute(
                        baseCab, dim,
                        x1: dim.Depth - 1.4567, x2: dim.Depth - 3.9764, x3: dim.Depth - 5.2362, x4: dim.Depth - 6.4961, x5: dim.Depth - 9.0158, x6: dim.Depth - 10.2756, dim.Depth - 14.0551, dim.Depth - 17.8346, dim.Depth - 20.3543,
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
