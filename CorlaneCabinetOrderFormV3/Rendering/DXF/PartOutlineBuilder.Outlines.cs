using netDxf;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class PartOutlineBuilder
{
    internal static List <Vector2> Rectangle(double length, double depth) =>
    [
        Vertex(0, 0), Vertex(length, 0),
        Vertex(length, depth), Vertex(0, depth)
    ];

    internal static List <Vector2> EndPanelWithToeKick(double depth, double height, double tkHeight, double tkDepth)
    {
        return
        [
            Vertex(depth, tkHeight), Vertex(depth, height),
            Vertex(0, height), Vertex(0, 0),
            Vertex(3, 0), Vertex(3, 0.5),
            Vertex(depth - tkDepth - 3, 0.5), Vertex(depth - tkDepth - 3, 0),
            Vertex(depth - tkDepth, 0), Vertex(depth - tkDepth, tkHeight)
        ];
    }

    internal static List <Vector2> BuildPanelWithTenons(
        double length, double depth, LockDadoSettings s,
        EdgeDesignators tenonEdges = EdgeDesignators.None, bool forceTwoTenons = false)
    {
        double dd = s.DadoDepth;
        double blindStart = s.BlindStart;
        double blindEnd = depth - s.BlindStop;
        var verts = new List<Vector2>();

        verts.Add(Vertex(0, 0));
        if (tenonEdges.HasFlag(EdgeDesignators.Bottom))
        {
            var tenons = ComputeTenonRanges(length, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Bottom), s.ResolveBlindStop(EdgeDesignators.Bottom));
            verts.Add(Vertex(blindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                verts.Add(Vertex(tStart, 0));
                verts.Add(Vertex(tStart, -dd));
                verts.Add(Vertex(tEnd, -dd));
                verts.Add(Vertex(tEnd, 0));
            }
        }
        verts.Add(Vertex(length, 0));

        if (tenonEdges.HasFlag(EdgeDesignators.Right))
        {
            var tenons = ComputeTenonRanges(depth, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Right), s.ResolveBlindStop(EdgeDesignators.Right));
            foreach (var (tStart, tEnd) in tenons)
            {
                verts.Add(Vertex(length, tStart));
                verts.Add(Vertex(length + dd, tStart));
                verts.Add(Vertex(length + dd, tEnd));
                verts.Add(Vertex(length, tEnd));
            }
            verts.Add(Vertex(length, blindEnd));
        }

        verts.Add(Vertex(length, depth));
        if (tenonEdges.HasFlag(EdgeDesignators.Top))
        {
            var tenons = ComputeTenonRanges(length, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Top), s.ResolveBlindStop(EdgeDesignators.Top));
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                verts.Add(Vertex(tEnd, depth));
                verts.Add(Vertex(tEnd, depth + dd));
                verts.Add(Vertex(tStart, depth + dd));
                verts.Add(Vertex(tStart, depth));
            }
            verts.Add(Vertex(blindStart, depth));
        }
        verts.Add(Vertex(0, depth));

        if (tenonEdges.HasFlag(EdgeDesignators.Left))
        {
            var tenons = ComputeTenonRanges(depth, s, forceTwoTenons, s.ResolveBlindStart(EdgeDesignators.Left), s.ResolveBlindStop(EdgeDesignators.Left));
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                verts.Add(Vertex(0, tEnd));
                verts.Add(Vertex(-dd, tEnd));
                verts.Add(Vertex(-dd, tStart));
                verts.Add(Vertex(0, tStart));
            }
            verts.Add(Vertex(0, blindStart));
        }

        return verts;
    }

    internal static List<Vector2> MortisePanel(double length, double depth) => Rectangle(length, depth);
}