using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Pre-parsed numeric dimensions for an UpperCabinetModel.
/// Eliminates repeated FractionToDouble calls throughout the builder.
/// </summary>
internal readonly record struct UpperCabinetDimensions
{
    public double Width { get; init; }
    public double Height { get; init; }
    public double Depth { get; init; }
    public double BackThickness { get; init; }
    public double LeftFrontWidth { get; init; }
    public double RightFrontWidth { get; init; }
    public double LeftDepth { get; init; }
    public double RightDepth { get; init; }
    public double LeftBackWidth { get; init; }
    public double RightBackWidth { get; init; }
    public double InteriorWidth { get; init; }
    public double InteriorDepth { get; init; }
    public double InteriorHeight { get; init; }
    public double ShelfDepth { get; init; }
    public double DoorGap { get; init; }
    public double DoorLeftReveal { get; init; }
    public double DoorRightReveal { get; init; }
    public double DoorTopReveal { get; init; }
    public double DoorBottomReveal { get; init; }
    public double DoorSideReveal { get; init; }

    internal static UpperCabinetDimensions From(UpperCabinetModel upperCab)
    {
        double mt34 = MaterialDefaults.Thickness34;

        double width = ConvertDimension.FractionToDouble(upperCab.Width);
        double height = ConvertDimension.FractionToDouble(upperCab.Height);
        double depth = ConvertDimension.FractionToDouble(upperCab.Depth);
        double backThickness = ConvertDimension.FractionToDouble(upperCab.BackThickness);
        if (backThickness == 0.25) { depth -= backThickness; }

        double interiorWidth = width - (mt34 * 2);
        double interiorDepth = depth - backThickness;
        double interiorHeight = height - (mt34 * 2);

        double doorLeftReveal = ConvertDimension.FractionToDouble(upperCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(upperCab.RightReveal);

        return new UpperCabinetDimensions
        {
            Width = width,
            Height = height,
            Depth = depth,
            BackThickness = backThickness,
            LeftFrontWidth = ConvertDimension.FractionToDouble(upperCab.LeftFrontWidth),
            RightFrontWidth = ConvertDimension.FractionToDouble(upperCab.RightFrontWidth),
            LeftDepth = ConvertDimension.FractionToDouble(upperCab.LeftDepth),
            RightDepth = ConvertDimension.FractionToDouble(upperCab.RightDepth),
            LeftBackWidth = ConvertDimension.FractionToDouble(upperCab.LeftBackWidth),
            RightBackWidth = ConvertDimension.FractionToDouble(upperCab.RightBackWidth),
            InteriorWidth = interiorWidth,
            InteriorDepth = interiorDepth,
            InteriorHeight = interiorHeight,
            ShelfDepth = interiorDepth - 0.125,
            DoorGap = ConvertDimension.FractionToDouble(upperCab.GapWidth),
            DoorLeftReveal = doorLeftReveal,
            DoorRightReveal = doorRightReveal,
            DoorTopReveal = ConvertDimension.FractionToDouble(upperCab.TopReveal),
            DoorBottomReveal = ConvertDimension.FractionToDouble(upperCab.BottomReveal),
            DoorSideReveal = (doorLeftReveal + doorRightReveal) / 2,
        };
    }
}