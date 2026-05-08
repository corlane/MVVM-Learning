using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Rendering.V4.DXF;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Orchestrator;

internal class DxfExportPipeline
{
    private readonly List<PartInfo> _parts;
    private readonly JoineryConfig _config;

    // Accept optional toekick dimensions from cabinet context
    internal DxfExportPipeline(
        IEnumerable<PartListEntry> existingParts,
        LockDadoSettings? settings = null,
        double tkHeight = 0,
        double tkDepth = 0,
        CabinetModel? cabinetModel = null,
        BaseCabinetDimensions? baseCabDim = null)
    {
        _parts = CabinetInputAdapter.MapParts(existingParts, settings, tkHeight, tkDepth, cabinetModel);
        _config = CabinetInputAdapter.MapSettings(settings);
    }

    internal bool ValidateInput()
    {
        return _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);
    }

    internal DxfDocument GenerateDxf()
    {
        var geometries = new List<PartGeometry>();

        foreach (var part in _parts)
        {
            var geometry = PanelGeometryCalculator.Compute(part, _config);
            geometries.Add(geometry);
        }

        return DxfWriter.GenerateDxf(geometries);
    }

    internal void ExportToFile(string filePath)
    {
        var doc = GenerateDxf();
        doc.Save(filePath);
    }
}
