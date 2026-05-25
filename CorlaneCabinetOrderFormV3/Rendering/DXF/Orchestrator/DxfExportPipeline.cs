using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;
using CorlaneCabinetOrderFormV3.Rendering.DXF.DXF;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Orchestrator;

internal record DxfExportOptions(
    LockDadoSettings? Settings,
    double TkHeight,
    double TkDepth,
    CabinetModel? CabinetModel,
    double MaterialThickness34
);

internal class DxfExportPipeline
{
    private readonly List<PartInfo> _parts;
    private readonly JoineryConfig _config;
    private readonly double _materialThickness;
    private readonly double _tenonThinningRatio;

    internal DxfExportPipeline(
        IEnumerable<PartListEntry> existingParts,
        DxfExportOptions options
    )
    {
        _parts = CabinetInputAdapter.MapParts(
            existingParts,
            options.Settings,
            options.TkHeight,
            options.TkDepth,
            options.CabinetModel,
            options.MaterialThickness34
        );
        _config = CabinetInputAdapter.MapSettings(options.Settings);
        _materialThickness = options.MaterialThickness34;
        _tenonThinningRatio = options.Settings?.TenonThickness ?? 0.4;
    }

    internal bool ValidateInput() => _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);

    internal DxfDocument GenerateDxf()
    {
        var geometries = new List<PartGeometry>();
        foreach (var part in _parts)
        {
            var geometry = PanelGeometryCalculator.Compute(part, _config, _materialThickness);
            geometries.Add(geometry);
        }

        var metricGeometries = MetricGeometryConverter.ConvertToMetric(geometries);

        return DxfWriter.GenerateDxf(metricGeometries, _materialThickness, _tenonThinningRatio);
    }

    internal void ExportToFile(string filePath)
    {
        var doc = GenerateDxf();
        doc.Save(filePath);
    }
}