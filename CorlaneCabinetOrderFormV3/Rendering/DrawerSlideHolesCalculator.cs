using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.Generic;
using System.Windows.Documents;

internal static class DrawerSlideHolesCalculator
{
    public record DrawerSlideHole(int OpeningIndex, double Y, double X);

    public static List<DrawerSlideHole> Compute(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double x1, double x2, double x3, double x4, double x5, double x6, double x7, double x8, double x9,
        double yOffsetFromBottom = 1.5)
    {
        // FIX 2: Specify the generic type List<DrawerSlideHole>
        var holes = new List<DrawerSlideHole>();

        double mt34 = MaterialDefaults.Thickness34;
        double height = dim.Height;

        double[] openingHeights = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
        bool[] drillPerOpening = [
            baseCab.DrillSlideHolesOpening1,
            baseCab.DrillSlideHolesOpening2,
            baseCab.DrillSlideHolesOpening3,
            baseCab.DrillSlideHolesOpening4
        ];

        double[] holeXPositions = [x1, x2, x3, x4, x5, x6, x7, x8, x9];

        // Start Y at the bottom of the first opening
        double openingBottomY = height - mt34 - openingHeights[0];

        for (int oi = 0; oi < 4; oi++)
        {
            int openingIndex = oi + 1;
            if (baseCab.DrwCount < openingIndex) break;
            if (!drillPerOpening[oi])
            {
                // Advance to next opening bottom even if not drilling
                if (oi + 1 < 4) openingBottomY -= openingHeights[oi + 1] + mt34;
                continue;
            }

            double y = openingBottomY + yOffsetFromBottom;

            // Add all 6 tailored depth positions for this opening
            foreach (double x in holeXPositions)
            {
                holes.Add(new DrawerSlideHole(openingIndex, y, x));
            }

            // Advance Y for the next opening
            if (oi + 1 < 4)
            {
                openingBottomY -= openingHeights[oi + 1] + mt34;
            }
        }

        return holes;
    }
}
