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
//     Populated for both Base and Upper cabinets from their respective
//     dimension structs (BaseCabinetDimensions / UpperCabinetDimensions).
//
//   - Drawer boxes: the cut-list width/depth and a per-opening list of heights,
//     sized for the specific drawer slide hardware being used. Base only.
//
//   - Rollouts: width/height/depth of rollout trays built inside base cabinets.
//     Base only.
//
//   - Doors: the finished door width and height for a single door opening,
//     before any pair-splitting or overlay adjustment. Populated for both
//     Base (Standard) and Upper (Standard) cabinets.
//
//   - Drawer fronts: the finished front width and a per-front list of heights,
//     computed from the opening stack and any custom height overrides.
//     Base only.
//
// Produced by: BaseCabinetBuilder.BuildBase, UpperCabinetBuilder.BuildUpper
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
    // Populated for Base and Upper cabinets.
    public double InteriorWidth { get; set; }
    public double InteriorDepth { get; set; }
    public double InteriorHeight { get; set; }
    public double ShelfDepth { get; set; }

    // ── Drawer boxes (Base only) ──
    public double DrawerBoxWidth { get; set; }
    public double DrawerBoxDepth { get; set; }
    public List<double> DrawerBoxHeights { get; } = [];

    // ── Rollouts (Base only) ──
    public double RolloutWidth { get; set; }
    public double RolloutHeight { get; set; }
    public double RolloutDepth { get; set; }

    // ── Doors (Base and Upper) ──
    // For 2-door cabinets, stores the per-door width after pair-splitting.
    public double DoorWidth { get; set; }
    public double DoorHeight { get; set; }

    // For Corner90 cabinets, door widths are independent (left vs right front).
    public double Door1Width { get; set; }
    public double Door2Width { get; set; }

    // ── Drawer fronts (Base only) ──
    public double DrawerFrontWidth { get; set; }
    public List<double> DrawerFrontHeights { get; } = [];
}