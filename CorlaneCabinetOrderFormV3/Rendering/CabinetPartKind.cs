namespace CorlaneCabinetOrderFormV3.Rendering;

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