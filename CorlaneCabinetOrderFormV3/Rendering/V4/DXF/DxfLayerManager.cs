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

        private readonly Dictionary<LayerType, Layer> _layers;

        public DxfLayerManager()
        {
            // netDxf requires Layer(name) constructor + explicit Color property assignment
            _layers = new Dictionary<LayerType, Layer>
            {
                { LayerType.PartOutline, new Layer("CHAINCOMPRIGHT [3185] z17p8") { Color = new AciColor(7) } }, // white CHANGE THIS TO ACTUAL MATERIAL THICKNESS!
                { LayerType.TenonThinningPocket, new Layer("CHAINCOMPRIGHT [3185] z9p0") { Color = new AciColor(1) } }, // red
                { LayerType.MortisePocket, new Layer("POCKET [3115] z9p0") { Color = new AciColor(5) } }, // blue
                { LayerType.DrillHolesBlind, new Layer("DRILL z12p7") { Color = new AciColor(4) } }, // cyan
                { LayerType.DrillHolesThrough, new Layer("DRILL z17p8") { Color = new AciColor(1) } } // red CHANGE THIS TO ACTUAL MATERIAL THICKNESS!
            };
        }

        public Layer GetLayer(LayerType type) => _layers[type];
        public IEnumerable<Layer> GetLayers() => _layers.Values;
    }
}
