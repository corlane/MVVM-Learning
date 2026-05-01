using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Mortise specification builder for base standard cabinet end panels.
/// 
/// Computes all mortise pocket and screw hole geometries for a single end panel
/// by assembling MortiseSpec entries for each tenon-bearing cabinet component
/// (Deck, Back, Top, Nailer, Drawer Stretchers, Toekick).
/// 
/// Y/X Position Synchronization:
///   All component positions are derived from the same ApplyTransform expressions
///   used in BaseCabinetBuilder 3D rendering. This keeps end panel DXF mortises
///   perfectly aligned with cabinet geometry, preventing assembly mismatches.
///   Any change to BaseCabinetBuilder transforms must be mirrored here.
/// 
/// Main Entry Point:
///   BuildForBaseStandard(BaseCabinetModel, BaseCabinetDimensions, LockDadoSettings?)
///     Orchestrates mortise spec generation for the entire cabinet.
///     
///     Inputs:
///       • baseCab — cabinet model with TopType, BackThickness, opening heights, etc.
///       • dim — pre-computed dimensions (Height, Depth, TKHeight, TKDepth)
///       • s — joinery settings (BlindStart, BlindStop, DadoDepth, TenonThickness, etc.)
///     
///     Output: List<MortiseSpec> — one entry per component with mortises + screw holes
///     
///     Algorithm:
///       1. Extract cabinet dimensions and thickness constants
///       2. For each tenon-bearing component, determine:
///          • Part depth or edge length
///          • Mortise position (Y for depth-direction, X+Y for height-direction)
///          • Tenon flush face (Top, Bottom, InteriorFront, Back)
///          • Special settings (BlindStart/Stop overrides, TenonThickness, forceTwoTenons)
///       3. Build specs via BuildDepthSpec() or BuildHeightSpec() factory methods
///       4. Return ordered list for export
/// 
/// Cabinet Components (in build order):
/// 
///   Deck (depth-direction, flush Top face)
///     • Top interior support surface
///     • Mortise position: Y = tkHeight (flush with toekick top edge)
///     • Part depth: depth - 3/4" (accounts for material thickness)
///     • Flush face: Top (mortise on outer/top surface)
///   
///   Back (height-direction, flush InteriorFront)
///     • Interior back panel (supports back edge)
///     • Mortise position: X = 3/8" (offset from edge), Y = tkHeight (bottom)
///       Height = height - 3/4" - tkHeight (spans above deck)
///     • Flush face: InteriorFront (mortise recessed into panel interior)
///   
///   Top (conditional: Full or Top Stretcher Front)
///     • Full Top: complete top panel, same geometry as Deck but positioned at cabinet top
///       Position: Y = height - 3/4"
///       Flush face: Top
///     • Top Stretcher Front: 6" wide stretcher on front edge (for open-top cabinets)
///       Position: Y = height - 3/4" (flush with full cabinet height)
///       Part depth: 6" (stretcher width)
///       Flush face: Bottom (mortise on bottom face, hidden)
///       forceTwoTenons: true (always exactly 2 tenons for structural consistency)
///   
///   Nailer (conditional, height-direction, flush InteriorFront)
///     • Present only if BackThickness = QuarterFraction (1/4" back + nailer)
///     • Small panel above back (6" tall), used for fastening thin back
///     • Position: X = 3/8", Y = height - 3/4" - 6"
///     • Flush face: InteriorFront
///     • forceTwoTenons: true
///   
///   Drawer Stretchers (depth-direction, flush Bottom face, 1–3 units)
///     • Horizontal supports between drawer openings
///     • One per opening (up to 3; opening count determines stretcher count)
///     • Position: Y computed by decrementing from top by opening heights + material
///       runningY starts at height, decreases by (opening + 2×3/4") per stretcher
///     • Part depth: 6" (stretcher width)
///     • TenonThickness override: 3/4" (structural reinforcement for drawer support)
///     • Flush face: Bottom (mortise on underside)
///     • forceTwoTenons: true
///   
///   Toekick (conditional, height-direction, flush Back face)
///     • Comb tenons run horizontally across toekick height
///     • Present only if tkHeight > 0 AND tkDepth > 0
///     • Position: X = depth - tkDepth - 3/4" (flush with back face)
///                 Y = 0.5" (starting height above floor)
///     • Edge length: tkHeight - 0.5"
///     • Special settings: BlindStart = 0, BlindStop = 0 (full-height tenons)
///                        TenonThickness = 3/4" × 0.4 = 0.3" (comb profile)
///     • Flush face: Back (mortise on rear surface, hidden)
///     • forceTwoTenons: true
/// 
/// Factories:
///   BuildDepthSpec()
///     Creates MortiseSpec for depth-direction components (Deck, Top, Top Stretcher, Drawer Stretchers).
///     Delegates to PartOutlineBuilder:
///       • ComputeDepthDirectionMortisePockets() — pocket rectangles
///       • ComputeDepthDirectionScrewHoles() — hole centers
///   
///   BuildHeightSpec()
///     Creates MortiseSpec for height-direction components (Back, Nailer, Toekick).
///     Delegates to PartOutlineBuilder:
///       • ComputeHeightDirectionMortisePockets() — pocket rectangles
///       • ComputeHeightDirectionScrewHoles() — hole centers
/// 
/// Special Parameters:
///   TenonFlushFace — controls how mortise recesses into the end panel:
///     • Top: mortise on outer/top surface (Deck, Full Top)
///     • Bottom: mortise on underside (Top Stretcher, Drawer Stretchers)
///     • InteriorFront: mortise recessed into panel interior (Back, Nailer)
///     • Back: mortise on rear surface, hidden (Toekick)
///   
///   forceTwoTenons — enforces exactly 2 tenons (ignores LockDadoSettings gap logic)
///     Used for: Top Stretcher, Nailer, Drawer Stretchers, Toekick
///     Reason: structural consistency across all similar components
///   
///   BlindStart / BlindStop overrides — component-specific blind pocket depths
///     Example: Top Stretcher Front: BlindStart = 1.25, BlindStop = 1.25
///       Prevents mortises from reaching stretcher ends (assembly flexibility)
///   
///   TenonThickness overrides — control mortise depth:
///     Example: Drawer Stretchers: TenonThickness = 3/4"
///       Thicker mortise for structural support; Toekick: TenonThickness = 0.3"
///       Thin comb profile for aesthetic toekick design
/// 
/// Material Thickness Constants:
///   • MaterialDefaults.Thickness34 = 0.75" (standard cabinet material)
///   • Used to adjust part depth (e.g., depth - 3/4" for Deck)
///   • Used to compute running Y for Drawer Stretchers
/// 
/// Integration:
///   Used by DxfExporter.ExportAll(CabinetModel[]) to generate mortise specs once per cabinet,
///   then passed to ExportEndPanel() for each Left/Right End panel.
///   MortiseSpec.Pockets rendered to MACHINING_MORTISE DXF layer.
///   MortiseSpec.ScrewHoles rendered to MACHINING_SCREW_HOLES DXF layer.
/// </summary>

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
        var baseSettings = s ?? new LockDadoSettings();
        double mt34 = MaterialDefaults.Thickness34;
        double backThickness = dim.BackThickness;
        double height = dim.Height;
        double depth = dim.Depth;
        double tkH = dim.TKHeight;
        double tkD = dim.TKDepth;

        const double stretcherWidth = 6.0;

        // Add settings below for custom overrides to default parameters
        var topStretcherFrontSettings = baseSettings with { BlindStart = 1.25, BlindStop = 1.25 };
        var drwStretcherSettings = baseSettings with { TenonThickness = mt34, BlindStart = 1.25, BlindStop = 1.25 };
        var toekickSettings = baseSettings with { BlindStart = 0, BlindStop = 0, TenonThickness = mt34 * 0.4 };
        // -------------------------------------------------------------


        var specs = new List<MortiseSpec>();

        // ── Deck (flush Top face) ────────────────────────────────────────────
        // ApplyTransform Y = tkHeight
        specs.Add(BuildDepthSpec("Deck",
            partDepth: depth - backThickness,
            mortiseBottomY: tkH - baseSettings.TenonClearance,
            flushFace: TenonFlushFace.Top,
            s: baseSettings));

        // ── Back  ───────────────────────────────────────
        if (dim.BackThickness == mt34)
        {
            specs.Add(BuildHeightSpec("Back",
                edgeLength: height - mt34 - tkH,
                xPosition: mt34 - baseSettings.MortiseSlotHeight,
                bottomY: tkH,
                flushFace: TenonFlushFace.Front,
                s: baseSettings,
                forceTwoTenons: false));
        }

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
                flushFace: TenonFlushFace.Bottom,
                s: baseSettings));
        }
        else
        {
            // Stretcher top — tenon runs along the 6" stretcher width, flush Bottom face
            specs.Add(BuildDepthSpec("Top Stretcher Front",
                partDepth: stretcherWidth,
                mortiseBottomY: height - mt34,
                flushFace: TenonFlushFace.Bottom,
                s: topStretcherFrontSettings,
                xOffset: depth - stretcherWidth,
                forceTwoTenons: true));          // ← always 2 tenons
        }


        // ── Nailer (height-direction, tenon faces toward inside/front) ───────
        // Panel height = stretcherWidth, with top edge at height - mt34.
        //bool hasNailer = string.Equals(baseCab.BackThickness, CabinetOptions.BackThickness.QuarterFraction, StringComparison.OrdinalIgnoreCase);
        if (backThickness == 0.25)
        {
            specs.Add(BuildHeightSpec("Nailer",
                edgeLength: stretcherWidth,
                xPosition: mt34 - baseSettings.MortiseSlotHeight,
                bottomY: height - mt34 - stretcherWidth,
                flushFace: TenonFlushFace.Front,
                s: baseSettings,
                forceTwoTenons: true));
        }

        // ── Drawer Stretchers (flush Bottom face) ────────────────────────────
        double[] openings = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];

        if (string.Equals(baseCab.Style, CabinetStyles.Base.Standard, StringComparison.OrdinalIgnoreCase) && baseCab.DrwCount == 1)
        {
            // Standard cabinet with single drawer: one stretcher below opening 1
            double stretcher1Y = height - (2 * mt34) - openings[0];
            specs.Add(BuildDepthSpec("Drawer Stretcher",
                partDepth: stretcherWidth,
                mortiseBottomY: stretcher1Y,
                flushFace: TenonFlushFace.Bottom,
                s: drwStretcherSettings,
                xOffset: depth - stretcherWidth,
                forceTwoTenons: true));
        }
        else if (string.Equals(baseCab.Style, CabinetStyles.Base.Drawer, StringComparison.OrdinalIgnoreCase) && baseCab.DrwCount > 1)
        {
            // Drawer cabinet with multiple drawers: stretchers between each pair
            double runningY = height - (2 * mt34);  // Start below top panel
            for (int i = 0; i < baseCab.DrwCount - 1; i++)
            {
                if (openings[i] <= 0) break;

                runningY -= openings[i];       // Subtract opening height to reach stretcher position
                specs.Add(BuildDepthSpec($"Drawer Stretcher {i + 1}",
                    partDepth: stretcherWidth,
                    mortiseBottomY: runningY,
                    flushFace: TenonFlushFace.Bottom,
                    s: drwStretcherSettings,
                    xOffset: depth - stretcherWidth,
                    forceTwoTenons: true));
                runningY -= mt34;              // Subtract stretcher thickness to start next opening
            }
        }

        // ── Toekick (flush Back face, comb runs in height direction) ─────────
        if (tkH > 0 && tkD > 0)
        {
            specs.Add(BuildHeightSpec("Toekick",
                edgeLength: tkH - 0.5,
                xPosition: depth - tkD - mt34,
                bottomY: 0.5,
                flushFace: TenonFlushFace.Back,
                s: toekickSettings,
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