using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;
using System.Diagnostics;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

/// <summary>
/// Assembles the final geometry for a part, handling winding order and edge routing.
/// </summary>
internal static class PanelGeometryCalculator
{
    internal static PartGeometry Compute(PartInfo part, JoineryConfig joinery, CabinetModel cabinet)
    {
        bool isEndPanelWithTk = part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.TkHeight > 0 && part.TkDepth > 0;

        var outline = new List<Vector2>();
        var thinningPockets = new List<(double x1, double x2, double y1, double y2)>();
        var mortisePockets = new List<(double x1, double x2, double y1, double y2)>();
        var holes = new List<(double x, double y, double radius)>();
        var holesThru = new List<(double x, double y, double radius)>();

        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double dadoDepth = joinery.DadoDepth;
        double materialThickness34 = MaterialDefaults.Thickness34;
        double stretcherWidth = 6;
        double topStretcherBackWidth = 3;

        // ── Branch to Toekick Outline ──────────────────────────────────────────
        if (isEndPanelWithTk)
        {
            outline = BuildEndPanelWithToeKick(length, height, part.TkHeight, part.TkDepth);
        }
        else
        {
            // ── Standard Panel Outline ─────────────────────────────────────────
            outline.Add(new Vector2(0, 0));
        }

        // Bottom Edge
        if (part.TenonEdges.HasFlag(TenonEdge.Bottom))
        {
            //var tenons = TenonCalculator.ComputeTenonRanges(length, joinery);
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            outline.Add(new Vector2(joinery.BlindStart, 0));
            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(tStart, 0));
                outline.Add(new Vector2(tStart, -dadoDepth));
                outline.Add(new Vector2(tEnd, -dadoDepth));
                outline.Add(new Vector2(tEnd, 0));
            }

            // Tenon Thinning Pockets
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Bottom))
            {
                if (length < 6)
                {
                    thinningPockets.Add((-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, 0, 0));
                }
                else
                {
                    thinningPockets.Add((joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, 0, 0));
                }
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, 0));
        



        // Right Edge
        if (part.TenonEdges.HasFlag(TenonEdge.Right))
        {
            //var tenons = TenonCalculator.ComputeTenonRanges(height, joinery);
            var tenons = TenonCalculator.ComputeTenonRanges(height, joinery, forceTwoTenons: height < 6);

            // Check to see if top stretcher front or drawer stretcher, if so, adjust blind start & blind stop so the part will have 2 tenons with 1 screw.

            if (part.Name.Contains("Stretcher") || part.Name.Equals("Nailer")) // Force two tenons and adjust blind start/stop for stretchers
            {
                tenons = TenonCalculator.ComputeTenonRanges(height, joinery, blindStartOverride: 1.25, blindStopOverride: 1.25, forceTwoTenons: true);
            }

            if (part.Name.Equals("Toekick")) // Force two tenons and adjust blind start/stop for Toekick
            {
                tenons = TenonCalculator.ComputeTenonRanges(height, joinery, blindStartOverride: 0, blindStopOverride: 0, forceTwoTenons: true);
            }

            foreach (var (tStart, tEnd) in tenons)
            {
                outline.Add(new Vector2(length, tStart));
                outline.Add(new Vector2(length + dadoDepth, tStart));
                outline.Add(new Vector2(length + dadoDepth, tEnd));
                outline.Add(new Vector2(length, tEnd));
            }

            // Tenon Thinning Pockets
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Right))
            {
                if (height < 6)
                {
                    thinningPockets.Add((length, length, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun));
                }
                else
                {
                    if (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer"))
                    {
                        thinningPockets.Add((length, length, 0, height));
                    }
                    else
                    {
                        thinningPockets.Add((length, length, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun));
                    }
                }
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(length, height));




        // Top Edge (Reverse order for winding)
        if (part.TenonEdges.HasFlag(TenonEdge.Top))
        {
            //var tenons = TenonCalculator.ComputeTenonRanges(length, joinery);
            var tenons = TenonCalculator.ComputeTenonRanges(length, joinery, forceTwoTenons: length < 6);
            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(tEnd, height));
                outline.Add(new Vector2(tEnd, height + dadoDepth));
                outline.Add(new Vector2(tStart, height + dadoDepth));
                outline.Add(new Vector2(tStart, height));
            }

            // Tenon Thinning Pockets
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Top))
            {
                if (length < 6)
                {
                    thinningPockets.Add((-joinery.TenonThinningOverrun, length + joinery.TenonThinningOverrun, height, height));
                }
                else
                {
                    thinningPockets.Add((joinery.BlindStart - joinery.TenonThinningOverrun, length - joinery.BlindStop + joinery.TenonThinningOverrun, height, height));
                }
            }
        }
        if (!isEndPanelWithTk) outline.Add(new Vector2(0, height));




        // Left Edge (Reverse order for winding)
        if (part.TenonEdges.HasFlag(TenonEdge.Left))
        {
            //var tenons = TenonCalculator.ComputeTenonRanges(height, joinery);
            var tenons = TenonCalculator.ComputeTenonRanges(height, joinery, forceTwoTenons: height < 6);

            if (part.Name.Contains("Stretcher") || part.Name.Equals("Nailer")) // Force two tenons and adjust blind start/stop for stretchers
            {
                tenons = TenonCalculator.ComputeTenonRanges(height, joinery, blindStartOverride: 1.25, blindStopOverride: 1.25, forceTwoTenons: true);
            }

            if (part.Name.Equals("Toekick")) // Force two tenons and adjust blind start/stop for Toekick
            {
                tenons = TenonCalculator.ComputeTenonRanges(height, joinery, blindStartOverride: 0, blindStopOverride: 0, forceTwoTenons: true);
            }

            for (int i = tenons.Count - 1; i >= 0; i--)
            {
                var (tStart, tEnd) = tenons[i];
                outline.Add(new Vector2(0, tEnd));
                outline.Add(new Vector2(-dadoDepth, tEnd));
                outline.Add(new Vector2(-dadoDepth, tStart));
                outline.Add(new Vector2(0, tStart));
            }

            // Tenon Thinning Pockets
            if (part.ThinningPockets.HasFlag(ThinningPocketEdge.Left))
            {
                if (height < 6)
                {
                    thinningPockets.Add((0, 0, -joinery.TenonThinningOverrun, height + joinery.TenonThinningOverrun));
                }
                else
                {
                    if (part.Name.Equals("Top Stretcher (Front)") || part.Name.Equals("Nailer"))
                    {
                        thinningPockets.Add((0, 0, 0, height));
                    }
                    else
                    {
                        thinningPockets.Add((0, 0, joinery.BlindStart - joinery.TenonThinningOverrun, height - joinery.BlindStop + joinery.TenonThinningOverrun));
                    }
                }
            }
        }



        // ── Compute Mortise Pockets ──────────────────────────────────────────────
        if (part.MortiseEdges.HasFlag(MortiseEdge.Left)) // All of the end panel operations happen only on the left end because the right end panels are mirrored from the left end panels, so we only need to compute mortises on the left edge for the end panels and then mirror them to the right edge.
        {
            if (part.Cabinet is BaseCabinetModel baseCab && part.Name.Contains("End"))
            {
                double openingHeight = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1) + materialThickness34;
                double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
                double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
                double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

                if (baseCab.TopType == "Stretcher") // Force two tenons and adjust blind start/stop for stretchers
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25));
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Left, joinery, additionalInset: 0));
                }

                // Drawer Stretchers
                if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: opening1Height + materialThickness34, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, fullThicknessTenon: true));
                }
                if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
                {
                    if (baseCab.DrwCount > 1)
                    {
                        for (int i = 0; i < baseCab.DrwCount; i++)
                        {
                            if (i == 1) openingHeight += opening2Height + materialThickness34;
                            if (i == 2) openingHeight += opening3Height + materialThickness34;
                            mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, stretcherWidth, MortiseEdge.Left, joinery, additionalInset: openingHeight, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25, fullThicknessTenon: true));
                        }
                    }
                }
            }
            else
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Left, joinery, additionalInset: 0));
            }
        }

        if (part.MortiseEdges.HasFlag(MortiseEdge.Right))
        {
            if (part.Cabinet is BaseCabinetModel baseCab && part.Name.Contains("End"))
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Right, joinery, additionalInset: part.TkHeight));
            }
            else
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(height, length, height, MortiseEdge.Right, joinery, additionalInset: 0));
            }
        }

        if (part.MortiseEdges.HasFlag(MortiseEdge.Bottom))
        {
            mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Bottom, joinery, additionalInset: 0));
        }

        if (part.MortiseEdges.HasFlag(MortiseEdge.Top))
        {
            if (part.Name.Contains("End"))
            {
                // Base Cab Back
                if (part.Cabinet is BaseCabinetModel baseCab) 
                {
                    if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                    {
                        mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(stretcherWidth, length, height, MortiseEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true));
                    }
                    else
                    {
                        mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length - ConvertDimension.FractionToDouble(baseCab.TKHeight), length, height, MortiseEdge.Top, joinery, additionalInset: 0));
                    }
                }
            }
            else
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePockets(length, length, height, MortiseEdge.Top, joinery, additionalInset: 0));
            }
        }




        // ── Compute Assembly Screw Holes in Gaps ─────────────────────────────────────────
        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left))
        {

            if (part.Cabinet is BaseCabinetModel baseCab && part.Name.Contains("End"))
            {
                double openingHeight = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1) + materialThickness34;
                double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
                double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
                double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

                if (baseCab.TopType == "Stretcher") // Force two tenons and adjust blind start/stop for stretchers
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25));
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0, forceTwoTenons: true, height - topStretcherBackWidth, blindStopOverride: 0));
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0));
                }

                // Drawer Stretchers
                if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: opening1Height + materialThickness34, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25));
                }
                if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
                {
                    if (baseCab.DrwCount > 1)
                    {
                        for (int i = 0; i < baseCab.DrwCount; i++)
                        {
                            if (i == 1) openingHeight += opening2Height + materialThickness34;
                            if (i == 2) openingHeight += opening3Height + materialThickness34;
                            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, stretcherWidth, ScrewHoleEdge.Left, joinery, additionalInset: openingHeight, forceTwoTenons: true, blindStartOverride: 1.25, blindStopOverride: 1.25));
                        }
                    }
                }
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Left, joinery, additionalInset: 0));
            }
        }

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right))
        {
            if (part.Cabinet is BaseCabinetModel baseCab)
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Right, joinery, additionalInset: part.TkHeight));
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(height, length, height, ScrewHoleEdge.Right, joinery, additionalInset: 0));
            }
        }

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Bottom))
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Bottom, joinery, additionalInset: 0));
        }

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Top))
        {
            if (part.Name.Contains("End"))
            {
                // Base Cab Back
                if (part.Cabinet is BaseCabinetModel baseCab)
                {
                    if (ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25)
                    {
                        holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(stretcherWidth, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0, forceTwoTenons: true));
                    }
                    else
                    {
                        holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length - ConvertDimension.FractionToDouble(baseCab.TKHeight), length - ConvertDimension.FractionToDouble(baseCab.TKHeight), height, ScrewHoleEdge.Top, joinery, additionalInset: 0));
                    }
                }
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHoles(length, length, height, ScrewHoleEdge.Top, joinery, additionalInset: 0));
            }
        }




        // ── Compute Shelf Holes ─────────────────────────────────────────
        if (part.Name.Contains("End", StringComparison.OrdinalIgnoreCase) && part.Cabinet is BaseCabinetModel || part.Cabinet is UpperCabinetModel)
        {
            if (part.Cabinet is BaseCabinetModel baseCab && part.Cabinet.Style != CabinetStyles.Base.Drawer & baseCab.DrillShelfHoles == true)
            {
                holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery));
            }
            else if (part.Cabinet is UpperCabinetModel upperCab && upperCab.DrillShelfHoles == true)
            {
                holes.AddRange(ShelfHoleCalculator.ComputeShelfHoles(part, joinery));
            }
        }


        // ── Mirror Left End Panels To Create Right End Panels ──────────────────────────────────────────────
        PartGeometry result = new PartGeometry(
            PartInfo: part,
            OutlineVertices: outline,
            TenonThinningPockets: thinningPockets,
            MortisePockets: mortisePockets,
            Holes: holes,
            HolesThru: holesThru
        );

        if (part.Name.Contains("Right End", StringComparison.OrdinalIgnoreCase))
        {
            result = result.MirrorAcrossVerticalCenterline(part.Bounds.Width);
        }

        return result;
    }


    /// <summary>
    /// Generates a Left end panel outline with rectangular toekick notch at bottom.
    /// </summary>
    private static List<Vector2> BuildEndPanelWithToeKick(double length, double height, double tkHeight, double tkDepth)
    {
        return new List<Vector2>
        {
            new (0,0),
            new (length-tkHeight, 0),
            new (length-tkHeight, tkDepth),
            new (length, tkDepth),
            new (length, tkDepth+3),
            new (length-0.5, tkDepth+3),
            new (length-0.5, height-3),
            new (length, height-3),
            new (length, height),
            new (0, height),
        };
    }
}
