using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;
using netDxf;
using netDxf.Entities;
using netDxfVector2 = netDxf.Vector2;
using netDxfVector3 = netDxf.Vector3;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.DXF
{
    internal class DxfWriter
    {
        private readonly DxfLayerManager _layerManager;
        private readonly double _tenonThinningRatio;

        public DxfWriter(DxfLayerManager layerManager, double tenonThinningRatio = 0.4)
        {
            _layerManager = layerManager;
            _tenonThinningRatio = tenonThinningRatio;
        }

        internal DxfDocument Write(List<PartGeometry> parts, double thicknessInches)
        {
            var doc = new DxfDocument();

            foreach (var part in parts)
            {
                WriteOutline(doc, part, thicknessInches);
                WritePockets(doc, part, thicknessInches);
                WriteHolesThru(doc, part, thicknessInches);
                WriteHolesBlind(doc, part, thicknessInches);
            }

            foreach (var layer in _layerManager.GetLayers())
            {
                if (!doc.Layers.Contains(layer))
                    doc.Layers.Add(layer);
            }

            return doc;
        }

        private void WriteOutline(DxfDocument doc, PartGeometry part, double thicknessInches)
        {
            if (part.OutlineVertices == null || part.OutlineVertices.Count < 3) return;

            var verts = part.OutlineVertices
                .Select(v => new netDxfVector2((float)v.X, (float)v.Y))
                .ToList();

            var polyline = new Polyline2D(verts) { IsClosed = true };
            polyline.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.PartOutline, thicknessInches);
            doc.Entities.Add(polyline);
        }

        private void WritePockets(DxfDocument doc, PartGeometry part, double thicknessInches)
        {
            // Calculate cut depth: Full Thickness - Remaining Material (40%)
            double cutDepth = thicknessInches * (1.0 - _tenonThinningRatio);

            foreach (var pocket in part.TenonThinningPockets)
            {
                var rect = CreateClosedRectangle(pocket);
                rect.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.TenonThinningPocket, cutDepth);
                doc.Entities.Add(rect);
            }

            foreach (var pocket in part.MortisePockets)
            {
                var rect = CreateClosedRectangle(pocket);
                rect.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.MortisePocket, thicknessInches);
                doc.Entities.Add(rect);
            }
        }

        private void WriteHolesThru(DxfDocument doc, PartGeometry part, double thicknessInches)
        {
            foreach (var (x, y, radius) in part.HolesThru)
            {
                var circle = new Circle(new netDxfVector3((float)x, (float)y, 0), (float)radius);
                circle.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.DrillHolesThrough, thicknessInches);
                doc.Entities.Add(circle);
            }
        }

        private void WriteHolesBlind(DxfDocument doc, PartGeometry part, double thicknessInches)
        {
            foreach (var (x, y, radius) in part.Holes)
            {
                var circle = new Circle(new netDxfVector3((float)x, (float)y, 0), (float)radius);
                circle.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.DrillHolesBlind, thicknessInches);
                doc.Entities.Add(circle);
            }
        }

        private Polyline2D CreateClosedRectangle((double X1, double X2, double Y1, double Y2) bounds)
        {
            var verts = new List<netDxfVector2>
            {
                new((float)bounds.X1, (float)bounds.Y1),
                new((float)bounds.X2, (float)bounds.Y1),
                new((float)bounds.X2, (float)bounds.Y2),
                new((float)bounds.X1, (float)bounds.Y2),
            };
            return new Polyline2D(verts) { IsClosed = true };
        }

        internal static DxfDocument GenerateDxf(List<PartGeometry> geometries, double thicknessInches, double tenonThinningRatio = 0.4)
        {
            var writer = new DxfWriter(new DxfLayerManager(), tenonThinningRatio);
            return writer.Write(geometries, thicknessInches);
        }
    }
}
