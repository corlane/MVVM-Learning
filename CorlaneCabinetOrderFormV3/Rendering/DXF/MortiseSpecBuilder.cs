using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Computes all MortiseSpec entries for a single end panel (base standard cabinet).
/// Y positions are derived from the same ApplyTransform expressions used in
/// BaseCabinetBuilder, keeping the two in sync.
/// </summary>
internal static class MortiseSpecBuilder
{
    internal static List<MortiseSpec> BuildForBaseStandard(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        LockDadoSettings? s = null)
    {
        s ??= LockDadoSettings.Default;
        double mt34 = MaterialDefaults.Thickness34;
        double height = dim.Height;
        double depth = dim.Depth;
        double tkH = dim.TKHeight;
        double tkD = dim.TKDepth;

        const double stretcherWidth = 6.0;

        var specs = new List<MortiseSpec>();

        // ── Deck (flush Top face) ────────────────────────────────────────────
        // ApplyTransform Y = tkHeight
        specs.Add(BuildDepthSpec("Deck",
            partDepth: depth,
            mortiseBottomY: tkH,
            flushFace: TenonFlushFace.Top,
            s));

        // ── Back  ───────────────────────────────────────
        specs.Add(BuildHeightSpec("Back",
            edgeLength: height - mt34 - tkH,
            xPosition: mt34 / 2,
            bottomY: tkH,
            flushFace: TenonFlushFace.InteriorFront,
            s,
            forceTwoTenons: false));

        // ── Top or Top Stretcher Front ───────────────────────────────────────
        // Full Top: same geometry as Deck but placed at top (flush Top face).
        // Stretcher Top: tenon is only on the 6" stretcher width, flush Bottom face.
        bool isFull = string.Equals(baseCab.TopType, CabinetOptions.TopType.Full, StringComparison.OrdinalIgnoreCase);
        if (isFull)
        {
            // Full top — same flush face as Deck (outer/top face is flush)
            specs.Add(BuildDepthSpec("Top",
                partDepth: depth,
                mortiseBottomY: height - mt34,
                flushFace: TenonFlushFace.Top,
                s));
        }
        else
        {
            // Stretcher top — tenon runs along the 6" stretcher width, flush Bottom face
            specs.Add(BuildDepthSpec("Top Stretcher Front",
                partDepth: stretcherWidth,
                mortiseBottomY: height - mt34,
                flushFace: TenonFlushFace.Bottom,
                s = s with { BlindStart = 1.25, BlindStop = 1.25},
                xOffset: depth - stretcherWidth,
                forceTwoTenons: true));          // ← always 2 tenons
        }


        // ── Nailer (height-direction, tenon faces toward inside/front) ───────
        // Panel height = stretcherWidth, with top edge at height - mt34.
        bool hasNailer = string.Equals(baseCab.BackThickness, CabinetOptions.BackThickness.QuarterFraction, StringComparison.OrdinalIgnoreCase);
        if (hasNailer)
        {
            specs.Add(BuildHeightSpec("Nailer",
                edgeLength: stretcherWidth,
                xPosition: mt34/2,
                bottomY: height - mt34 - stretcherWidth,
                flushFace: TenonFlushFace.InteriorFront,
                s,
                forceTwoTenons: true));
        }

        // ── Drawer Stretchers (flush Bottom face) ────────────────────────────
        double[] openings = [dim.Opening1Height, dim.Opening2Height,
                             dim.Opening3Height, dim.Opening4Height];
        double runningY = height;
        for (int i = 0; i < 3; i++)
        {
            if (openings[i] <= 0) break;
            runningY -= openings[i] + 2 * mt34;
            specs.Add(BuildDepthSpec($"Drawer Stretcher {i + 1}",
                partDepth: stretcherWidth,
                mortiseBottomY: runningY,
                flushFace: TenonFlushFace.Bottom,
                s = s with { TenonThickness = mt34},
                xOffset: depth - stretcherWidth,
                forceTwoTenons: true));          // ← always 2 tenons
        }

        // ── Toekick (flush Back face, comb runs in height direction) ─────────
        if (tkH > 0 && tkD > 0)
        {
            specs.Add(BuildHeightSpec("Toekick",
                edgeLength: tkH - 0.5,
                xPosition: depth - tkD - mt34,
                bottomY: 0.5,
                flushFace: TenonFlushFace.InteriorFront,
                s = s with { BlindStart = 0, BlindStop = 0, TenonThickness = mt34 * 0.4},
                forceTwoTenons: true));
        }

        return specs;
    }

    // ── Factories ─────────────────────────────────────────────────────────────

    private static MortiseSpec BuildDepthSpec(
        string label, double partDepth, double mortiseBottomY,
        TenonFlushFace flushFace, LockDadoSettings s,
        double xOffset = 0, bool forceTwoTenons = false)
    {
        return new MortiseSpec
        {
            Label = label,
            Pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
                             partDepth, mortiseBottomY, flushFace, s, xOffset, forceTwoTenons),
            ScrewHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                             partDepth, mortiseBottomY, flushFace, s, xOffset, forceTwoTenons),
        };
    }

    private static MortiseSpec BuildHeightSpec(
        string label, double edgeLength, double xPosition, double bottomY, 
        TenonFlushFace flushFace, LockDadoSettings s, bool forceTwoTenons = false) 
    { 
        return new MortiseSpec 
        { Label = label, Pockets = PartOutlineBuilder.ComputeHeightDirectionMortisePockets(
            edgeLength, xPosition, bottomY, flushFace, s, forceTwoTenons), 
            ScrewHoles = PartOutlineBuilder.ComputeHeightDirectionScrewHoles(
                edgeLength, xPosition, bottomY, flushFace, s, forceTwoTenons), 
        }; 
    }
}