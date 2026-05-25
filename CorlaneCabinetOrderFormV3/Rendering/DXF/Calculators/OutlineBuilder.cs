using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Builds the outline geometry for cabinet panels, including standard rectangular outlines,
/// end panels with toe kicks, and arc generation for inside corners.
/// </summary>
internal static class OutlineBuilder
{
    internal static void BuildOutline(PartInfo part, bool isEndPanelWithTk, bool isLShape, double mt34, List<Vector2> outline)
    {
        if (isEndPanelWithTk)
        {
            outline.AddRange(BuildEndPanelWithToeKick(part.Bounds.Width, part.Bounds.Height, part.TkHeight, part.TkDepth));
        }
        else if (isLShape)
        {
            // L-shape outline is built sequentially in ComputeLShapeJoinery
            // We just seed it with the first point here to keep the signature consistent
            outline.Add(new Vector2(0, 0));
        }
        else
        {
            outline.Add(new Vector2(0, 0));
        }
    }

    internal static List<Vector2> BuildEndPanelWithToeKick(double length, double height, double tkHeight, double tkDepth)
    {
        return new List<Vector2>
        {
            new(0, 0),
            new(length - tkHeight, 0),
            new(length - tkHeight, tkDepth),
            new(length, tkDepth),
            new(length, tkDepth + 3),
            new(length - 0.5, tkDepth + 3),
            new(length - 0.5, height - 3),
            new(length, height - 3),
            new(length, height),
            new(0, height)
        };
    }

    internal static List<(double X, double Y)> GenerateInsideCornerArc(double cornerX, double cornerY, double radius, int segments)
    {
        double cx = cornerX - radius;
        double cy = cornerY + radius;
        var pts = new List<(double X, double Y)>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            double t = (double)i / segments;
            double angle = -(Math.PI / 2.0) + (t * Math.PI / 2.0);
            pts.Add((cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle)));
        }
        return pts;
    }
}
