using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Adapters;
using CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Rendering.V4.DXF;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Orchestrator;

internal class DxfExportPipeline
{
    private readonly List<PartInfo> _parts;
    private readonly JoineryConfig _config;

    internal DxfExportPipeline(IEnumerable<PartListEntry> existingParts, LockDadoSettings? settings = null)
    {
        _parts = CabinetInputAdapter.MapParts(existingParts);
        _config = CabinetInputAdapter.MapSettings(settings);
    }

    internal bool ValidateInput()
    {
        return _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);
    }

    /// <summary>
    /// Generates the complete DXF document for all parts.
    /// </summary>
    internal DxfDocument GenerateDxf()
    {
        var geometries = new List<PartGeometry>();

        foreach (var part in _parts)
        {
            // Compute geometry for each part
            var geometry = PanelGeometryCalculator.Compute(part, _config);
            geometries.Add(geometry);
        }

        // Render to DXF
        return DxfWriter.GenerateDxf(geometries);
    }

    /// <summary>
    /// Convenience method to generate and save directly.
    /// </summary>
    internal void ExportToFile(string filePath)
    {
        var doc = GenerateDxf();
        doc.Save(filePath);
    }
}
