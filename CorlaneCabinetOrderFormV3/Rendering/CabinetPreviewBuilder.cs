using CorlaneCabinetOrderFormV3.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetPreviewBuilder
{
    private readonly record struct PreviewCacheKey(
        CabinetModel Cab,
        int GeometryVersion,
        bool LeftEndHidden,
        bool RightEndHidden,
        bool DeckHidden,
        bool TopHidden);

    private static readonly object _cacheSync = new();
    private static PreviewCacheKey? _lastPreviewKey;
    private static Model3DGroup? _lastPreviewModel;

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
            // Totals (full cabinet, independent of preview hides)
            cab.ResetAllMaterialAndEdgeTotals();
            _ = BuildCabinetForTotals(cab);

            // Preview (may hide parts; cached)
            var built = BuildCabinetForPreviewCached(cab, leftEndHidden, rightEndHidden, deckHidden, topHidden);
            group.Children.Add(built);
        }

        group.Children.Add(new DirectionalLight(Colors.DarkGray, new Vector3D(-1, -1, -1)));

        // Freeze the final group so it can be reused by WPF cheaply if desired.
        // (Lights/materials created by builders are typically freezable.)
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

    private static Model3DGroup BuildCabinetForPreviewCached(
        CabinetModel cab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden)
    {
        var key = new PreviewCacheKey(cab, cab.GeometryVersion, leftEndHidden, rightEndHidden, deckHidden, topHidden);

        lock (_cacheSync)
        {
            if (_lastPreviewKey is PreviewCacheKey lastKey &&
                _lastPreviewModel is not null &&
                ReferenceEquals(lastKey.Cab, key.Cab) &&
                lastKey.GeometryVersion == key.GeometryVersion &&
                lastKey.LeftEndHidden == key.LeftEndHidden &&
                lastKey.RightEndHidden == key.RightEndHidden &&
                lastKey.DeckHidden == key.DeckHidden &&
                lastKey.TopHidden == key.TopHidden)
            {
                return _lastPreviewModel;
            }
        }

        var built = BuildCabinetForPreview(cab, leftEndHidden, rightEndHidden, deckHidden, topHidden);

        lock (_cacheSync)
        {
            _lastPreviewKey = key;
            _lastPreviewModel = built;
        }

        return built;
    }

    private static void TryFreeze(Freezable freezable)
    {
        if (freezable.CanFreeze && !freezable.IsFrozen)
        {
            freezable.Freeze();
        }
    }
}