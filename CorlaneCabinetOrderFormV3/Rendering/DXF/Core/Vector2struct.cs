namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

/// <summary>
/// Lightweight 2D vector for geometry calculations.
/// </summary>
public readonly struct Vector2
{
    public double X { get; }
    public double Y { get; }

    public Vector2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"({X}, {Y})";
}
