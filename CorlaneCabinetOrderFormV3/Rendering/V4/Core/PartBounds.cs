namespace CorlaneCabinetOrderFormV3.Rendering.V4.Core;

/// <summary>
/// Represents the 2D dimensions of a part in the export plane.
/// Width = X-axis (Left→Right), Height = Y-axis (Bottom→Top)
/// </summary>
internal record PartBounds(double Width, double Height);

[Flags]
internal enum Edge
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    All = Left | Right | Top | Bottom
}

internal record PartInfo(
    string Name,
    PartBounds Bounds,
    string Material,
    int Quantity,
    Edge TenonEdges,
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
