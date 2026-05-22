using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using System.Diagnostics;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal class DrawerSlideHoleCalculator
{
    internal static List<(double x, double y, double radius)> Compute(BaseCabinetModel b, JoineryConfig joinery, double materialThickness34)
    {
        var holes = new List<(double, double, double)>();
        var baseCab = b;
        var dim = BaseCabinetDimensions.From(baseCab);

        double mt34 = materialThickness34;

        double radius = 0.19685 / 2.0; // 5mm diameter
        string slideType = baseCab.DrwStyle;
        double slidePositionX = 0;
        double patternHeightAboveStretcher = 0;

        if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
        {
            double openingHeight = dim.Opening1Height;

            if (slideType.Contains("Blum"))
            {
                slidePositionX = mt34 + openingHeight;
                patternHeightAboveStretcher = 1.4961;

                slidePositionX -= patternHeightAboveStretcher;

                if (baseCab.DrillSlideHolesOpening1)
                {
                    ApplyBlumHolePattern(holes, dim, radius, slidePositionX);
                }
            }
        }

        if (baseCab.Style == CabinetStyles.Base.Drawer)
        {
            double[] openingHeight = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
            bool[] drillSlideHolesForOpening = [baseCab.DrillSlideHolesOpening1, baseCab.DrillSlideHolesOpening2, baseCab.DrillSlideHolesOpening3, baseCab.DrillSlideHolesOpening4];

            for (int openingIndex = 0; openingIndex < baseCab.DrwCount; openingIndex++)
            {
                if (slideType.Contains("Blum"))
                {
                    patternHeightAboveStretcher = 1.4961;

                    slidePositionX += mt34 + openingHeight[openingIndex];

                    if (drillSlideHolesForOpening[openingIndex])
                    {
                        ApplyBlumHolePattern(holes, dim, radius, slidePositionX - patternHeightAboveStretcher);
                    }
                }
            }
        }

        return holes;
    }





    private static void ApplyBlumHolePattern(List<(double, double, double)> holes, BaseCabinetDimensions dim, double radius, double slidePositionX)
    {
        double hole1Y = 0.3937;
        double hole2Y = 1.4567;
        double hole3Y = 2.7165;
        double hole4Y = 0;
        double hole5Y = 0;

        holes.Add((slidePositionX, hole1Y, radius));
        holes.Add((slidePositionX, hole2Y, radius));

        if (dim.Depth >= 24)
        {
            hole4Y = hole2Y + (224 / 25.4); // Convert mm to inches
            hole5Y = hole4Y + (256 / 25.4);
        }
        else if (dim.Depth >= 21 && dim.Depth < 24)
        {
            hole4Y = hole2Y + (224 / 25.4);
            hole5Y = hole4Y + (192 / 25.4);
        }
        else if (dim.Depth >= 18 && dim.Depth < 21)
        {
            hole4Y = hole2Y + (128 / 25.4);
            hole5Y = hole4Y + (192 / 25.4);
        }
        else if (dim.Depth >= 15 && dim.Depth < 18)
        {
            hole4Y = hole2Y + (128 / 25.4);
            hole5Y = hole4Y + (96 / 25.4);
        }
        else if (dim.Depth >= 12 && dim.Depth < 15)
        {
            hole3Y = hole2Y + (96 / 25.4);
            hole4Y = hole3Y + (96 / 25.4);
            holes.Add((slidePositionX, hole2Y, radius));
            holes.Add((slidePositionX, hole3Y, radius));
            holes.Add((slidePositionX, hole4Y, radius));
        }

        if (dim.Depth >= 15)
        {
            holes.Add((slidePositionX, hole3Y, radius));
            holes.Add((slidePositionX, hole4Y, radius));
            holes.Add((slidePositionX, hole5Y, radius));
        }
    }

    // Cab depth - back thickness - 5/8" (for 3/4" back). 
}
