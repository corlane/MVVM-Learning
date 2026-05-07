namespace CorlaneCabinetOrderFormV3.Rendering.V4.Core;

/// <summary>
/// Represents the 2D dimensions of a part in the export plane.
/// Width = X-axis (Left→Right), Height = Y-axis (Bottom→Top)
/// </summary>
internal record PartBounds(double Width, double Height);

[Flags]
internal enum TenonEdge
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    All = Left | Right | Top | Bottom
}

[Flags]
internal enum MortiseEdge
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    All = Left | Right | Top | Bottom
}

[Flags]
internal enum ScrewHoleEdge
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Bottom = 1 << 2,
    Top = 1 << 3
}


internal record PartInfo(
    string Name,
    PartBounds Bounds,
    string Material,
    int Quantity,
    TenonEdge TenonEdges,
    MortiseEdge MortiseEdges,
    ScrewHoleEdge ScrewHoleEdges,
    string? EdgeBand,
    string? Notes,
    double TkHeight = 0,
    double TkDepth = 0);

/// <summary>
/// Standardized joinery configuration for the new pipeline.
/// </summary>
internal record JoineryConfig(
    double BlindStart,
    double BlindStop,
    double DadoDepth,
    double MortiseDepthClearance,
    double TenonThickness,
    double TenonClearance,
    double TenonPocketOversize,
    double MortiseOversize,
    double GapWidth,
    double GapSpacing,
    double ScrewPilotHoleDiameter,
    double Thickness34,
    double TenonThinningOverrun = 0.375);
