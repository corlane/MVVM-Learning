using CorlaneCabinetOrderFormV3.Converters;
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
        double tkDepth = 0,
        CabinetModel? cabinet = null)
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
                TenonEdges: ResolveTenonEdges(entry.PartName, cabinet),
                MortiseEdges: ResolveMortiseEdges(entry.PartName, cabinet),
                ScrewHoleEdges: (ScrewHoleEdge)ResolveMortiseEdges(entry.PartName, cabinet), // This automatically adds screw holes where there are mortises. Can be changed to be independent.
                ThinningPockets: ResolveTenonThinningEdges(entry.PartName, cabinet),
                EdgeBand: entry.EdgeBandSpecies,
                Notes: entry.Notes,
                TkHeight: tkHeight,
                TkDepth: tkDepth,
                Cabinet: isEndPanel ? cabinet : null
                );
            mapped.Add( mappedPart );
        }
        return mapped;
    }

    private static TenonEdge ResolveTenonEdges(string partName, CabinetModel? cabinet)
    {
        return partName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" => TenonEdge.Top | TenonEdge.Left | TenonEdge.Right,
            "Top Stretcher (Front)" or "Drawer Stretcher" => TenonEdge.Left | TenonEdge.Right,
            "Back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => TenonEdge.Top | TenonEdge.Bottom | TenonEdge.Left,
            "Back" when cabinet is UpperCabinetModel uppercab && ConvertDimension.FractionToDouble(uppercab.BackThickness) != 0.25 => TenonEdge.Top | TenonEdge.Bottom,
            "Deck" => TenonEdge.Left | TenonEdge.Right | TenonEdge.Bottom,
            "Nailer" => TenonEdge.Left | TenonEdge.Right | TenonEdge.Top,
            _ => TenonEdge.None
        };
    }

    private static ThinningPocketEdge ResolveTenonThinningEdges(string partName, CabinetModel? cabinet)
    {
        return partName switch
        {
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" => ThinningPocketEdge.Top | ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            "Top Stretcher (Front)" => ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            "Back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => ThinningPocketEdge.Top | ThinningPocketEdge.Bottom | ThinningPocketEdge.Left,
            "Back" when cabinet is UpperCabinetModel uppercab && ConvertDimension.FractionToDouble(uppercab.BackThickness) != 0.25 => ThinningPocketEdge.Top | ThinningPocketEdge.Bottom,
            "Deck" => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Bottom,
            "Nailer" => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Top,
            _ => ThinningPocketEdge.None
        };
    }


    private static MortiseEdge ResolveMortiseEdges(string partName, CabinetModel? cabinet)
    {
        return partName.ToLowerInvariant() switch
        {
            "left end" or "right end" => MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top,
            "back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => MortiseEdge.Right,
            "back" when cabinet is UpperCabinetModel uppercab && ConvertDimension.FractionToDouble(uppercab.BackThickness) != 0.25 => MortiseEdge.Right | MortiseEdge.Left,
            "top stretcher (back)" => MortiseEdge.Bottom,
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
