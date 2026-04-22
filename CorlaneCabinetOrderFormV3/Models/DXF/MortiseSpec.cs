namespace CorlaneCabinetOrderFormV3.Models;

/// <summary>
/// All geometry for one lock dado joint as it appears on the end panel INSIDE face.
///
/// End panel local coords (face up, laid flat):
///   X = depth direction  (0 = front face,  depthIn = back edge)
///   Y = height direction (0 = bottom edge)
///
/// Depth-direction joints  (deck, top, stretchers, nailer):
///   Pockets have a fixed Y range and varying X ranges along the comb.
///
/// Height-direction joints (toekick):
///   Pockets have a fixed X range and varying Y ranges along the comb.
/// </summary>
internal sealed record MortiseSpec
{
    /// <summary>Human-readable label for DXF annotation (e.g. "Deck", "Toekick").</summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// Discrete mortise pocket rectangles (X1, X2, Y1, Y2) in end panel face coords.
    /// Rendered on the MACHINING_MORTISE DXF layer.
    /// </summary>
    public IReadOnlyList<(double X1, double X2, double Y1, double Y2)> Pockets { get; init; } = [];

    /// <summary>
    /// CNC pilot hole positions (CenterX, CenterY, Diameter) in end panel face coords.
    /// Rendered on the MACHINING_SCREW_HOLES DXF layer.
    /// </summary>
    public IReadOnlyList<(double CenterX, double CenterY, double Diameter)> ScrewHoles { get; init; } = [];
}