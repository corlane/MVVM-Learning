using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Calculators;

internal static class ShelfHoleCalculator
{
    internal static List<(double x, double y, double radius)> ComputeShelfHoles(PartInfo part, JoineryConfig joinery)
    {
        var holes = new List<(double, double, double)>();

        if (part.Cabinet is BaseCabinetModel baseCab)
        {
            //if (baseCab == null) return holes;

            var dim = BaseCabinetDimensions.From(baseCab);
            double mt34 = MaterialDefaults.Thickness34;
            double height = part.Bounds.Height;
            double tkH = part.TkHeight;

            int count = (int)Math.Round(((height - 12) / 1.26) - tkH);
            double yStart = tkH + 6;
            double? maxY = baseCab.DrwCount == 1
                ? (height - dim.Opening1Height - (2 * mt34)) - 6
                : null;

            double backX = 1 + dim.BackThickness;
            double frontX = dim.ShelfDepth + dim.BackThickness - 1;
            double radius = 0.19685 / 2.0; // 5mm diameter

            for (int i = 0; i < count; i++)
            {
                double y = yStart + (i * 1.26);
                if (maxY.HasValue && y > maxY.Value) break;

                holes.Add((backX, y, radius));
                holes.Add((frontX, y, radius));
            }
        }


        return holes;
    }
}
