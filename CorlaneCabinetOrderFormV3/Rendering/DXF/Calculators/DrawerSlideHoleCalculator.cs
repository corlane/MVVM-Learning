//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
//using System.Diagnostics;

//namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

//internal class DrawerSlideHoleCalculator
//{
//    internal static List<(double x, double y, double radius)> Compute(BaseCabinetModel b, JoineryConfig joinery, double materialThickness34)
//    {
//        var holes = new List<(double, double, double)>();
//        var baseCab = b;
//        var dim = BaseCabinetDimensions.From(baseCab);

//        double mt34 = materialThickness34;

//        double radius = 0.19685 / 2.0; // 5mm diameter
//        string slideType = baseCab.DrwStyle;
//        double slidePositionX = 0;
//        double patternHeightAboveStretcher = 0;

//        if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
//        {
//            double openingHeight = dim.Opening1Height;

//            if (slideType.Contains("Blum"))
//            {
//                slidePositionX = mt34 + openingHeight;
//                patternHeightAboveStretcher = 1.4961;

//                slidePositionX -= patternHeightAboveStretcher;

//                if (baseCab.DrillSlideHolesOpening1)
//                {
//                    ApplyBlumHolePattern(holes, dim, radius, slidePositionX);
//                }
//            }
//        }

//        if (baseCab.Style == CabinetStyles.Base.Drawer)
//        {
//            double[] openingHeight = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
//            bool[] drillSlideHolesForOpening = [baseCab.DrillSlideHolesOpening1, baseCab.DrillSlideHolesOpening2, baseCab.DrillSlideHolesOpening3, baseCab.DrillSlideHolesOpening4];

//            for (int openingIndex = 0; openingIndex < baseCab.DrwCount; openingIndex++)
//            {
//                if (slideType.Contains("Blum"))
//                {
//                    patternHeightAboveStretcher = 1.4961;

//                    slidePositionX += mt34 + openingHeight[openingIndex];

//                    if (drillSlideHolesForOpening[openingIndex])
//                    {
//                        ApplyBlumHolePattern(holes, dim, radius, slidePositionX - patternHeightAboveStretcher);
//                    }
//                }
//            }
//        }

//        return holes;
//    }


//    private static void ApplyBlumHolePattern(List<(double, double, double)> holes, BaseCabinetDimensions dim, double radius, double slidePositionX)
//    {
//        double hole1Y = 0.3937;
//        double hole2Y = 1.4567;
//        double hole3Y = 2.7165;
//        double hole4Y = 0;
//        double hole5Y = 0;

//        holes.Add((slidePositionX, hole1Y, radius));
//        holes.Add((slidePositionX, hole2Y, radius));

//        if (dim.Depth >= 24)
//        {
//            hole4Y = hole2Y + (224 / 25.4); // Convert mm to inches
//            hole5Y = hole4Y + (256 / 25.4);
//        }
//        else if (dim.Depth >= 21 && dim.Depth < 24)
//        {
//            hole4Y = hole2Y + (224 / 25.4);
//            hole5Y = hole4Y + (192 / 25.4);
//        }
//        else if (dim.Depth >= 18 && dim.Depth < 21)
//        {
//            hole4Y = hole2Y + (128 / 25.4);
//            hole5Y = hole4Y + (192 / 25.4);
//        }
//        else if (dim.Depth >= 15 && dim.Depth < 18)
//        {
//            hole4Y = hole2Y + (128 / 25.4);
//            hole5Y = hole4Y + (96 / 25.4);
//        }
//        else if (dim.Depth >= 12 && dim.Depth < 15)
//        {
//            hole3Y = hole2Y + (96 / 25.4);
//            hole4Y = hole3Y + (96 / 25.4);
//            holes.Add((slidePositionX, hole2Y, radius));
//            holes.Add((slidePositionX, hole3Y, radius));
//            holes.Add((slidePositionX, hole4Y, radius));
//        }

//        if (dim.Depth >= 15)
//        {
//            holes.Add((slidePositionX, hole3Y, radius));
//            holes.Add((slidePositionX, hole4Y, radius));
//            holes.Add((slidePositionX, hole5Y, radius));
//        }
//    }

//    // Cab depth - back thickness - 5/8" (for 3/4" back). 
//}







using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class DrawerSlideHoleCalculator
{
    // 5mm diameter hole
    private const double HoleRadius = 0.19685 / 2.0;

    // Blum slide pattern offset from bottom of opening
    private const double PatternHeightAboveStretcher = 1.4961;

    // Fixed Y-offsets (inches)
    private const double BlumHole1Y = 0.3937;
    private const double BlumHole2Y = 1.4567;
    private const double BlumHole3Y_Default = 2.7165;

    private const double MmToInch = 1.0 / 25.4;

    internal static List<(double x, double y, double radius)> Compute(
        BaseCabinetModel baseCab,
        JoineryConfig joinery,
        double materialThickness34)
    {
        var holes = new List<(double, double, double)>();

        if (!baseCab.DrwStyle.Contains("Blum", StringComparison.OrdinalIgnoreCase))
            return holes;

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
            double slidePositionX = runningX - PatternHeightAboveStretcher;

            ApplyBlumHolePattern(holes, dim, slidePositionX);
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
}