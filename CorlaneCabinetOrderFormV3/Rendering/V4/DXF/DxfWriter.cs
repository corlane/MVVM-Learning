using netDxf;
using netDxf.Entities;
using netDxf.Geometry;
using netDxf.Tables;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.DXF;

/// <summary>
/// Renders PartGeometry objects into a netDxf document.
/// </summary>
internal static class DxfWriter
{
    internal static DxfDocument GenerateDxf(List<PartGeometry> geometries)
    {
        var doc = new DxfDocument();

        // 1. Setup Layers
        foreach (var layer in DxfLayerManager.GetStandardLayers())
            doc.AddTableEntry(layer);

        // 2. Render Each Part
        foreach (var geo in geometries)
        {
            // Outline: Use LWPolyline for 2D closed paths
            var outlinePoints = geo.OutlineVertices.Select(v => new Vector2(v.X, v.Y)).ToList();
            var polyline = new LWPolyline(outlinePoints);
            polyline.Layer = new Layer(DxfLayerManager.LayerOutline);
            doc.Add(polyline);

            // Thinning Pockets
            foreach (var (x1, x2, y1, y2) in geo.ThinningPockets)
            {
                var rect = CreateRectangle(x1, x2, y1, y2);
                rect.Layer = new Layer(DxfLayerManager.LayerTenonPocket);
                doc.Add(rect);
            }

            // Mortise Pockets
            foreach (var (x1, x2, y1, y2) in geo.MortisePockets)
            {
                var rect = CreateRectangle(x1, x2, y1, y2);
                rect.Layer = new Layer(DxfLayerManager.LayerMortisePocket);
                doc.Add(rect);
            }

            // Holes
            foreach (var (x, y, radius) in geo.Holes)
            {
                var circle = new Circle(new Vector3(x, y, 0), radius);
                circle.Layer = new Layer(DxfLayerManager.LayerHoles);
                doc.Add(circle);
            }
        }

        return doc;
    }

    /// <summary>
    /// Creates a closed 2D rectangle polyline for pockets.
    /// </summary>
    private static LWPolyline CreateRectangle(double x1, double x2, double y1, double y2)
    {
        return new LWPolyline(new[]
        {
            new Vector2(x1, y1),
            new Vector2(x2, y1),
            new Vector2(x2, y2),
            new Vector2(x1, y2)
        });
    }
}
