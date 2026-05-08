using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// All shop-specific lock dado parameters in one named place.
/// Adjust any value here to propagate across all part outline and mortise calculations.
/// </summary>
internal sealed record LockDadoSettings
{
    // ── Blind zone ────────────────────────────────────────────────────────────

    /// <summary>Distance from the front face where the tenon/mortise zone begins.</summary>
    public double BlindStart { get; init; } = 2.75;

    /// <summary>Distance from the back edge where the tenon/mortise zone ends.</summary>
    public double BlindStop { get; init; } = 2.75;

    // ── Per-edge blind overrides (nullable, fall back to global above) ──

    public double? BlindStartLeft { get; init; } = 2;
    public double? BlindStopLeft { get; init; } = 2;
    public double? BlindStartRight { get; init; } = 2;
    public double? BlindStopRight { get; init; } = 2;
    public double? BlindStartTop { get; init; } = 2;
    public double? BlindStopTop { get; init; } = 2;
    public double? BlindStartBottom { get; init; } = 2;
    public double? BlindStopBottom { get; init; } = 2;

    /// <summary>Resolve effective BlindStart for a given edge, falling back to global.</summary>
    public double ResolveBlindStart(EdgeDesignators edge) => edge switch
    {
        EdgeDesignators.Left => BlindStartLeft ?? BlindStart,
        EdgeDesignators.Right => BlindStartRight ?? BlindStart,
        EdgeDesignators.Top => BlindStartTop ?? BlindStart,
        EdgeDesignators.Bottom => BlindStartBottom ?? BlindStart,
        _ => BlindStart
    };

    /// <summary>Resolve effective BlindStop for a given edge, falling back to global.</summary>
    public double ResolveBlindStop(EdgeDesignators edge) => edge switch
    {
        EdgeDesignators.Left => BlindStopLeft ?? BlindStop,
        EdgeDesignators.Right => BlindStopRight ?? BlindStop,
        EdgeDesignators.Top => BlindStopTop ?? BlindStop,
        EdgeDesignators.Bottom => BlindStopBottom ?? BlindStop,
        _ => BlindStop
    };

    // ── Dado / tenon protrusion ───────────────────────────────────────────────

    /// <summary>
    /// How far the tenon protrudes from the tenon panel edge into the end panel face.
    /// Also the nominal routed depth of the mortise pocket before clearance is added.
    /// </summary>
    public double DadoDepth { get; init; } = 0.375;

    /// <summary>
    /// Extra depth added to the mortise pocket so the tenon never bottoms out (1mm).
    /// Full mortise pocket depth = DadoDepth + MortiseDepthClearance.
    /// </summary>
    public double MortiseDepthClearance { get; init; } = 0.03937;   // 1mm

    // ── Tenon thickness ───────────────────────────────────────────────────────

    /// <summary>
    /// Thickness of the tenon after the thinning pocket is routed on the CNC.
    /// The tenon is flush on one face; the other face is routed down to this thickness.
    /// Typically MaterialThickness34 / 2.
    /// </summary>
    public double TenonThickness { get; init; } = MaterialDefaults.Thickness34 * 0.4;

    /// <summary>
    /// Clearance added to the mortise slot height so the tenon slides in freely (0.5mm).
    /// Full mortise slot height = TenonThickness + TenonClearance.
    /// </summary>
    public double TenonClearance { get; init; } = 0.01969;   // 0.5mm

    // ── Tenon thinning pocket oversize ────────────────────────────────────────

    /// <summary>
    /// The thinning pocket on the tenon panel face extends beyond the usable tenon
    /// zone by this amount on EACH end (in the depth direction), giving the router
    /// a clean run-in and run-out. 1mm each end.
    /// </summary>
    public double TenonPocketOversize { get; init; } = 0.03937;   // 1mm

    // ── Mortise lateral oversize ──────────────────────────────────────────────

    /// <summary>
    /// Each mortise pocket is wider than its matching tenon by this amount on EACH
    /// end in the comb direction, providing assembly clearance.
    /// Total mortise length = tenonSegmentLength + (2 × MortiseOversize).
    /// </summary>
    public double MortiseOversize { get; init; } = 0.25;

    // ── Gap / screw access ────────────────────────────────────────────────────

    /// <summary>Fixed width of each screw-access gap between tenon segments.</summary>
    public double GapWidth { get; init; } = 1.5;

    /// <summary>One gap per approximately this many inches of usable tenon zone.</summary>
    public double GapSpacing { get; init; } = 10.0;

    // ── Screw pilot hole ──────────────────────────────────────────────────────

    /// <summary>Diameter of the CNC-drilled pilot hole at each gap center. 5mm.</summary>
    public double ScrewPilotHoleDiameter { get; init; } = 0.19685;   // 5mm

    // ── Derived helpers ───────────────────────────────────────────────────────

    /// <summary>Usable tenon zone length between the two blind stops.</summary>
    public double UsableZone(double edgeLength) => edgeLength - BlindStart - BlindStop;

    /// <summary>Number of screw-access gaps for a given edge length.</summary>
    public int GapCount(double edgeLength)
    {
        double usable = UsableZone(edgeLength);
        return usable <= 0 ? 0 : (int)Math.Floor(usable / GapSpacing);
    }

    /// <summary>Full mortise pocket depth into the end panel face.</summary>
    public double MortisePocketDepth => DadoDepth + MortiseDepthClearance;

    /// <summary>Full mortise slot height on the end panel face (perpendicular to comb direction).</summary>
    public double MortiseSlotHeight => TenonThickness + TenonClearance;

    /// <summary>How deep the thinning pocket is routed into the tenon panel face.</summary>
    public double TenonThinningDepth(double panelThickness) => panelThickness - TenonThickness;

    public static LockDadoSettings Default { get; } = new();
}