using CorlaneCabinetOrderFormV3.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetPreviewBuilder
{
    internal static Model3DGroup BuildPreviewModel(
    CabinetModel? cab,
    bool leftEndHidden,
    bool rightEndHidden,
    bool deckHidden,
    bool topHidden)
    {
        var group = new Model3DGroup();

        if (cab is not null)
        {
            // Reset totals before the single build pass.
            // Totals are accumulated inside CreatePanel as a side-effect,
            // so every part must call CreatePanel even when hidden from preview.
            cab.ResetAllMaterialAndEdgeTotals();

            // Single build: accumulates totals for ALL parts; hides geometry per flags.
            var built = BuildCabinetForPreview(cab, leftEndHidden, rightEndHidden, deckHidden, topHidden);
            group.Children.Add(built);
        }

        group.Children.Add(new DirectionalLight(Colors.DarkGray, new Vector3D(-1, -1, -1)));

        TryFreeze(group);

        return group;
    }

    internal static Model3DGroup BuildCabinetForTotals(CabinetModel cab)
        => BuildCabinetForPreview(
            cab,
            leftEndHidden: false,
            rightEndHidden: false,
            deckHidden: false,
            topHidden: false);

    internal static Model3DGroup BuildCabinetForPreview(
        CabinetModel cab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden)
    {
        var cabinet = new Model3DGroup();

        var getEb = CabinetBuildHelpers.GetMatchingEdgebandingSpecies;
        var resolveDoorSpecies = CabinetBuildHelpers.ResolveDoorSpeciesForTotals;

        if (cab is BaseCabinetModel baseCab)
        {
            BaseCabinetBuilder.BuildBase(
                cabinet,
                baseCab,
                leftEndHidden,
                rightEndHidden,
                deckHidden,
                topHidden,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow,
                CabinetBuildHelpers.AddDrawerBoxRow);
        }
        else if (cab is UpperCabinetModel upperCab)
        {
            UpperCabinetBuilder.BuildUpper(
                cabinet,
                upperCab,
                leftEndHidden,
                rightEndHidden,
                deckHidden,
                topHidden,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow);
        }
        else if (cab is FillerModel filler)
        {
            CabinetSimpleBuilders.BuildFiller(cabinet, filler, getEb);
        }
        else if (cab is PanelModel panel)
        {
            CabinetSimpleBuilders.BuildPanel(cabinet, panel);
        }

        TryFreeze(cabinet);
        return cabinet;
    }

    /// <summary>
    /// Builds the cabinet and returns a <see cref="CabinetBuildResult"/> populated
    /// by the builders as they compute values. Single source of truth.
    /// </summary>
    internal static CabinetBuildResult BuildCabinetWithResult(CabinetModel cab)
    {
        var cabinet = new Model3DGroup();
        var result = new CabinetBuildResult();

        var getEb = CabinetBuildHelpers.GetMatchingEdgebandingSpecies;
        var resolveDoorSpecies = CabinetBuildHelpers.ResolveDoorSpeciesForTotals;

        if (cab is BaseCabinetModel baseCab)
        {
            BaseCabinetBuilder.BuildBase(
                cabinet,
                baseCab,
                leftEndHidden: false,
                rightEndHidden: false,
                deckHidden: false,
                topHidden: false,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow,
                CabinetBuildHelpers.AddDrawerBoxRow,
                result);
        }

        TryFreeze(cabinet);
        return result;
    }

    private static void TryFreeze(Freezable freezable)
    {
        if (freezable.CanFreeze && !freezable.IsFrozen)
        {
            freezable.Freeze();
        }
    }
}