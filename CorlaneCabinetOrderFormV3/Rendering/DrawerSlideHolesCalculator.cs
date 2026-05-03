using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;

internal static class DrawerSlideHolesCalculator
{
    public record DrawerSlideHole(int OpeningIndex, double Y, double X);

    public static List<DrawerSlideHole> Compute(
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        double yOffsetFromBottom = 1.5)
    {
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

        // Calculate the 3 fixed X positions based on cabinet depth
        // Coordinate system (per BaseCabinetBuilder): X=0 is back edge, X=Depth is front edge
        double xFront = dim.Depth - 1.456;
        double xBack = 3.0;
        double xMiddle = (xFront + xBack) / 2.0;

        double[] holeXPositions = [xFront, xMiddle, xBack];

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

            // Add all tailored depth positions for this opening
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
