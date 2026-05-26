namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

/// <summary>
/// Lightweight 2D vector for geometry calculations.
/// </summary>
public readonly struct Vector2(double x, double y)
{
    public double X { get; } = x;
    public double Y { get; } = y;

    public override string ToString() => $"({X}, {Y})";
}
