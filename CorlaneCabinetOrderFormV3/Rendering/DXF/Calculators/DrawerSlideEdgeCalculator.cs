using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Computes drawer slide mounting holes for base cabinet end panels.
/// Delegates to DrawerSlideHoleCalculator for the actual computation.
/// </summary>
internal static class DrawerSlideEdgeCalculator
{
    internal static void ComputeDrawerSlideHoles(BaseCabinetModel baseCab, List<(double, double, double)> holes, JoineryConfig joinery, double mt34)
    {
        holes.AddRange(DrawerSlideHoleCalculator.Compute(baseCab, joinery, materialThickness34: mt34));
    }
}
