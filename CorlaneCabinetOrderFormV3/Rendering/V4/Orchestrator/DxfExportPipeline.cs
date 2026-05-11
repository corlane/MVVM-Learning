//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;
//using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
//using CorlaneCabinetOrderFormV3.Rendering.V4.DXF;
//using netDxf;

//namespace CorlaneCabinetOrderFormV3.Rendering.V4.Orchestrator;

//internal class DxfExportPipeline
//{
//    private readonly List<PartInfo> _parts;
//    private readonly JoineryConfig _config;

//    // Accept optional toekick dimensions from cabinet context
//    internal DxfExportPipeline(
//        IEnumerable<PartListEntry> existingParts,
//        LockDadoSettings? settings = null,
//        double tkHeight = 0,
//        double tkDepth = 0,
//        CabinetModel? cabinetModel = null,
//        BaseCabinetDimensions? baseCabDim = null,
//        double materialThickness34 = 0
//        )
//    {
//        _parts = CabinetInputAdapter.MapParts(existingParts, settings, tkHeight, tkDepth, cabinetModel, baseCabDim, materialThickness34);
//        _config = CabinetInputAdapter.MapSettings(settings);
//    }

//    internal bool ValidateInput()
//    {
//        return _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);
//    }

//    internal DxfDocument GenerateDxf(double materialThickness34)
//    {
//        var geometries = new List<PartGeometry>();

//        foreach (var part in _parts)
//        {
//            var geometry = PanelGeometryCalculator.Compute(part, _config, part.Cabinet!, materialThickness34);
//            geometries.Add(geometry);
//        }

//        return DxfWriter.GenerateDxf(geometries);
//    }

//    internal void ExportToFile(string filePath, double materialThickness34)
//    {
//        var doc = GenerateDxf(materialThickness34);
//        doc.Save(filePath);
//    }
//}




//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;
//using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
//using CorlaneCabinetOrderFormV3.Rendering.V4.DXF;
//using netDxf;

//namespace CorlaneCabinetOrderFormV3.Rendering.V4.Orchestrator
//{
//    internal class DxfExportPipeline
//    {
//        private readonly List<PartInfo> _parts;
//        private readonly JoineryConfig _config;
//        private readonly double _materialThickness;

//        internal DxfExportPipeline(
//            IEnumerable<PartListEntry> existingParts,
//            LockDadoSettings? settings = null,
//            double tkHeight = 0,
//            double tkDepth = 0,
//            CabinetModel? cabinetModel = null,
//            BaseCabinetDimensions? baseCabDim = null,
//            double materialThickness34 = 0
//        )
//        {
//            _parts = CabinetInputAdapter.MapParts(existingParts, settings, tkHeight, tkDepth, cabinetModel, baseCabDim, materialThickness34);
//            _config = CabinetInputAdapter.MapSettings(settings);
//            _materialThickness = materialThickness34;
//        }

//        internal bool ValidateInput() => _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);

//        internal DxfDocument GenerateDxf()
//        {
//            var geometries = new List<PartGeometry>();
//            foreach (var part in _parts)
//            {
//                var geometry = PanelGeometryCalculator.Compute(part, _config, part.Cabinet!, _materialThickness);
//                geometries.Add(geometry);
//            }
//            return DxfWriter.GenerateDxf(geometries, _materialThickness);
//        }

//        internal void ExportToFile(string filePath)
//        {
//            var doc = GenerateDxf();
//            doc.Save(filePath);
//        }
//    }
//}




using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Rendering.V4.DXF;
using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Orchestrator
{
    internal class DxfExportPipeline
    {
        private readonly List<PartInfo> _parts;
        private readonly JoineryConfig _config;
        private readonly double _materialThickness;
        private readonly double _tenonThinningRatio;

        internal DxfExportPipeline(
            IEnumerable<PartListEntry> existingParts,
            LockDadoSettings? settings = null,
            double tkHeight = 0,
            double tkDepth = 0,
            CabinetModel? cabinetModel = null,
            BaseCabinetDimensions? baseCabDim = null,
            double materialThickness34 = 0
        )
        {
            _parts = CabinetInputAdapter.MapParts(existingParts, settings, tkHeight, tkDepth, cabinetModel, baseCabDim, materialThickness34);
            _config = CabinetInputAdapter.MapSettings(settings);
            _materialThickness = materialThickness34;
            _tenonThinningRatio = settings?.TenonThickness ?? 0.4; // Fallback to 40% if null
        }

        internal bool ValidateInput() => _parts.All(p => p.Bounds.Width > 0 && p.Bounds.Height > 0);

        internal DxfDocument GenerateDxf()
        {
            var geometries = new List<PartGeometry>();
            foreach (var part in _parts)
            {
                var geometry = PanelGeometryCalculator.Compute(part, _config, part.Cabinet!, _materialThickness);
                geometries.Add(geometry);
            }
            return DxfWriter.GenerateDxf(geometries, _materialThickness, _tenonThinningRatio);
        }

        internal void ExportToFile(string filePath)
        {
            var doc = GenerateDxf();
            doc.Save(filePath);
        }
    }
}
