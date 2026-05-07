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

            var mappedPart = new PartInfo(
                Name: entry.PartName,
                Bounds: new PartBounds(entry.LengthIn, entry.WidthIn),
                Material: entry.Species,
                Quantity: entry.Qty,
                TenonEdges: ResolveTenonEdges(entry.PartName),
                MortiseEdges: ResolveMortiseEdges(entry.PartName),
                ScrewHoleEdges: (ScrewHoleEdge)ResolveMortiseEdges(entry.PartName),
                ThinningPockets: (ThinningPocketEdge) ResolveTenonThinningEdges(entry.PartName),
                EdgeBand: entry.EdgeBandSpecies,
                Notes: entry.Notes,
                TkHeight: tkHeight,
                TkDepth: tkDepth
                );
            mapped.Add( mappedPart );
        }
        return mapped;
    }

    private static TenonEdge ResolveTenonEdges(string partName)
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

    private static ThinningPocketEdge ResolveTenonThinningEdges(string partName)
    {
        return partName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" => ThinningPocketEdge.Top | ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            "Top Stretcher (Front)" => ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            "Back" => ThinningPocketEdge.Top | ThinningPocketEdge.Bottom | ThinningPocketEdge.Left,
            "Deck" => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Bottom,
            _ => ThinningPocketEdge.None
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
