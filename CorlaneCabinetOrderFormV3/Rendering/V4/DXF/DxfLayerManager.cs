using netDxf;
using netDxf.Tables;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.DXF
{
    public class DxfLayerManager
    {
        public enum LayerType
        {
            PartOutline,
            TenonThinningPocket,
            MortisePocket,
            DrillHolesBlind,
            DrillHolesThrough
        }

        private readonly Dictionary<string, Layer> _layerCache = new();
        private readonly Dictionary<LayerType, (string Template, AciColor Color)> _layerConfig = new()
        {
            { LayerType.PartOutline, ("OUTLINE z{0}", new AciColor(7)) },
            { LayerType.TenonThinningPocket, ("CHAINCOMPOUT [3185] z{0}", new AciColor(1)) },
            { LayerType.MortisePocket, ("POCKET z{0}", new AciColor(5)) },
            { LayerType.DrillHolesBlind, ("DRILL z12p7", new AciColor(4)) }, // Static as before
            { LayerType.DrillHolesThrough, ("DRILL z{0}", new AciColor(1)) }
        };

        private static string FormatThicknessKey(double thicknessInches)
        {
            double mm = thicknessInches * 25.4;
            // Matches your format: 17.8mm → 17p8
            return Math.Round(mm, 2).ToString("0.00").Replace('.', 'p');
        }

        public Layer GetLayer(LayerType type, double thicknessInches)
        {
            var (template, color) = _layerConfig[type];
            string layerName = string.Format(template, FormatThicknessKey(thicknessInches));

            if (!_layerCache.TryGetValue(layerName, out var layer))
            {
                layer = new Layer(layerName) { Color = color };
                _layerCache[layerName] = layer;
            }
            return layer;
        }

        public IEnumerable<Layer> GetLayers() => _layerCache.Values;
    }
}
