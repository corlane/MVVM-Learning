using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    internal static void BuildUpper(
        Model3DGroup cabinet,
        UpperCabinetModel upperCab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden,
        Func<string?, string> getMatchingEdgebandingSpecies,
        Func<string?, string?, string> resolveDoorSpeciesForTotals,
        Action<UpperCabinetModel, string, double, double, string?, string?> addFrontPartRow)
    {
        var dim = UpperCabinetDimensions.From(upperCab);

        if (string.Equals(upperCab.Style, CabinetStyles.Upper.Standard, StringComparison.OrdinalIgnoreCase))
        {
            BuildStandard(cabinet, upperCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
        else if (upperCab.Style == CabinetStyles.Upper.Corner90)
        {
            BuildCorner90(cabinet, upperCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
        else if (upperCab.Style == CabinetStyles.Upper.AngleFront)
        {
            BuildAngleFront(cabinet, upperCab, dim,
                leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden,
                getMatchingEdgebandingSpecies, resolveDoorSpeciesForTotals,
                addFrontPartRow);
        }
    }
}