//using netDxf;
//using netDxf.Tables;

//namespace CorlaneCabinetOrderFormV3.Rendering.V4.DXF
//{
//    public class DxfLayerManager
//    {
//        public enum LayerType
//        {
//            PartOutline,
//            TenonThinningPocket,
//            MortisePocket,
//            DrillHolesBlind,
//            DrillHolesThrough
//        }

//        private readonly Dictionary<LayerType, Layer> _layers;

//        public DxfLayerManager()
//        {
//            // netDxf requires Layer(name) constructor + explicit Color property assignment
//            _layers = new Dictionary<LayerType, Layer>
//            {
//                { LayerType.PartOutline, new Layer("OUTLINE z17p8") { Color = new AciColor(7) } }, // white CHANGE THIS TO ACTUAL MATERIAL THICKNESS!
//                { LayerType.TenonThinningPocket, new Layer("CHAINCOMPRIGHT [3185] z9p0") { Color = new AciColor(1) } }, // red
//                { LayerType.MortisePocket, new Layer("POCKET [3115] z9p0") { Color = new AciColor(5) } }, // blue
//                { LayerType.DrillHolesBlind, new Layer("DRILL z12p7") { Color = new AciColor(4) } }, // cyan
//                { LayerType.DrillHolesThrough, new Layer("DRILL z17p8") { Color = new AciColor(1) } } // red CHANGE THIS TO ACTUAL MATERIAL THICKNESS!
//            };
//        }

//        public Layer GetLayer(LayerType type) => _layers[type];
//        public IEnumerable<Layer> GetLayers() => _layers.Values;
//    }
//}



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
            { LayerType.TenonThinningPocket, ("CHAINCOMPRIGHT [3185] z{0}", new AciColor(1)) },
            { LayerType.MortisePocket, ("POCKET [3115] z{0}", new AciColor(5)) },
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
