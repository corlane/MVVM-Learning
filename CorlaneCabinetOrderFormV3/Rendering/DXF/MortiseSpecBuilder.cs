using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

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
        LockDadoSettings s)
    {
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
                s,
                xOffset: depth - stretcherWidth));
        }


        // ── Nailer (flush InteriorFront face, same offset as Bottom) ─────────
        // ApplyTransform Y = height - stretcherWidth - Mt34
        specs.Add(BuildDepthSpec("Nailer",
            partDepth: depth,
            mortiseBottomY: height - stretcherWidth - mt34,
            flushFace: TenonFlushFace.InteriorFront,
            s));

        // ── Drawer Stretchers (flush Bottom face) ────────────────────────────
        double[] openings = [dim.Opening1Height, dim.Opening2Height,
                             dim.Opening3Height, dim.Opening4Height];
        double runningY = height;
        for (int i = 0; i < 4; i++)
        {
            if (openings[i] <= 0) break;
            runningY -= openings[i] + 2 * mt34;
            specs.Add(BuildDepthSpec($"Drawer Stretcher {i + 1}",
                partDepth: depth,
                mortiseBottomY: runningY,
                flushFace: TenonFlushFace.Bottom,
                s));
        }

        // ── Toekick (flush Back face, comb runs in height direction) ─────────
        if (tkH > 0 && tkD > 0)
        {
            specs.Add(BuildHeightSpec("Toekick",
                tkHeight: tkH,
                tkDepth: tkD,
                cabinetDepth: depth,
                s));
        }

        return specs;
    }

    // ── Factories ─────────────────────────────────────────────────────────────

    private static MortiseSpec BuildDepthSpec(
        string label, double partDepth, double mortiseBottomY,
        TenonFlushFace flushFace, LockDadoSettings s, double xOffset = 0)
    {
        return new MortiseSpec
        {
            Label = label,
            Pockets = PartOutlineBuilder.ComputeDepthDirectionMortisePockets(
                             partDepth, mortiseBottomY, flushFace, s, xOffset),
            ScrewHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                             partDepth, mortiseBottomY, flushFace, s, xOffset),
        };
    }

    private static MortiseSpec BuildHeightSpec(
        string label, double tkHeight, double tkDepth,
        double cabinetDepth, LockDadoSettings s)
    {
        return new MortiseSpec
        {
            Label = label,
            Pockets = PartOutlineBuilder.ComputeHeightDirectionMortisePockets(
                             tkHeight, tkDepth, cabinetDepth, s),
            ScrewHoles = PartOutlineBuilder.ComputeHeightDirectionScrewHoles(
                             tkHeight, tkDepth, cabinetDepth, s),
        };
    }
}