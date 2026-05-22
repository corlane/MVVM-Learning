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
        }


        return holes;
    }

    // Cab depth - back thickness - 5/8" (for 3/4" back). 
}
