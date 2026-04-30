namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Which face of a tenon panel is flush (unrouted).
/// The OPPOSITE face receives the CNC thinning pocket operation.
/// Controls the mortise slot Y offset on the mating end panel.
/// </summary>
internal enum TenonFlushFace
{
    /// <summary>Top face is flush. Pocket routed from bottom face. (Deck)</summary>
    Top,

    /// <summary>Bottom face is flush. Pocket routed from top face. (Top, Drawer Stretchers)</summary>
    Bottom,

    /// <summary>Front-facing face is flush. Pocket routed from back face. (Nailer)</summary>
    Front,

    /// <summary>Back-facing face is flush. Pocket routed from front face. (Toekick)</summary>
    Back,
}