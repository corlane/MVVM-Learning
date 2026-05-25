using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Computes shelf pin holes for end panels of base and upper cabinets.
/// Delegates to DXFShelfHoleCalculator with cabinet-style-specific filtering.
/// </summary>
internal static class ShelfHoleEdgeCalculator
{
    internal static void ComputeShelfHoles(PartInfo part, List<(double, double, double)> holes, JoineryConfig joinery, double mt34)
    {
        if (!part.Name.Contains("End", StringComparison.OrdinalIgnoreCase)) return;

        if (part.CabinetModel is BaseCabinetModel bCab && bCab.Style == CabinetStyles.Base.Standard && bCab.DrillShelfHoles)
        {
            holes.AddRange(DXFShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        if (part.CabinetModel is BaseCabinetModel bCab90Left && bCab90Left.Style == CabinetStyles.Base.Corner90 && bCab90Left.DrillShelfHoles && part.Name.Contains("Left End"))
        {
            holes.AddRange(DXFShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        if (part.CabinetModel is BaseCabinetModel bCab90Right && bCab90Right.Style == CabinetStyles.Base.Corner90 && bCab90Right.DrillShelfHoles && part.Name.Contains("Right End"))
        {
            holes.AddRange(DXFShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }

        else if (part.CabinetModel is UpperCabinetModel uCab && uCab.DrillShelfHoles)
        {
            holes.AddRange(DXFShelfHoleCalculator.ComputeShelfHoles(part, joinery, materialThickness34: mt34));
        }
    }
}
