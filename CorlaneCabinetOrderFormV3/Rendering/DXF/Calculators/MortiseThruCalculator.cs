using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Computes through-mortise pockets for all four edges of a panel, mirroring the blind mortise logic
/// but intended for full-thickness joinery. Handles various cabinet styles and configurations.
/// </summary>
internal static class MortiseThruCalculator
{
    internal static void ComputeMortisePocketsThru(PartInfo part, List<(double, double, double, double)> mortisePockets, JoineryConfig joinery, double mt34)
    {
        var baseCab = part.CabinetModel as BaseCabinetModel;
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double stretcherWidth = 6;
        double upperNailerWidth = 4;

        // --------------------------------------------------------- LEFT -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Left))
        {
            if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Left) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                double openingHeight = ConvertDimension.FractionToDouble(baseCab!.OpeningHeight1) + mt34;
                double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
                double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
                double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

                if (baseCab.TopType == "Stretcher") // Stretcher Top
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );

                }
                else // Full Top
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1) // Base Std 1 Drw Stretcher
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: opening1Height + mt34,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: true,
                        MaterialThickness34: mt34), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1) // Drw Base Stretchers
                {
                    for (int i = 0; i < baseCab.DrwCount; i++)
                    {
                        if (i == 1) openingHeight += opening2Height + mt34;
                        if (i == 2) openingHeight += opening3Height + mt34;

                        mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                        (
                            Edge: MortiseEdge.Left,
                            EdgeLength: stretcherWidth,
                            PartWidth: length,
                            PartHeight: height,
                            OffsetFromEdge: openingHeight,
                            OffsetAlongEdge: 0,
                            ForceTwoTenons: true,
                            BlindStartOverride: 1.25,
                            BlindStopOverride: 1.25,
                            FullThicknessTenon: true,
                            MaterialThickness34: mt34), joinery)
                        );
                    }
                }
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck")) // Base Corner 90 deg Toekick (right)
            {
                double leftDeckOffsetAlong = ConvertDimension.FractionToDouble(base90corner.RightBackWidth) - (3 * mt34) - base90corner.ToeKickRightWidth;
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Left,
                    EdgeLength: base90corner.ToeKickRightWidth,
                    PartWidth: 0,//length,
                    PartHeight: 0,//height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.LeftFrontWidth) - mt34 + ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: leftDeckOffsetAlong,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Left) && part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper cab Top Mortises, 1/4" back
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec  // Upper cab Top Mortises, 3/4" back
                    (
                        Edge: MortiseEdge.Left,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else // Standard catch-all Left Edge mortise
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }



        // --------------------------------------------------------- RIGHT -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Right))
        {
            if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Right) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel) // Base Cabinet Deck, accounting for Back Thickness
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Right) && part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper cab Top Mortises, 1/4" back
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec  // Upper cab Top Mortises, 3/4" back
                    (
                        Edge: MortiseEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }



            else // Standard catch-all Right Edge mortise
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }



        // --------------------------------------------------------- BOTTOM -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Bottom))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel && baseCab!.HasTK) // Std Base Toekick End Panel Mortises
            {
                double tkBottomOffsetAlong = length - ConvertDimension.FractionToDouble(baseCab.TKHeight);
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Bottom,
                    EdgeLength: ConvertDimension.FractionToDouble(baseCab.TKHeight) - 0.5,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: part.TkDepth,
                    OffsetAlongEdge: tkBottomOffsetAlong,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
            else // Standard catch-all for Bottom Edge mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Bottom,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }



        // --------------------------------------------------------- TOP -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Top))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Nailer mortise (1/4" back)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else // Base Back mortise (3/4" back)
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: length - part.TkHeight - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper Cabinet 1/4" back end panel nailer mortises
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );

                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: length - upperNailerWidth - mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
                else // Upper Cabinet 3/4" back end panel mortises
                {
                    mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                    (
                        Edge: MortiseEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        FullThicknessTenon: false,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
            else if (part.Name.Contains("Deck") && part.CabinetModel is BaseCabinetModel baseCabStd && (baseCabStd.Style == CabinetStyles.Base.Standard || baseCabStd.Style == CabinetStyles.Base.Drawer)) // Base Std & Drw toekick top mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCabStd.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck"))
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: base90corner.ToeKickLeftWidth,
                    PartWidth: 0, //length,
                    PartHeight: ConvertDimension.FractionToDouble(base90corner.LeftDepth) - (2 * mt34),
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }

            else // Standard catch-all for Top Edge mortises
            {
                mortisePockets.AddRange(MortiseCalculator.ComputeMortisePocketsFromSpecs(new MortisePlacementSpec
                (
                    Edge: MortiseEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    FullThicknessTenon: false,
                    MaterialThickness34: mt34), joinery)
                );
            }
        }
    }
}
