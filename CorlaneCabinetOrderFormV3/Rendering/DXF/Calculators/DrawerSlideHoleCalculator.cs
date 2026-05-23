using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

internal static class DrawerSlideHoleCalculator
{
    private const double MmToInch = 1.0 / 25.4;

    // 5mm diameter hole
    private const double HoleRadius = 0.19685 / 2.0;

    // Slide pattern offset from bottom of opening
    private const double BlumPatternHeightAboveStretcher = 1.4961;
    private const double AccuridePatternHeightAboveStretcher = 1.4961; // TODO NEED THE ACTUAL NUMBER

    // Fixed Y-offsets (inches)
    private const double BlumHole1Y = 0.3937;
    private const double BlumHole2Y = 1.4567;
    private const double BlumHole3Y_Default = 2.7165;

    private const double AccurideInsetFromFront = 2;
    private const double AccurideHole1Y = (35 + AccurideInsetFromFront) * MmToInch;
    private const double AccurideHole2Y = (AccurideHole1Y + 224) * MmToInch;
    



    internal static List<(double x, double y, double radius)> Compute(
        BaseCabinetModel baseCab,
        JoineryConfig joinery,
        double materialThickness34)
    {
        var holes = new List<(double, double, double)>();

        var dim = BaseCabinetDimensions.From(baseCab);

        if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
        {
            ComputeForOpenings(holes, dim, baseCab, materialThickness34,
                openingHeights: [dim.Opening1Height],
                drillFlags: [baseCab.DrillSlideHolesOpening1]);
        }
        else if (baseCab.Style == CabinetStyles.Base.Drawer)
        {
            ComputeForOpenings(holes, dim, baseCab, materialThickness34,
                openingHeights: [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height],
                drillFlags: [baseCab.DrillSlideHolesOpening1, baseCab.DrillSlideHolesOpening2, baseCab.DrillSlideHolesOpening3, baseCab.DrillSlideHolesOpening4]);
        }

        return holes;
    }

    private static void ComputeForOpenings(
        List<(double x, double y, double radius)> holes,
        BaseCabinetDimensions dim,
        BaseCabinetModel baseCab,
        double mt34,
        double[] openingHeights,
        bool[] drillFlags)
    {
        double runningX = 0;

        for (int i = 0; i < baseCab.DrwCount && i < openingHeights.Length; i++)
        {
            if (!drillFlags[i])
            {
                runningX += mt34 + openingHeights[i];
                continue;
            }

            runningX += mt34 + openingHeights[i];
            double slidePositionX = runningX - BlumPatternHeightAboveStretcher;

            if (baseCab.DrwStyle.Contains("Blum"))
            {
                ApplyBlumHolePattern(holes, dim, slidePositionX);
            }

            else
            {
                ApplyAccurideHolePattern(holes, dim, slidePositionX);
            }
        }
    }

    private static void ApplyBlumHolePattern(
        List<(double x, double y, double radius)> holes,
        BaseCabinetDimensions dim,
        double slidePositionX)
    {
        double hole1Y = BlumHole1Y;
        double hole2Y = BlumHole2Y;
        double hole3Y = BlumHole3Y_Default;
        double hole4Y = 0;
        double hole5Y = 0;

        if (dim.Depth >= 24)
        {
            hole4Y = hole2Y + 224 * MmToInch;
            hole5Y = hole4Y + 256 * MmToInch;
        }
        else if (dim.Depth >= 21)
        {
            hole4Y = hole2Y + 224 * MmToInch;
            hole5Y = hole4Y + 192 * MmToInch;
        }
        else if (dim.Depth >= 18)
        {
            hole4Y = hole2Y + 128 * MmToInch;
            hole5Y = hole4Y + 192 * MmToInch;
        }
        else if (dim.Depth >= 15)
        {
            hole4Y = hole2Y + 128 * MmToInch;
            hole5Y = hole4Y + 96 * MmToInch;
        }
        else // dim.Depth >= 12 && < 15
        {
            hole3Y = hole2Y + 96 * MmToInch;
            hole4Y = hole3Y + 96 * MmToInch;
            hole5Y = 0; // not used for this depth range
        }

        // Always add holes 1, 2, 3
        holes.Add((slidePositionX, hole1Y, HoleRadius));
        holes.Add((slidePositionX, hole2Y, HoleRadius));
        holes.Add((slidePositionX, hole3Y, HoleRadius));

        // Holes 4 and 5 only for depth >= 15
        if (dim.Depth >= 15)
        {
            holes.Add((slidePositionX, hole4Y, HoleRadius));
            holes.Add((slidePositionX, hole5Y, HoleRadius));
        }
        // For depth < 15, hole3Y and hole4Y were already computed above
        // and hole3Y was added in the "always" block; hole4Y is added below
        else
        {
            holes.Add((slidePositionX, hole4Y, HoleRadius));
        }
    }


    private static void ApplyAccurideHolePattern(
    List<(double x, double y, double radius)> holes,
    BaseCabinetDimensions dim,
    double slidePositionX)
    {
        double hole1Y = AccurideHole1Y;
        double hole2Y = AccurideHole2Y;

        holes.Add((slidePositionX, hole1Y, HoleRadius));
        holes.Add((slidePositionX, hole2Y, HoleRadius));

        double depth = dim.Depth;

        if (depth < 17)
        {
            // No additional holes for shallow cabinets
            return;
        }

        double hole3OffsetMm = 0;
        double hole4OffsetMm = 0;

        if (depth <= 19)
        {
            // 16" slide
            hole3OffsetMm = 224;
        }
        else if (depth <= 21)
        {
            // 18" slide
            hole3OffsetMm = 352;
        }
        else if (depth <= 23)
        {
            // 20" slide
            hole3OffsetMm = 416;
        }
        else if (depth <= 25)
        {
            // 22" slide
            hole3OffsetMm = 352;
            hole4OffsetMm = 448;
        }
        else if (depth <= 27)
        {
            // 24" slide
            hole3OffsetMm = 352;
            hole4OffsetMm = 480;
        }
        else
        {
            // 26" & 28" slides
            hole3OffsetMm = 352;
            hole4OffsetMm = 544;
        }

        if (hole3OffsetMm > 0)
        {
            holes.Add((slidePositionX, hole1Y + hole3OffsetMm * MmToInch, HoleRadius));
        }

        if (hole4OffsetMm > 0)
        {
            holes.Add((slidePositionX, hole1Y + hole4OffsetMm * MmToInch, HoleRadius));
        }
    }
}