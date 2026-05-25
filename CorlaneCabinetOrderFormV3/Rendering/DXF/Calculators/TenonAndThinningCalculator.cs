using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Computes tenon protrusions and thinning pockets for standard (non-L-shape) panel edges.
/// Handles all four edge directions with proper winding order.
/// </summary>
internal static class TenonAndThinningCalculator
{
    internal static void ComputeTenonsAndThinningPockets(PartInfo part, List<Vector2> outline, List<(double, double, double, double)> thinningPockets, JoineryConfig joinery, double mt34)
    {
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double dadoDepth = joinery.DadoDepth;
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        if (part.TenonEdges.HasFlag(TenonEdge.Bottom))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            outline.Add(new Vector2(joinery.BlindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(tStart, 0));
                outline.Add(new Vector2(tStart, -dadoDepth));
                outline.Add(new Vector2(tEnd, -dadoDepth));
                outline.Add(new Vector2(tEnd, 0));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Bottom))
            {
                thinningPockets.Add(length < 6
                    ? (-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, 0, 0)
                    : (joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, 0, 0));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, 0));

        if (part.TenonEdges.HasFlag(TenonEdge.Right))
        {
            var tenons = ResolveTenonRanges(height, joinery, part.Name, part.CabinetModel);
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(length, tStart));
                outline.Add(new Vector2(length + dadoDepth, tStart));
                outline.Add(new Vector2(length + dadoDepth, tEnd));
                outline.Add(new Vector2(length, tEnd));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Right))
            {
                thinningPockets.Add(height < 6
                    ? (length, length, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun)
                    : (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer")
                        ? (length, length, 0, height)
                        : (length, length, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun)));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, height));

        if (part.TenonEdges.HasFlag(TenonEdge.Top))
        {
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(tEnd, height));
                outline.Add(new Vector2(tEnd, height + dadoDepth));
                outline.Add(new Vector2(tStart, height + dadoDepth));
                outline.Add(new Vector2(tStart, height));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Top))
            {
                thinningPockets.Add(length < 6
                    ? (-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, height, height)
                    : (joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, height, height));
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(0, height));

        if (part.TenonEdges.HasFlag(TenonEdge.Left))
        {
            var tenons = ResolveTenonRanges(height, joinery, part.Name, part.CabinetModel);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(0, tEnd));
                outline.Add(new Vector2(-dadoDepth, tEnd));
                outline.Add(new Vector2(-dadoDepth, tStart));
                outline.Add(new Vector2(0, tStart));
            }
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Left))
            {
                thinningPockets.Add(height < 6
                    ? (0, 0, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun)
                    : (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer")
                        ? (0, 0, 0, height)
                        : (0, 0, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun)));
            }
        }
    }

    internal static List<(double start, double end)> ResolveTenonRanges(double edgeLength, JoineryConfig joinery, string partName, CabinetModel? cabinet)
    {
        var tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, forceTwoTenons: edgeLength < 6);

        if (partName.Contains("Stretcher") || partName.Equals("Nailer"))
        {
            if (cabinet is BaseCabinetModel)
                tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 1.25, blindStopOverride: 1.25, forceTwoTenons: true);
            else if (cabinet is UpperCabinetModel && partName.Contains("Nailer"))
                tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 1, blindStopOverride: 1, forceTwoTenons: false);
        }
        else if (partName.Contains("Toekick"))
        {
            tenons = TenonCalculator.ComputeTenonRanges(edgeLength, joinery, blindStartOverride: 0, blindStopOverride: 0, forceTwoTenons: true);
        }

        return tenons;
    }
}
