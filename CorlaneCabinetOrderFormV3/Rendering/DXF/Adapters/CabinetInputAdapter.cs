using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;
using System.Diagnostics;
using System.Windows;

internal static class CabinetInputAdapter
{
    internal static List<PartInfo> MapParts(
        IEnumerable<PartListEntry> parts,
        LockDadoSettings? settings = null,
        double tkHeight = 0,
        double tkDepth = 0,
        CabinetModel? cabinet = null,
        double materialThickness34 = 0
        )
    {
        var mapped = new List<PartInfo>();
        foreach (var entry in parts)
        {
            PartInfo mappedPart;

            // Cabinet-type-agnostic style detection
            bool isCorner90 = cabinet.IsCorner90();
            bool isAngleFront = cabinet.IsAngleFront();

            // ── Resolve Bounds (handle "—" dimensions for Corner90 L-shapes) ──
            double partLength = entry.LengthIn;
            double partWidth = entry.WidthIn;

            if (isCorner90 && (partLength == 0 || partWidth == 0))
            {
                bool isLShape = entry.PartName.Contains("Top") || entry.PartName.Contains("Deck") || entry.PartName.Contains("Shelf");
                if (isLShape)
                {
                    // Assign bounding box dimensions so ValidateInput() passes
                    var (_, _, lf, rf, ld, rd) = cabinet!.GetCornerDimensions();
                    double mt = materialThickness34;

                    partLength = Math.Max(lf, ld) + Math.Max(rf, rd) - mt;
                    partWidth = Math.Max(lf, ld) - mt;
                }
            }

            bool isCornerOrAngleTopDeck = (isCorner90 || isAngleFront) && (entry.PartName.Contains("Top") || entry.PartName.Contains("Deck"));

            if (isCornerOrAngleTopDeck)
            {
                mappedPart = new PartInfo(
                    Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                    TenonEdges: TenonEdge.None, MortiseEdges: MortiseEdge.Left | MortiseEdge.Top, ScrewHoleEdges: ScrewHoleEdge.None, ThinningPockets: ThinningPocketEdge.None,
                    EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
            }
            else
            {
                // Standard mapping for all other parts (including non-Top/Deck Corner90/AngleFront parts)
                double partTkH = cabinet is BaseCabinetModel ? tkHeight : 0;
                double partTkD = cabinet is BaseCabinetModel ? tkDepth : 0;

                mappedPart = new PartInfo(
                    Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                    TenonEdges: ResolveTenonEdges(entry.PartName, cabinet),
                    MortiseEdges: ResolveMortiseEdges(entry.PartName, cabinet),
                    MortiseThruEdges: ResolveMortiseThruEdges(entry.PartName, cabinet),
                    ScrewHoleEdges: (ScrewHoleEdge)ResolveMortiseEdges(entry.PartName, cabinet),
                    ThinningPockets: ResolveTenonThinningEdges(entry.PartName, cabinet),
                    EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: partTkH, TkDepth: partTkD, CabinetModel: cabinet);
            }
            mapped.Add(mappedPart);
        }
        return mapped;
    }

    private static TenonEdge ResolveTenonEdges(string partName, CabinetModel? cabinet)
    {
        return partName switch
        {
            "Toekick" => TenonEdge.Top | TenonEdge.Left | TenonEdge.Right,
            "Toekick (Left)" => TenonEdge.Top | TenonEdge.Left,
            "Toekick (Right)" => TenonEdge.Top | TenonEdge.Right,
            "Top Stretcher (Front)" or "Drawer Stretcher" => TenonEdge.Left | TenonEdge.Right,
            "Back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => TenonEdge.Top | TenonEdge.Bottom | TenonEdge.Left,
            "Left Back" when cabinet is BaseCabinetModel => TenonEdge.Left,
            "Right Back" when cabinet is BaseCabinetModel => TenonEdge.Right,
            "Left Back" when cabinet is UpperCabinetModel => TenonEdge.Top,
            "Right Back" when cabinet is UpperCabinetModel => TenonEdge.Bottom,
            "Back" when cabinet is UpperCabinetModel uppercab && ConvertDimension.FractionToDouble(uppercab.BackThickness) != 0.25 => TenonEdge.Top | TenonEdge.Bottom,
            "Deck" when cabinet is BaseCabinetModel baseCab && ConvertDimension.FractionToDouble(baseCab.BackThickness) != 0.25 => TenonEdge.Left | TenonEdge.Right | TenonEdge.Bottom,
            "Deck" when cabinet is BaseCabinetModel baseCab && ConvertDimension.FractionToDouble(baseCab.BackThickness) == 0.25 => TenonEdge.Left | TenonEdge.Right,
            "Deck" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) != 0.25 => TenonEdge.Left | TenonEdge.Right | TenonEdge.Bottom,
            "Deck" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25 => TenonEdge.Left | TenonEdge.Right,
            "Top" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) != 0.25 => TenonEdge.Left | TenonEdge.Right | TenonEdge.Bottom,
            "Top" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25 => TenonEdge.Left | TenonEdge.Right,
            "Nailer" when cabinet is BaseCabinetModel => TenonEdge.Left | TenonEdge.Right | TenonEdge.Top,
            "Nailer" when cabinet is UpperCabinetModel => TenonEdge.Left | TenonEdge.Right,

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
            "Left Back" when cabinet is BaseCabinetModel basecab => ThinningPocketEdge.Left,
            "Right Back" when cabinet is BaseCabinetModel basecab => ThinningPocketEdge.Right,
            "Left Back" when cabinet is UpperCabinetModel uppercab => ThinningPocketEdge.Top,
            "Right Back" when cabinet is UpperCabinetModel uppercab => ThinningPocketEdge.Bottom,
            "Deck" => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Bottom,
            "Nailer" when cabinet is BaseCabinetModel => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Top,
            "Nailer" when cabinet is UpperCabinetModel => ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            "Top" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) != 0.25 => ThinningPocketEdge.Left | ThinningPocketEdge.Right | ThinningPocketEdge.Bottom,
            "Top" when cabinet is UpperCabinetModel upperCab && ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25 => ThinningPocketEdge.Left | ThinningPocketEdge.Right,
            _ => ThinningPocketEdge.None
        };
    }


    private static MortiseEdge ResolveMortiseEdges(string partName, CabinetModel? cabinet)
    {
        bool isBaseCab = cabinet is BaseCabinetModel;
        bool isUpperCab = cabinet is UpperCabinetModel;

        return partName.ToLowerInvariant() switch
        {
            "left end" or "right end" when isBaseCab && cabinet is BaseCabinetModel basecab && basecab.HasTK => MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top | MortiseEdge.Bottom,
            "left end" or "right end" when isBaseCab && cabinet is BaseCabinetModel basecab && !basecab.HasTK => MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top,
            "left end" or "right end" when isUpperCab => MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top,
            "top stretcher (back)" => MortiseEdge.Bottom,
            "deck" when isBaseCab && cabinet is BaseCabinetModel basecab && basecab.HasTK => MortiseEdge.Top,
            _ => MortiseEdge.None
        };
    }

    private static MortiseThruEdge ResolveMortiseThruEdges(string partName, CabinetModel? cabinet)
    {
        bool isBaseCab = cabinet is BaseCabinetModel;
        bool isUpperCab = cabinet is UpperCabinetModel;

        BaseCabinetDimensions baseDims = default;
        if (isBaseCab) baseDims = BaseCabinetDimensions.From((BaseCabinetModel)cabinet!);
        
        UpperCabinetDimensions upperDims = default;
        if (isUpperCab) upperDims = UpperCabinetDimensions.From((UpperCabinetModel)cabinet!);
        
        return partName.ToLowerInvariant() switch
        {
            "back" when isBaseCab && baseDims.BackThickness != 0.25 => MortiseThruEdge.Right,
            "left back" when isBaseCab => MortiseThruEdge.Bottom,
            "right back" when isBaseCab => MortiseThruEdge.Bottom,
            "left back" when isUpperCab => MortiseThruEdge.Left | MortiseThruEdge.Right,
            "right back" when isUpperCab => MortiseThruEdge.Left | MortiseThruEdge.Right,
            "back" when isUpperCab && upperDims.BackThickness != 0.25 => MortiseThruEdge.Left | MortiseThruEdge.Right,
            _ => MortiseThruEdge.None
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
            ScrewPilotHoleDiameter: s.ScrewPilotHoleDiameter
        );
    }
}
