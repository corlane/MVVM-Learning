using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.Calculators;

/// <summary>
/// Computes through screw holes for mortise-to-tenon assembly on all four edges,
/// handling base cabinets, upper cabinets, stretchers, nailers, and toe kicks.
/// </summary>
internal static class ScrewHoleEdgeCalculator
{
    internal static void ComputeScrewHoles(PartInfo part, List<(double, double, double)> holesThru, JoineryConfig joinery, double mt34)
    {
        var baseCab = part.CabinetModel as BaseCabinetModel;
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;
        double stretcherWidth = 6;
        double upperNailerWidth = 4;
        double topStretcherBackWidth = 3;

        // --------------------------------------------------------- LEFT -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
        {
            double openingHeight = ConvertDimension.FractionToDouble(baseCab!.OpeningHeight1) + mt34;
            double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
            double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
            double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

            if (baseCab.TopType == "Stretcher") // Holes for Std/Drw Base Cab stretcher top
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Front hole
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: stretcherWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );

                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Rear hole
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: topStretcherBackWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: height - topStretcherBackWidth,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }

            if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1) // Holes for Base Std 1 Drw Drawer Stretcher
            {

                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: stretcherWidth,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: mt34 + opening1Height,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: true,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }


            if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1) // Holes for Base Drw Drawer Stretchers
            {
                for (int i = 0; i < baseCab.DrwCount; i++)
                {
                    if (i == 1) openingHeight += opening2Height + mt34;
                    if (i == 2) openingHeight += opening3Height + mt34;

                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: openingHeight,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left) && part.CabinetModel is UpperCabinetModel upperCab && part.Name.Contains("End")) // Upper Cabinet Top screw holes
        {
            if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 1/4" back Top Screw Holes
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Top Screw Holes
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Left)) // Generic catch-all Left Edge screw holes
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Holes for Std/Drw base Cab full top
            (
                Edge: ScrewHoleEdge.Left,
                EdgeLength: height,
                PartWidth: length,
                PartHeight: height,
                OffsetFromEdge: 0,
                OffsetAlongEdge: 0,
                ForceTwoTenons: false,
                BlindStartOverride: 2.75,
                BlindStopOverride: 2.75,
                MaterialThickness34: mt34,
                IncludeEndHoles: true), joinery)
            );
        }


        // --------------------------------------------------------- RIGHT -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right) && part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
        {
            if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Cabinet End Panel Deck Screw Holes, 1/4" back
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else // Base Cabinet End Panel Deck Screw Holes, 3/4" back
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right) && part.CabinetModel is UpperCabinetModel upperCab && part.Name.Contains("End")) // Upper Cabinet Deck screw holes
        {
            if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 1/4" back Deck Screw Holes
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
            else
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Deck Screw Holes
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height - mt34,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }

        else if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Right)) // Generic catch-all Right Edge screw holes
        {
            holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec // Upper Cab 3/4" back Deck Screw Holes
            (
                Edge: ScrewHoleEdge.Right,
                EdgeLength: height,
                PartWidth: length,
                PartHeight: height,
                OffsetFromEdge: 0,
                OffsetAlongEdge: 0,
                ForceTwoTenons: false,
                BlindStartOverride: 2.75,
                BlindStopOverride: 2.75,
                MaterialThickness34: mt34,
                IncludeEndHoles: true), joinery)
            );
        }


        // --------------------------------------------------------- BOTTOM -------------------------------------------------------------------
        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Bottom))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel && baseCab!.HasTK) // End Panel Screw Holes for toekick
            {
                double tkBottomOffsetAlong = length - ConvertDimension.FractionToDouble(baseCab.TKHeight);
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: ConvertDimension.FractionToDouble(baseCab.TKHeight) - 0.5,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: part.TkDepth,
                    OffsetAlongEdge: tkBottomOffsetAlong,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }

            else // Generic catch-all for Bottom Edge screw holes
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }




        // --------------------------------------------------------- TOP -------------------------------------------------------------------

        if (part.ScrewHoleEdges.HasFlag(ScrewHoleEdge.Top))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25) // Base Cab nailer screw holes, 1/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: stretcherWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)                    
                    );
                }

                else // Base Cabinet End Panel Back Screw Holes, 3/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length - mt34 - part.TkHeight,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25) // Upper Cab nailer screw holes, 1/4" back
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );

                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: upperNailerWidth,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: length - upperNailerWidth - mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 0,
                        BlindStopOverride: 0,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }

            else // Generic catch-all for Top Edge screw holes
            {
                if (part.CabinetModel is BaseCabinetModel && !part.Name.Contains("Deck")) // Base cabinet decks do not get screw holes for toekick mortises
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: part.TkDepth,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34), joinery)
                    );
                }
            }
        }
    }

    /// <summary>
    /// Computes assembly screw holes for parts that have MortiseThruEdges set but no ScrewHoleEdges.
    /// Mirrors the edge-by-edge logic from MortiseThruCalculator so screw holes align with thru-mortise positions.
    /// </summary>
    internal static void ComputeScrewHolesFromMortiseThru(PartInfo part, List<(double, double, double)> holesThru, JoineryConfig joinery, double mt34)
    {
        var baseCab = part.CabinetModel as BaseCabinetModel;
        double length = part.Bounds.Width;
        double height = part.Bounds.Height;

        // --------------------------------------------------------- LEFT -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Left))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                double openingHeight = ConvertDimension.FractionToDouble(baseCab!.OpeningHeight1) + mt34;
                double opening1Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1);
                double opening2Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2);
                double opening3Height = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3);

                if (baseCab.TopType == "Stretcher")
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: 6, // stretcherWidth
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: 6, // stretcherWidth
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: opening1Height + mt34,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }

                if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
                {
                    for (int i = 0; i < baseCab.DrwCount; i++)
                    {
                        if (i == 1) openingHeight += opening2Height + mt34;
                        if (i == 2) openingHeight += opening3Height + mt34;

                        holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                        (
                            Edge: ScrewHoleEdge.Left,
                            EdgeLength: 6, // stretcherWidth
                            PartWidth: length,
                            PartHeight: height,
                            OffsetFromEdge: openingHeight,
                            OffsetAlongEdge: 0,
                            ForceTwoTenons: true,
                            BlindStartOverride: 1.25,
                            BlindStopOverride: 1.25,
                            MaterialThickness34: mt34,
                            IncludeEndHoles: false), joinery)
                        );
                    }
                }
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck"))
            {
                double leftDeckOffsetAlong = ConvertDimension.FractionToDouble(base90corner.RightBackWidth) - (3 * mt34) - base90corner.ToeKickRightWidth;
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: base90corner.ToeKickRightWidth,
                    PartWidth: 0, // length
                    PartHeight: 0, // height
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.LeftFrontWidth) - mt34 + ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: leftDeckOffsetAlong,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }

            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Left,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
            }

            else // Standard catch-all Left Edge screw holes for thru-mortises
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Left,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }



        // --------------------------------------------------------- RIGHT -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Right))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: ConvertDimension.FractionToDouble(baseCab.TKHeight),
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
            }

            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Right,
                        EdgeLength: height,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Right,
                        EdgeLength: height - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
            }

            else // Standard catch-all Right Edge screw holes for thru-mortises
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Right,
                    EdgeLength: height,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }



        // --------------------------------------------------------- BOTTOM -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Bottom))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel && baseCab!.HasTK)
            {
                double tkBottomOffsetAlong = length - ConvertDimension.FractionToDouble(baseCab.TKHeight);
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: ConvertDimension.FractionToDouble(baseCab.TKHeight) - 0.5,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: part.TkDepth,
                    OffsetAlongEdge: tkBottomOffsetAlong,
                    ForceTwoTenons: true,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: false), joinery)
                );
            }
            else // Standard catch-all for Bottom Edge screw holes
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Bottom,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }



        // --------------------------------------------------------- TOP -------------------------------------------------------------------

        if (part.MortiseThruEdges.HasFlag(MortiseThruEdge.Top))
        {
            if (part.Name.Contains("End") && part.CabinetModel is BaseCabinetModel)
            {
                if (ConvertDimension.FractionToDouble(baseCab!.BackThickness) == 0.25)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: 6, // stretcherWidth
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: true,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: false), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length - part.TkHeight - mt34,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
            }
            else if (part.Name.Contains("End") && part.CabinetModel is UpperCabinetModel upperCab)
            {
                if (ConvertDimension.FractionToDouble(upperCab.BackThickness) == 0.25)
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: 4, // upperNailerWidth
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );

                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: 4, // upperNailerWidth
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: length - 4 - mt34,
                        ForceTwoTenons: false,
                        BlindStartOverride: 1.25,
                        BlindStopOverride: 1.25,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
                else
                {
                    holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                    (
                        Edge: ScrewHoleEdge.Top,
                        EdgeLength: length,
                        PartWidth: length,
                        PartHeight: height,
                        OffsetFromEdge: 0,
                        OffsetAlongEdge: 0,
                        ForceTwoTenons: false,
                        BlindStartOverride: 2.75,
                        BlindStopOverride: 2.75,
                        MaterialThickness34: mt34,
                        IncludeEndHoles: true), joinery)
                    );
                }
            }
            else if (part.Name.Contains("Deck") && part.CabinetModel is BaseCabinetModel baseCabStd && (baseCabStd.Style == CabinetStyles.Base.Standard || baseCabStd.Style == CabinetStyles.Base.Drawer))
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: ConvertDimension.FractionToDouble(baseCabStd.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 1.25,
                    BlindStopOverride: 1.25,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }

            else if (part.CabinetModel is BaseCabinetModel base90corner && base90corner.Style == CabinetStyles.Base.Corner90 && part.Name.Contains("Deck"))
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Top,
                    EdgeLength: base90corner.ToeKickLeftWidth,
                    PartWidth: 0, // length
                    PartHeight: ConvertDimension.FractionToDouble(base90corner.LeftDepth) - (2 * mt34),
                    OffsetFromEdge: ConvertDimension.FractionToDouble(base90corner.TKDepth),
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 0,
                    BlindStopOverride: 0,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }

            else // Standard catch-all for Top Edge screw holes
            {
                holesThru.AddRange(MortiseScrewHoleCalculator.ComputeScrewHolesFromSpecs(new ScrewHolePlacementSpec
                (
                    Edge: ScrewHoleEdge.Top,
                    EdgeLength: length,
                    PartWidth: length,
                    PartHeight: height,
                    OffsetFromEdge: 0,
                    OffsetAlongEdge: 0,
                    ForceTwoTenons: false,
                    BlindStartOverride: 2.75,
                    BlindStopOverride: 2.75,
                    MaterialThickness34: mt34,
                    IncludeEndHoles: true), joinery)
                );
            }
        }
    }
}
