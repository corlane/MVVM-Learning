using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal class DrawerSlideHoleCalculator
{
    internal static List<(double x, double y, double radius)> Compute(PartInfo part, JoineryConfig joinery, double materialThickness34)
    {
        var holes = new List<(double, double, double)>();

        if (part.CabinetModel is BaseCabinetModel baseCab && baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
        {
            var dim = BaseCabinetDimensions.From(baseCab);

            double mt34 = materialThickness34;

            double backThickness = dim.BackThickness;
            if (backThickness == 0.75) backThickness = mt34;
            if (dim.BackThickness == 0.25) backThickness = 0;

            double radius = 0.19685 / 2.0; // 5mm diameter
            double openingHeight = dim.Opening1Height;
            string slideType = baseCab.DrwStyle;
            double slidePositionX = mt34 + openingHeight;

            if (slideType.Contains("Blum"))
            {
                slidePositionX -= 1.4961; // Height of drawer slide pattern above Drw Stretcher

                double hole1Y = 1.4567;
                double hole2Y = 3.9764;
                double hole3Y = 0;
                double hole4Y = 0;
                double hole5Y = 0;

                holes.Add((slidePositionX, hole1Y, radius));
                holes.Add((slidePositionX, hole2Y, radius));

                if (dim.Depth > 21)
                {
                    hole3Y = 5.2362;
                    hole4Y = 6.4961;
                    hole5Y = 9.0157;
                }
                else if (dim.Depth > 18 && dim.Depth <= 21)
                {
                    hole3Y = 5.2362;
                    hole4Y = 6.4961;
                    hole5Y = 9.0157;
                }
                else if (dim.Depth > 15 && dim.Depth <= 18)
                {
                    hole3Y = 5.2362;
                    hole4Y = 6.4961;
                    hole5Y = 9.0157;
                }
                else if (dim.Depth > 11.99 && dim.Depth <= 15)
                {
                    hole3Y = 5.2362;
                    hole4Y = 6.4961;
                    hole5Y = 9.0157;
                }

                holes.Add((slidePositionX, hole3Y, radius));
                holes.Add((slidePositionX, hole4Y, radius));
                holes.Add((slidePositionX, hole5Y, radius));

            }
        }


        return holes;
    }

    // Cab depth - back thickness - 5/8" (for 3/4" back). 
}