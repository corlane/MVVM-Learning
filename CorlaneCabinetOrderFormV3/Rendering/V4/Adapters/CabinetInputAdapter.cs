using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Rendering.V4.Core;
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
        BaseCabinetDimensions? baseCabDim = null,
        double materialThickness34 = 0
        )
    {
        var mapped = new List<PartInfo>();

        foreach (var entry in parts)
        {
            PartInfo mappedPart;
            bool isCorner90 = cabinet is BaseCabinetModel bCab && bCab.Style == CabinetStyles.Base.Corner90;
            var baseCab = cabinet as BaseCabinetModel;

            // ── Resolve Bounds (handle "—" dimensions for Corner90 L-shapes) ──
            double partLength = entry.LengthIn;
            double partWidth = entry.WidthIn;

            if (isCorner90 && (partLength == 0 || partWidth == 0))
            {
                bool isLShape = entry.PartName.Contains("Top") || entry.PartName.Contains("Deck") || entry.PartName.Contains("Shelf");
                if (isLShape && baseCab != null)
                {
                    // Assign bounding box dimensions so ValidateInput() passes
                    double lf = ConvertDimension.FractionToDouble(baseCab.LeftFrontWidth);
                    double rf = ConvertDimension.FractionToDouble(baseCab.RightFrontWidth);
                    double ld = ConvertDimension.FractionToDouble(baseCab.LeftDepth);
                    double rd = ConvertDimension.FractionToDouble(baseCab.RightDepth);
                    double mt = materialThickness34;

                    partLength = Math.Max(lf, ld) + Math.Max(rf, rd) - mt;
                    partWidth = Math.Max(lf, ld) - mt;
                }
            }

            if (isCorner90)
            {
                bool isEnd = entry.PartName.Contains("End", StringComparison.OrdinalIgnoreCase);
                bool isLeftBack = entry.PartName.Contains("Left Back", StringComparison.OrdinalIgnoreCase);
                bool isRightBack = entry.PartName.Contains("Right Back", StringComparison.OrdinalIgnoreCase);
                bool isTopOrDeck = entry.PartName.Contains("Top") || entry.PartName.Contains("Deck");
                bool isShelf = entry.PartName.Contains("Shelf");
                bool isDoor = entry.PartName.Contains("Door");
                bool isToekick = entry.PartName.Contains("Toekick");

                bool isEndPanel = entry.PartName.Contains("End", StringComparison.OrdinalIgnoreCase);
                double partTkH = cabinet is BaseCabinetModel ? tkHeight : 0;
                double partTkD = cabinet is BaseCabinetModel ? tkDepth : 0;


                if (isEnd)
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.None,
                        MortiseEdges: MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top | MortiseEdge.Bottom,
                        ScrewHoleEdges: ScrewHoleEdge.Left | ScrewHoleEdge.Right | ScrewHoleEdge.Top | ScrewHoleEdge.Bottom,
                        ThinningPockets: ThinningPocketEdge.None,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
                else if (isTopOrDeck)
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.None, MortiseEdges: MortiseEdge.Left | MortiseEdge.Top, ScrewHoleEdges: ScrewHoleEdge.None, ThinningPockets: ThinningPocketEdge.None,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
                else if (isLeftBack)
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.Left, MortiseEdges: MortiseEdge.Bottom, ScrewHoleEdges: ScrewHoleEdge.Bottom, ThinningPockets: ThinningPocketEdge.Left,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
                else if (isRightBack)
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.Right, MortiseEdges: MortiseEdge.Bottom, ScrewHoleEdges: ScrewHoleEdge.Bottom, ThinningPockets: ThinningPocketEdge.Right,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
                else if (isToekick)
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.Right | TenonEdge.Left | TenonEdge.Top, MortiseEdges: MortiseEdge.None, ScrewHoleEdges: ScrewHoleEdge.None, ThinningPockets: ThinningPocketEdge.Right | ThinningPocketEdge.Left | ThinningPocketEdge.Top,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
                else
                {
                    mappedPart = new PartInfo(
                        Name: entry.PartName, Bounds: new PartBounds(partLength, partWidth), Species: entry.Species, Quantity: entry.Qty,
                        TenonEdges: TenonEdge.None, MortiseEdges: MortiseEdge.None, ScrewHoleEdges: ScrewHoleEdge.None, ThinningPockets: ThinningPocketEdge.None,
                        EdgeBand: entry.EdgeBandSpecies, Notes: entry.Notes, TkHeight: tkHeight, TkDepth: tkDepth, CabinetModel: cabinet);
                }
            }
            else
            {
                // Original mapping for Standard/Drawer
                bool isEndPanel = entry.PartName.Contains("End", StringComparison.OrdinalIgnoreCase);
                double partTkH = cabinet is BaseCabinetModel ? tkHeight : 0;
                double partTkD = cabinet is BaseCabinetModel ? tkDepth : 0;

                mappedPart = new PartInfo(
                    Name: entry.PartName, Bounds: new PartBounds(entry.LengthIn, entry.WidthIn), Species: entry.Species, Quantity: entry.Qty,
                    TenonEdges: ResolveTenonEdges(entry.PartName, cabinet),
                    MortiseEdges: ResolveMortiseEdges(entry.PartName, cabinet),
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
            "Toekick" or "Toekick (Left)" or "Toekick (Right)" => TenonEdge.Top | TenonEdge.Left | TenonEdge.Right,
            "Top Stretcher (Front)" or "Drawer Stretcher" => TenonEdge.Left | TenonEdge.Right,
            "Back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => TenonEdge.Top | TenonEdge.Bottom | TenonEdge.Left,
            "Left Back" when cabinet is BaseCabinetModel basecab => TenonEdge.Left,
            "Right Back" when cabinet is BaseCabinetModel basecab => TenonEdge.Right,
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
        return partName.ToLowerInvariant() switch
        {
            "left end" or "right end" => MortiseEdge.Left | MortiseEdge.Right | MortiseEdge.Top | MortiseEdge.Bottom,
            "back" when cabinet is BaseCabinetModel basecab && ConvertDimension.FractionToDouble(basecab.BackThickness) != 0.25 => MortiseEdge.Right,
            "left back" when cabinet is BaseCabinetModel basecab => MortiseEdge.Bottom,
            "right back" when cabinet is BaseCabinetModel basecab => MortiseEdge.Bottom,
            "back" when cabinet is UpperCabinetModel uppercab && ConvertDimension.FractionToDouble(uppercab.BackThickness) != 0.25 => MortiseEdge.Right | MortiseEdge.Left,
            "top stretcher (back)" => MortiseEdge.Bottom,
            "deck" when cabinet is BaseCabinetModel basecab && basecab.HasTK => MortiseEdge.Top,
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
            ScrewPilotHoleDiameter: s.ScrewPilotHoleDiameter
        );
    }
}
