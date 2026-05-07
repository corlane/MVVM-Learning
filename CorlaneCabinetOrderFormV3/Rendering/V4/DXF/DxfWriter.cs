using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using netDxf;
using netDxf.Entities;
// Aliases to prevent collision with your custom V4.Vector2
using netDxfVector2 = netDxf.Vector2;
using netDxfVector3 = netDxf.Vector3;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.DXF
{
    // Changed to internal to match PartGeometry accessibility
    internal class DxfWriter
    {
        private readonly DxfLayerManager _layerManager;

        public DxfWriter(DxfLayerManager layerManager)
        {
            _layerManager = layerManager;
        }

        internal DxfDocument Write(List<PartGeometry> parts)
        {
            var doc = new DxfDocument(); // Matches your netDxf version

            // Register layers before assignment
            foreach (var layer in _layerManager.GetLayers())
            {
                doc.Layers.Add(layer);
            }

            foreach (var part in parts)
            {
                WriteOutline(doc, part);
                WritePockets(doc, part);
                WriteHolesThru(doc, part);
                WriteHolesBlind(doc, part);
            }

            return doc;
        }

        private void WriteOutline(DxfDocument doc, PartGeometry part)
        {
            if (part.OutlineVertices == null || part.OutlineVertices.Count < 3)
                return;

            // Convert custom V4.Vector2 → netDxf.Vector2
            var verts = part.OutlineVertices
                .Select(v => new netDxfVector2((float)v.X, (float)v.Y))
                .ToList();

            var polyline = new Polyline2D(verts) { IsClosed = true };
            polyline.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.PartOutline);
            doc.Entities.Add(polyline);
        }

        private void WritePockets(DxfDocument doc, PartGeometry part)
        {
            // Assumes TenonPockets & MortisePockets are IEnumerable<(double X1, double X2, double Y1, double Y2)>
            foreach (var pocket in part.TenonThinningPockets)
            {
                var rect = CreateClosedRectangle(pocket);
                rect.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.TenonThinningPocket);
                doc.Entities.Add(rect);
            }

            foreach (var pocket in part.MortisePockets)
            {
                var rect = CreateClosedRectangle(pocket);
                rect.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.MortisePocket);
                doc.Entities.Add(rect);
            }
        }

        private void WriteHolesThru(DxfDocument doc, PartGeometry part)
        {
            // Deconstruct (x, y, radius) tuple directly
            foreach (var (x, y, radius) in part.HolesThru)
            {
                var circle = new Circle(new netDxfVector3((float)x, (float)y, 0), (float)radius);
                circle.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.DrillHolesThrough);
                doc.Entities.Add(circle);
            }
        }

        private void WriteHolesBlind(DxfDocument doc, PartGeometry part)
        {
            // Deconstruct (x, y, radius) tuple directly
            foreach (var (x, y, radius) in part.Holes)
            {
                var circle = new Circle(new netDxfVector3((float)x, (float)y, 0), (float)radius);
                circle.Layer = _layerManager.GetLayer(DxfLayerManager.LayerType.DrillHolesBlind);
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

        /// <summary>
        /// Static convenience method matching DxfExportPipeline usage.
        /// </summary>
        internal static DxfDocument GenerateDxf(List<PartGeometry> geometries)
        {
            var writer = new DxfWriter(new DxfLayerManager());
            return writer.Write(geometries);
        }

    }
}
