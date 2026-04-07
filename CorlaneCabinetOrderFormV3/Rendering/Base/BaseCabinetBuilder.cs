using System.Windows.Media.Media3D;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    internal static void BuildBase(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow)
    {
        // Normal path — no result capture
        BuildBase(cabinet, baseCab, leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
            getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
            addFrontPartRow, addDrawerBoxRow, result: null);
    }

    /// <summary>
    /// Builds a base cabinet and populates <paramref name="result"/> with
    /// every intermediate calculated dimension as they are computed.
    /// </summary>
    internal static void BuildBase(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow,
        CabinetBuildResult? result)
    {
        var dim = BaseCabinetDimensions.From(baseCab);

        if (baseCab.Style == CabinetStyles.Base.Standard || baseCab.Style == CabinetStyles.Base.Drawer)
        {
            BuildStandardOrDrawer(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow, addDrawerBoxRow, result);
        }
        else if (baseCab.Style == CabinetStyles.Base.Corner90)
        {
            BuildCorner90(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
        else if (baseCab.Style == CabinetStyles.Base.AngleFront)
        {
            BuildAngleFront(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
    }
}