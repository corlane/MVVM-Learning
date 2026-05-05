using netDxf.Tables;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.DXF;

/// <summary>
/// Manages DXF layers and colors for the new pipeline.
/// </summary>
internal static class DxfLayerManager
{
    public const string LayerOutline = "PART_OUTLINE";
    public const string LayerTenonPocket = "MACHINING_TENON_POCKET";
    public const string LayerMortisePocket = "MACHINING_MORTISE";
    public const string LayerHoles = "MACHINING_HOLES";
    public const string LayerGrain = "GRAIN_DIRECTION";
    public const string LayerLabels = "LABELS";

    public static Layer[] GetStandardLayers()
    {
        return new[]
        {
            new Layer(LayerOutline) { Color = new AciColor(2) }, // red
            new Layer(LayerTenonPocket) { Color = new AciColor(3) }, // green
            new Layer(LayerMortisePocket) { Color = new AciColor(5) }, // blue
            new Layer(LayerHoles) { Color = new AciColor(6) }, // cyan
            new Layer(LayerGrain) { Color = new AciColor(8) }, // gray
            new Layer(LayerLabels) { Color = new AciColor(7) } // white
        };
    }
}
