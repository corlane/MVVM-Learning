using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;
using CorlaneCabinetOrderFormV3.Services;
using System.Diagnostics;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

internal static class DXFShelfHoleCalculator
{
    internal static List<(double x, double y, double radius)> ComputeShelfHoles(PartInfo part, JoineryConfig joinery, double materialThickness34)
    {
        var holes = new List<(double, double, double)>();

        if (part.CabinetModel is BaseCabinetModel baseCab && baseCab.Style == CabinetStyles.Base.Standard)
        {
            var dim = BaseCabinetDimensions.From(baseCab);

            double mt34 = materialThickness34;
            
            double backThickness = dim.BackThickness;
            if (backThickness == 0.75) backThickness = mt34;
            if (dim.BackThickness == 0.25) backThickness = 0;

            double width = part.Bounds.Width;
            double height = part.Bounds.Height;
            double tkH = part.TkHeight;

            int count = (int)Math.Round((width - 12 - tkH) / 1.26);
            double xStart = width - tkH - mt34 - 6;
            double? xStop = baseCab.DrwCount == 1
                ? dim.Opening1Height + (2 * mt34) + 6
                : null;

            double backY = height - 1 - backThickness;
            double frontY = height - dim.ShelfDepth + 1 - backThickness;
            double radius = 0.19685 / 2.0; // 5mm diameter

            for (int i = 0; i < count; i++)
            {
                double x = xStart - (i * 1.26);
                if (xStop.HasValue && x < xStop.Value) break;

                holes.Add((x, backY, radius));
                holes.Add((x, frontY, radius));
            }
        }

        else if (part.CabinetModel is BaseCabinetModel baseCorner && (baseCorner.Style == CabinetStyles.Base.Corner90 || baseCorner.Style == CabinetStyles.Base.AngleFront))
        {
            var dim = BaseCabinetDimensions.From(baseCorner);

            double mt34 = materialThickness34;

            double width = part.Bounds.Width;
            double height = part.Bounds.Height;
            double tkH = part.TkHeight;

            int count = (int)Math.Round((width - 12 - tkH) / 1.26);
            double xStart = width - tkH - mt34 - 6;
            double? xStop = 0 + mt34 + 6;

            double frontY = 0;
            double backY = height - 1 - mt34;

            if (part.Name.Contains("Left End"))
            {
                frontY = height - dim.LeftDepth + 1;
            }
            if (part.Name.Contains("Right End"))
            {
                frontY = height - dim.RightDepth + 1;
            }

            double radius = 0.19685 / 2.0; // 5mm diameter

            for (int i = 0; i < count; i++)
            {
                double x = xStart - (i * 1.26);
                if (xStop.HasValue && x < xStop.Value) break;

                holes.Add((x, backY, radius));
                holes.Add((x, frontY, radius));
            }
        }

        else if (part.CabinetModel is UpperCabinetModel upperCab)
        {
            var dim = UpperCabinetDimensions.From(upperCab);
            double mt34 = materialThickness34;

            double backThickness = dim.BackThickness;
            if (dim.BackThickness == 0.25) backThickness = 0;

            double width = part.Bounds.Width;
            double height = part.Bounds.Height;
            double tkH = part.TkHeight;

            int count = (int)Math.Round((width - 12) / 1.26);
            double xStart = width - mt34 - 6;

            double backY = height - 1 - backThickness;
            double frontY = height - dim.ShelfDepth + 1 - backThickness;
            double radius = 0.19685 / 2.0; // 5mm diameter

            for (int i = 0; i < count; i++)
            {
                double x = xStart - (i * 1.26);

                holes.Add((x, backY, radius));
                holes.Add((x, frontY, radius));
            }
        }


        return holes;
    }
}
