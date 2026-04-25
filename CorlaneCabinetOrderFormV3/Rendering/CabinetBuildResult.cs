using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

// CabinetBuildResult.cs
// ─────────────────────────────────────────────────────────────────────────────
// A mutable result bag populated by base and upper cabinet builders as they
// compute dimensions during a single build pass. Acts as the single source of
// truth for all derived cabinet measurements — eliminating duplicated arithmetic
// across the builder, BOM accumulator, and list view-models.
//
// Properties are grouped by concern:
//   - Interior dimensions: usable inside width/depth/height and shelf depth,
//     computed from the nominal cabinet size minus material thicknesses.
//
//   - Drawer boxes: the cut-list width/depth and a per-opening list of heights,
//     sized for the specific drawer slide hardware being used.
//
//   - Rollouts: width/height/depth of rollout trays built inside base cabinets.
//
//   - Doors: the finished door width and height for a single door opening,
//     before any pair-splitting or overlay adjustment.
//
//   - Drawer fronts: the finished front width and a per-front list of heights,
//     computed from the opening stack and any custom height overrides.
//
// Produced by: BaseCabinetBuilder.Build / UpperCabinetBuilder.BuildUpper
// Consumed by: CabinetPreviewBuilder (testing), DoorSizesListViewModel,
//              DrawerBoxSizesListViewModel, and material/edge-total accumulators.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Populated by the cabinet builders as they compute values.
/// Single source of truth — no duplicated arithmetic.
/// </summary>
internal sealed class CabinetBuildResult
{
    // ── Core interior dimensions ──
    public double InteriorWidth { get; set; }
    public double InteriorDepth { get; set; }
    public double InteriorHeight { get; set; }
    public double ShelfDepth { get; set; }

    // ── Drawer boxes ──
    public double DrawerBoxWidth { get; set; }
    public double DrawerBoxDepth { get; set; }
    public List<double> DrawerBoxHeights { get; } = [];

    // ── Rollouts ──
    public double RolloutWidth { get; set; }
    public double RolloutHeight { get; set; }
    public double RolloutDepth { get; set; }

    // ── Doors ──
    public double DoorWidth { get; set; }
    public double DoorHeight { get; set; }

    // ── Drawer fronts ──
    public double DrawerFrontWidth { get; set; }
    public List<double> DrawerFrontHeights { get; } = [];
}