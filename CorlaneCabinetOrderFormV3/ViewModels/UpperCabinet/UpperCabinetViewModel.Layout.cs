using CorlaneCabinetOrderFormV3.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel
{
    private void RecalculateFrontWidth()
    {

        // Only relevant for "Angle Front" (Style4). Clear it otherwise.
        if (!string.Equals(Style, Style3, StringComparison.Ordinal))
        {
            FrontWidth = string.Empty;
            return;
        }

        // For the angle-front cabinet, the polygon edge used in Cabinet3DViewModel is:
        // p0 = (LeftDepth, 0)
        // p1 = (RightBackWidth - 3/4, LeftBackWidth - RightDepth)
        // frontWidth = distance(p0, p1)
        try
        {
            double leftDepth = ConvertDimension.FractionToDouble(LeftDepth);
            double rightDepth = ConvertDimension.FractionToDouble(RightDepth);
            double leftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth);
            double rightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth);

            const double materialThickness34 = 0.75;

            double p0x = leftDepth;
            double p0y = materialThickness34;

            double p1x = rightBackWidth - materialThickness34;
            double p1y = leftBackWidth - rightDepth;

            double vx = p1x - p0x;
            double vy = p1y - p0y;

            double frontWidth = Math.Sqrt((vx * vx) + (vy * vy));

            // Format per default settings
            string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";
            FrontWidth = string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                ? ConvertDimension.DoubleToFraction(frontWidth)
                : frontWidth.ToString("0.####");
        }
        catch
        {
            // If any inputs are invalid/empty, just clear output.
            FrontWidth = string.Empty;
        }
    }

    private void RecalculateBackWidths90()
    {
        double leftBack = ConvertDimension.FractionToDouble(LeftFrontWidth) + ConvertDimension.FractionToDouble(RightDepth);
        double rightBack = ConvertDimension.FractionToDouble(RightFrontWidth) + ConvertDimension.FractionToDouble(LeftDepth);

        bool useFraction = string.Equals(_defaults?.DefaultDimensionFormat, "Fraction", StringComparison.OrdinalIgnoreCase);

        LeftBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(leftBack)
            : leftBack.ToString();

        RightBackWidth90 = useFraction
            ? ConvertDimension.DoubleToFraction(rightBack)
            : rightBack.ToString();
    }

}
