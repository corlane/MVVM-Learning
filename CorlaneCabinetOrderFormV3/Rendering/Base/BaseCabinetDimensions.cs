using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

/// <summary>
/// Pre-parsed numeric dimensions for a BaseCabinetModel.
/// Eliminates repeated FractionToDouble calls throughout the builder.
/// </summary>
internal readonly record struct BaseCabinetDimensions
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
    public double TKHeight { get; init; }
    public double TKDepth { get; init; }
    public double Opening1Height { get; init; }
    public double Opening2Height { get; init; }
    public double Opening3Height { get; init; }
    public double Opening4Height { get; init; }
    public double DrwFront1Height { get; init; }
    public double DrwFront2Height { get; init; }
    public double DrwFront3Height { get; init; }
    public double DrwFront4Height { get; init; }
    public double InteriorWidth { get; init; }
    public double InteriorDepth { get; init; }
    public double InteriorHeight { get; init; }
    public double ShelfDepth { get; init; }
    public double BaseDoorGap { get; init; }
    public double DoorLeftReveal { get; init; }
    public double DoorRightReveal { get; init; }
    public double DoorTopReveal { get; init; }
    public double DoorBottomReveal { get; init; }
    public double DoorSideReveal { get; init; }
    public double DrawerBoxDepth { get; init; }

    internal static BaseCabinetDimensions From(BaseCabinetModel baseCab)
    {
        double mt34 = MaterialDefaults.Thickness34;

        double width = ConvertDimension.FractionToDouble(baseCab.Width);
        double height = ConvertDimension.FractionToDouble(baseCab.Height);
        double depth = ConvertDimension.FractionToDouble(baseCab.Depth);
        double backThickness = ConvertDimension.FractionToDouble(baseCab.BackThickness);
        if (backThickness == 0.25) { depth -= backThickness; }

        double tkHeight = baseCab.HasTK ? ConvertDimension.FractionToDouble(baseCab.TKHeight ?? "4") : 0;
        double tkDepth = baseCab.HasTK ? ConvertDimension.FractionToDouble(baseCab.TKDepth ?? "3") : 0;

        double interiorWidth = width - (mt34 * 2);
        double interiorDepth = depth - backThickness;
        double interiorHeight = height - (mt34 * 2) - tkHeight;

        double shelfDepth = string.Equals(baseCab.ShelfDepth, CabinetOptions.ShelfDepth.HalfDepth, StringComparison.OrdinalIgnoreCase)
            ? interiorDepth / 2
            : interiorDepth;

        double doorLeftReveal = ConvertDimension.FractionToDouble(baseCab.LeftReveal);
        double doorRightReveal = ConvertDimension.FractionToDouble(baseCab.RightReveal);

        double dbxDepth = interiorDepth - 1;
        if (depth >= 10.625 && depth < 13.625) dbxDepth = 9;
        if (depth >= 13.625 && depth < 16.625) dbxDepth = 12;
        if (depth >= 16.625 && depth < 19.625) dbxDepth = 15;
        if (depth >= 19.625 && depth < 22.625) dbxDepth = 18;
        if (depth >= 22.625) dbxDepth = 21;

        return new BaseCabinetDimensions
        {
            Width = width,
            Height = height,
            Depth = depth,
            BackThickness = backThickness,
            LeftFrontWidth = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth),
            RightFrontWidth = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth),
            LeftDepth = ConvertDimension.FractionToDouble(baseCab.LeftDepth),
            RightDepth = ConvertDimension.FractionToDouble(baseCab.RightDepth),
            LeftBackWidth = ConvertDimension.FractionToDouble(baseCab.LeftBackWidth),
            RightBackWidth = ConvertDimension.FractionToDouble(baseCab.RightBackWidth),
            TKHeight = tkHeight,
            TKDepth = tkDepth,
            Opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1),
            Opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2),
            Opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3),
            Opening4Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight4),
            DrwFront1Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1),
            DrwFront2Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2),
            DrwFront3Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3),
            DrwFront4Height = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4),
            InteriorWidth = interiorWidth,
            InteriorDepth = interiorDepth,
            InteriorHeight = interiorHeight,
            ShelfDepth = shelfDepth,
            BaseDoorGap = ConvertDimension.FractionToDouble(baseCab.GapWidth),
            DoorLeftReveal = doorLeftReveal,
            DoorRightReveal = doorRightReveal,
            DoorTopReveal = ConvertDimension.FractionToDouble(baseCab.TopReveal),
            DoorBottomReveal = ConvertDimension.FractionToDouble(baseCab.BottomReveal),
            DoorSideReveal = (doorLeftReveal + doorRightReveal) / 2,
            DrawerBoxDepth = dbxDepth,
        };
    }
}