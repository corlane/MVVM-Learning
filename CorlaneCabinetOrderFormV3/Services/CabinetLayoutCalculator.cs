namespace CorlaneCabinetOrderFormV3.Services;

using CorlaneCabinetOrderFormV3.Models;
/// <summary>
/// Pure math for drawer/opening height calculations.
/// Extracted from BaseCabinetViewModel so it can be unit-tested without a VM.
/// </summary>
public static class CabinetLayoutCalculator
{
    public const double MaterialThickness34 = 0.75;

    public record LayoutInputs(
        string Style,
        int DrwCount,
        double Height,
        double TkHeight,
        bool HasTK,
        double TopReveal,
        double BottomReveal,
        double GapWidth,
        double Opening1,
        double Opening2,
        double Opening3,
        double Opening4,
        double DrwFront1,
        double DrwFront2,
        double DrwFront3,
        double DrwFront4);

    public record LayoutResult(
        double Opening1,
        double Opening2,
        double Opening3,
        double Opening4,
        double DrwFront1,
        double DrwFront2,
        double DrwFront3,
        double DrwFront4);

    /// <summary>
    /// Mirrors the "resize opening heights" path — given opening heights, compute drawer front heights.
    /// </summary>
    public static LayoutResult ComputeFromOpenings(LayoutInputs input)
    {
        double effectiveHeight = input.Height - (input.HasTK ? input.TkHeight : 0);
        double deckThickness = (input.DrwCount + 1) * MaterialThickness34;

        double o1 = input.Opening1, o2 = input.Opening2, o3 = input.Opening3, o4 = input.Opening4;
        double f1 = input.DrwFront1, f2 = input.DrwFront2, f3 = input.DrwFront3, f4 = input.DrwFront4;

        if (input.Style == CabinetStyles.Base.Standard)
        {
            if (input.DrwCount == 1)
            {
                f1 = o1 + (1.5 * MaterialThickness34) - input.TopReveal - (input.GapWidth / 2);
            }
        }
        else if (input.Style == CabinetStyles.Base.Drawer)
        {
            if (input.DrwCount == 1)
            {
                o1 = effectiveHeight - (2 * MaterialThickness34);
                f1 = o1 + (2 * MaterialThickness34) - input.TopReveal - input.BottomReveal;
            }
            else if (input.DrwCount == 2)
            {
                o2 = effectiveHeight - deckThickness - o1;
                f1 = o1 + (1.5 * MaterialThickness34) - input.TopReveal - (input.GapWidth / 2);
                f2 = o2 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
            else if (input.DrwCount == 3)
            {
                o3 = effectiveHeight - deckThickness - o1 - o2;
                f1 = o1 + (1.5 * MaterialThickness34) - input.TopReveal - (input.GapWidth / 2);
                f2 = o2 + MaterialThickness34 - input.GapWidth;
                f3 = o3 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
            else if (input.DrwCount == 4)
            {
                o4 = effectiveHeight - deckThickness - o1 - o2 - o3;
                f1 = o1 + (1.5 * MaterialThickness34) - input.TopReveal - (input.GapWidth / 2);
                f2 = o2 + MaterialThickness34 - input.GapWidth;
                f3 = o3 + MaterialThickness34 - input.GapWidth;
                f4 = o4 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
        }

        return new(o1, o2, o3, o4, f1, f2, f3, f4);
    }

    /// <summary>
    /// Mirrors the "resize drawer front heights" path — given drawer front heights, compute opening heights.
    /// </summary>
    public static LayoutResult ComputeFromDrawerFronts(LayoutInputs input)
    {
        double effectiveHeight = input.Height - (input.HasTK ? input.TkHeight : 0);
        double deckThickness = (input.DrwCount + 1) * MaterialThickness34;

        double o1 = input.Opening1, o2 = input.Opening2, o3 = input.Opening3, o4 = input.Opening4;
        double f1 = input.DrwFront1, f2 = input.DrwFront2, f3 = input.DrwFront3, f4 = input.DrwFront4;

        if (input.Style == CabinetStyles.Base.Standard)
        {
            if (input.DrwCount == 1)
            {
                o1 = f1 + input.TopReveal + (input.GapWidth / 2) - (1.5 * MaterialThickness34);
                f1 = o1 + (1.5 * MaterialThickness34) - input.TopReveal - (input.GapWidth / 2);
            }
        }
        else if (input.Style == CabinetStyles.Base.Drawer)
        {
            if (input.DrwCount == 1)
            {
                o1 = effectiveHeight - (2 * MaterialThickness34);
                f1 = o1 + (2 * MaterialThickness34) - input.TopReveal - input.BottomReveal;
            }
            else if (input.DrwCount == 2)
            {
                o1 = f1 + input.TopReveal + (input.GapWidth / 2) - (1.5 * MaterialThickness34);
                o2 = effectiveHeight - deckThickness - o1;
                f2 = o2 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
            else if (input.DrwCount == 3)
            {
                o1 = f1 + input.TopReveal + (input.GapWidth / 2) - (1.5 * MaterialThickness34);
                o2 = f2 + input.GapWidth - MaterialThickness34;
                o3 = effectiveHeight - deckThickness - o1 - o2;
                f3 = o3 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
            else if (input.DrwCount == 4)
            {
                o1 = f1 + input.TopReveal + (input.GapWidth / 2) - (1.5 * MaterialThickness34);
                o2 = f2 + input.GapWidth - MaterialThickness34;
                o3 = f3 + input.GapWidth - MaterialThickness34;
                o4 = effectiveHeight - deckThickness - o1 - o2 - o3;
                f4 = o4 + (1.5 * MaterialThickness34) - input.BottomReveal - (input.GapWidth / 2);
            }
        }

        return new(o1, o2, o3, o4, f1, f2, f3, f4);
    }

    /// <summary>
    /// Equalize all drawer fronts evenly. Same formula as DrawerFrontEqualizationTests.
    /// </summary>
    public static double EqualizeAll(double height, double topReveal, double bottomReveal, double gapWidth, int drwCount)
    {
        double total = height - topReveal - bottomReveal - (gapWidth * (drwCount - 1));
        return total / drwCount;
    }

    /// <summary>
    /// Equalize bottom drawer fronts — top drawer fixed, rest split the remainder.
    /// </summary>
    public static double EqualizeBottom(double height, double topReveal, double bottomReveal, double gapWidth, int drwCount, double topDrawerHeight)
    {
        double total = height - topReveal - bottomReveal - (gapWidth * (drwCount - 1)) - topDrawerHeight;
        return total / (drwCount - 1);
    }

    /// <summary>
    /// Angle-front width = distance between two corner points.
    /// </summary>
    public static double ComputeAngleFrontWidth(double leftDepth, double rightDepth, double leftBackWidth, double rightBackWidth)
    {
        double p0x = leftDepth;
        double p0y = MaterialThickness34;
        double p1x = rightBackWidth - MaterialThickness34;
        double p1y = leftBackWidth - rightDepth;

        double vx = p1x - p0x;
        double vy = p1y - p0y;
        return Math.Sqrt((vx * vx) + (vy * vy));
    }
}