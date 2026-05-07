using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
using CorlaneCabinetOrderFormV3.Services;

internal static class CabinetInputAdapter
{
    internal static List<PartInfo> MapParts(
        IEnumerable<PartListEntry> parts,
        LockDadoSettings? settings = null,
        double tkHeight = 0,
        double tkDepth = 0)
    {
        var mapped = new List<PartInfo>();

        foreach (var entry in parts)
        {
            // Enrich end panels with cabinet toekick dimensions
            bool isEndPanel = entry.PartName.Contains("End", StringComparison.OrdinalIgnoreCase);
            double partTkH = isEndPanel ? tkHeight : 0;
            double partTkD = isEndPanel ? tkDepth : 0;

            mapped.Add(new PartInfo(
                Name: entry.PartName,
                Bounds: new PartBounds(entry.LengthIn, entry.WidthIn),
                Material: entry.Species,
                Quantity: entry.Qty,
                TenonEdges: ResolveEdgeFlags(entry.PartName),
                EdgeBand: entry.EdgeBandSpecies,
                Notes: entry.Notes,
                TkHeight: partTkH,   // ← Actually assigns the value
                TkDepth: partTkD,
                MortiseEdges: ResolveMortiseEdges(entry.PartName)
            ));
        }

        return mapped;
    }

    private static TenonEdge ResolveEdgeFlags(string partName)
    {
        return partName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" => TenonEdge.Top | TenonEdge.Left | TenonEdge.Right,
            "Top Stretcher (Front)" or "Drawer Stretcher" => TenonEdge.Left | TenonEdge.Right,
            "Back" => TenonEdge.Top | TenonEdge.Bottom | TenonEdge.Left,
            "Deck" => TenonEdge.Left | TenonEdge.Right | TenonEdge.Bottom,
            _ => TenonEdge.None
        };
    }


    private static MortiseEdge ResolveMortiseEdges(string partName)
    {
        return partName.ToLowerInvariant() switch
        {
            // End panels receive mortises on left/right edges
            "left end" or "right end" => MortiseEdge.Left | MortiseEdge.Right,

            // Back panel receives mortises on top/bottom (mating with deck/top)
            "back" => MortiseEdge.Right,

            // Other parts as needed
            _ => MortiseEdge.None
        };
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
}
