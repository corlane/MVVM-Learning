using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class ShelfHoleCalculator
{
    public record ShelfHole(double X, double Y);

    public static List<ShelfHole> ComputeShelfHoles(BaseCabinetModel baseCab, BaseCabinetDimensions dim)
    {
        List<ShelfHole> holes = new List<ShelfHole>();
        double mt34 = MaterialDefaults.Thickness34;
        double height = dim.Height;
        double tkH = dim.TKHeight;
        double opening1H = dim.Opening1Height;
        double shelfDepth = dim.ShelfDepth;
        double backThick = dim.BackThickness;

        int count = (int)Math.Round(((height - 12) / 1.26) - tkH);
        double yStart = tkH + 6;
        double? maxY = baseCab.DrwCount == 1
            ? (height - opening1H - (2 * mt34)) - 6
            : null;

        double backX = 1 + backThick;
        double frontX = shelfDepth + backThick - 1;

        for (int i = 0; i < count; i++)
        {
            double y = yStart + (i * 1.26);
            if (maxY.HasValue && y > maxY.Value) break;
            holes.Add(new ShelfHole(backX, y));
            holes.Add(new ShelfHole(frontX, y));
        }
        return holes;
    }
}