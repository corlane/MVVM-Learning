using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Resolves panel depths and computes hinge mounting holes for base and upper cabinet end panels.
/// Delegates to HingeHoleCalculator for the actual hole computation.
/// </summary>
internal static class HingeHoleEdgeCalculator
{
    internal static double ResolveBasePanelDepth(BaseCabinetModel baseCab, BaseCabinetDimensions dim, string partName)
    {
        if (baseCab.Style == CabinetStyles.Base.Corner90 || baseCab.Style == CabinetStyles.Base.AngleFront)
        {
            bool isLeft = partName.Contains("Left End", StringComparison.OrdinalIgnoreCase);
            return isLeft ? dim.LeftDepth : dim.RightDepth;
        }
        return dim.Depth;
    }

    internal static double ResolveUpperPanelDepth(UpperCabinetModel upperCab, UpperCabinetDimensions dim, string partName)
    {
        if (upperCab.Style == CabinetStyles.Upper.Corner90 || upperCab.Style == CabinetStyles.Upper.AngleFront)
        {
            bool isLeft = partName.Contains("Left End", StringComparison.OrdinalIgnoreCase);
            return isLeft ? dim.LeftDepth : dim.RightDepth;
        }
        return dim.Depth;
    }

    internal static void ComputeHingeHoles(BaseCabinetModel baseCab, BaseCabinetDimensions dim, double panelDepth, List<(double, double, double)> holes, double mt34)
    {
        holes.AddRange(HingeHoleCalculator.Compute(baseCab, dim, panelDepth, mt34));
    }

    internal static void ComputeUpperHingeHoles(UpperCabinetModel upperCab, UpperCabinetDimensions dim, double panelDepth, List<(double, double, double)> holes)
    {
        holes.AddRange(HingeHoleCalculator.Compute(upperCab, dim, panelDepth));
    }
}
