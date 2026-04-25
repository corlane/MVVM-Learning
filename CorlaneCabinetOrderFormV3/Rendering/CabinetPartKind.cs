namespace CorlaneCabinetOrderFormV3.Rendering;

// =============================================================================
// CabinetPartKind.cs
// CorlaneCabinetOrderFormV3.Rendering
//
// Enum that tags every panel or structural part built by CabinetPartFactory
// with its physical role in the cabinet. Used throughout the rendering pipeline
// to drive part-specific behavior without needing separate class hierarchies.
//
// Key uses by role:
//   - Edgebanding rules: Toekick, TopStretcherBack, BackBase34/14, BackUpper14,
//     SinkStretcher, and DrawerBoxBottom are excluded from edgebanding entirely.
//     Door and DrawerFront always get all four edges banded (TBLR). Panel and
//     FillerFront edges must be explicitly provided by the caller.
//   - Upper end panel bottom edge: LeftEnd/RightEnd on an UpperCabinetModel
//     triggers the automatic "PVC Hardrock Maple" bottom-edge accumulation path.
//   - Corner90 arc shapes: Deck and Top with >4 polygon points activate the
//     arc-edge edgebanding path in CabinetPartFactory.
//   - BOM / material totals: partKind is used to label cut-list rows and filter
//     which parts contribute to material area and edgebanding length accumulators.
//   - Unspecified: placeholder for call sites not yet tagged; treated as a
//     standard structural part with front-edge-only edgebanding.
// =============================================================================


/// <summary>
/// Identifies the structural role of a panel within a cabinet,
/// enabling part-specific logic (edgebanding rules, material totals, BOM labels, etc.).
/// </summary>
internal enum CabinetPartKind
{
    /// <summary>Default for call sites not yet tagged.</summary>
    Unspecified,

    LeftEnd,
    RightEnd,
    Deck,
    Top,
    TopStretcherFront,
    TopStretcherBack,
    Toekick,
    BackBase34,
    BackBase14,
    BackUpper34,
    BackUpper14,
    Shelf,
    DrawerStretcher,
    SinkStretcher,
    Nailer,
    Door,
    DrawerFront,
    DrawerBoxSide,
    DrawerBoxFront,
    DrawerBoxBack,
    DrawerBoxBottom,
    FillerEnd,
    FillerFront,
    Panel,
}