using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

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
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<BaseCabinetModel, string, double, double, string?, string?> addFrontPartRow,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow)
    {
        var dim = BaseCabinetDimensions.From(baseCab);

        if (baseCab.Style == CabinetStyles.Base.Standard || baseCab.Style == CabinetStyles.Base.Drawer)
        {
            BuildStandardOrDrawer(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow, addDrawerBoxRow);
        }
        else if (baseCab.Style == CabinetStyles.Base.Corner90)
        {
            BuildCorner90(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
        else if (baseCab.Style == CabinetStyles.Base.AngleFront)
        {
            BuildAngleFront(cabinet, baseCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
    }
}