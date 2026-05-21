using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services; // Added for MaterialDefaults
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
    private const string LayerMortiseThru = "pocket [3185] z18p6";

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
            List<HingeHolesCalculator.HingeBore>? hingeHoles = null;
            BaseCabinetDimensions dim = default;

            if (cab is BaseCabinetModel baseCab &&
                (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase)))
            {
                dim = BaseCabinetDimensions.From(baseCab);

                // --------- Mortises and Tenon Thinning Pockets
                mortiseSpecs = MortiseSpecBuilder.BuildForBaseStandard(baseCab, dim, s);

                // --------- Shelf Holes
                if (baseCab.DrillShelfHoles)
                {
                    shelfHoles = ShelfHoleCalculator.ComputeShelfHoles(baseCab, dim);
                }

                // Compute hinge holes if enabled and not a drawer cabinet
                if (baseCab.DrillHingeHoles && baseCab.Style != CabinetStyles.Base.Drawer)
                {
                    hingeHoles = HingeHolesCalculator.Compute(baseCab, dim, MaterialDefaults.Thickness34);
                }

                // Compute drawer slide holes if cabinet has drawers
                if (baseCab.DrwCount > 0)
                {
                    drawerSlideHoles = DrawerSlideHolesCalculator.Compute(baseCab, dim, yOffsetFromBottom: 1.5);
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
                        drawerSlideHoles,
                        hingeHoles);
                else
                    ExportPart(path, part, s);
            }
        }
    }
}

