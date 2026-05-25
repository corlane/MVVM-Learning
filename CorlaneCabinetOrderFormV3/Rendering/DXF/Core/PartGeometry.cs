namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

/// <summary>
/// Represents the computed geometry for a single part, ready for DXF export.
/// </summary>
internal record PartGeometry(
    PartInfo PartInfo,
    List<Vector2> OutlineVertices,      // Closed polyline vertices
    List<(double x1, double x2, double y1, double y2)> TenonThinningPockets, // Tenon pockets
    List<(double x1, double x2, double y1, double y2)> MortisePockets,  // Mortise pockets (if applicable)
    List<(double x1, double x2, double y1, double y2)> MortisePocketsThru,  // Mortise pockets thru (if applicable)
    List<(double x, double y, double radius)> Holes, // Screw/Hinge holes
    List<(double x, double y, double radius)> HolesThru); // Screw/Hinge holes
