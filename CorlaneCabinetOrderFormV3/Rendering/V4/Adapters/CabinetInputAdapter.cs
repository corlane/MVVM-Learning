using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Adapters;

/// <summary>
/// Bridges existing cabinet models into the new standardized pipeline.
/// </summary>
internal static class CabinetInputAdapter
{
    internal static List<PartInfo> MapParts(IEnumerable<PartListEntry> existingParts)
    {
        return existingParts.Select(p => new PartInfo(
            Name: p.PartName,
            Bounds: new PartBounds(
                Width: p.WidthIn,       // X-axis
                Height: p.LengthIn      // Y-axis
            ),
            Material: p.Species ?? "Standard",
            Quantity: p.Qty,
            TenonEdges: ResolveEdgeFlags(p.PartName),
            EdgeBand: string.IsNullOrEmpty(p.EdgeBandSpecies) ? null : $"{p.EdgeBandSpecies} ({p.EdgeBandLength})",
            Notes: p.Notes
        )).ToList();
    }

    internal static JoineryConfig MapSettings(LockDadoSettings? settings)
    {
        var s = settings ?? LockDadoSettings.Default;
        return new JoineryConfig(
            BlindStart: s.BlindStart,
            BlindStop: s.BlindStop,
            DadoDepth: s.DadoDepth,
            MortiseDepthClearance: s.MortiseDepthClearance,
            TenonThickness: s.TenonThickness,
            TenonClearance: s.TenonClearance,
            TenonPocketOversize: s.TenonPocketOversize,
            MortiseOversize: s.MortiseOversize,
            GapWidth: s.GapWidth,
            GapSpacing: s.GapSpacing,
            ScrewPilotHoleDiameter: s.ScrewPilotHoleDiameter,
            Thickness34: MaterialDefaults.Thickness34
        );
    }

    /// <summary>
    /// Replicates existing DxfPartKind logic to determine tenon placement 
    /// without relying on a missing property in PartListEntry.
    /// </summary>
    private static Edge ResolveEdgeFlags(string partName)
    {
        return partName.ToLowerInvariant() switch
        {
            // TenonLeftAndRight
            "deck" or "top" or "top stretcher (front)" or "nailer" or "sink stretcher"
                or "drawer stretcher" => Edge.Left | Edge.Right,

            // TenonTopAndBottom
            "back" => Edge.Top | Edge.Bottom,

            // TenonTopLeftRight
            "toekick" or "toekick (left)" or "toekick (right)" => Edge.Top | Edge.Left | Edge.Right,

            // MortisePanel / Plain (No tenons on the panel itself)
            "left end" or "right end" or _ => Edge.None
        };
    }
}
