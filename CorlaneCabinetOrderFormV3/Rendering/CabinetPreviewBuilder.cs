using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// =============================================================================
// CabinetPreviewBuilder.cs
// CorlaneCabinetOrderFormV3.Rendering
//
// Top-level orchestrator for building the 3D cabinet preview Model3DGroup shown
// in the HelixViewport3D. Acts as the single entry point that dispatches to the
// correct type-specific builder (BaseCabinetBuilder, UpperCabinetBuilder,
// FillerAndPanelBuilder) based on the runtime CabinetModel subtype.
//
// Entry points:
//   - BuildPreviewModel: Called by the preview ViewModel. Resets all material
//     and edge-banding accumulators on the CabinetModel, then performs a single
//     build pass so geometry is generated (and totals accumulated) for every
//     part. Parts flagged as hidden are excluded from the returned Model3DGroup
//     but still built internally so their material/edge totals are captured.
//     Adds a DirectionalLight and freezes the result before returning.
//
//   - BuildCabinetForTotals: Convenience wrapper that calls BuildCabinetForPreview
//     with all hide flags false. Used when only material/edge totals are needed
//     with no visibility filtering (e.g., BOM/cut-list generation paths).
//
//   - BuildCabinetForPreview: Core dispatch method. Routes to the appropriate
//     builder based on cab type and forwards all hide flags and helper delegates
//     (GetMatchingEdgebandingSpecies, ResolveDoorSpeciesForTotals, AddFrontPartRow,
//     AddDrawerBoxRow) so builders remain stateless.
//
//   - BuildCabinetWithResult: Testing/inspection entry point. Builds a base
//     cabinet and returns a CabinetBuildResult populated by the builder with
//     intermediate computed values, enabling unit tests to assert on geometry
//     outputs without going through the full preview pipeline.
//
// Notes:
//   - All returned Model3DGroups are frozen (via TryFreeze) before being handed
//     to the UI thread, keeping WPF rendering efficient.
//   - Material and edge totals are a side-effect of CreatePanel inside each
//     builder — BuildPreviewModel's reset + single-pass pattern ensures totals
//     are always consistent with what the preview would show.
// =============================================================================

internal static class CabinetPreviewBuilder
{
    internal static Model3DGroup BuildPreviewModel(
    CabinetModel? cab,
    bool leftEndHidden,
    bool rightEndHidden,
    bool deckHidden,
    bool topHidden,
    bool doorsHidden)
    {
        var group = new Model3DGroup();

        if (cab is not null)
        {
            // Reset totals before the single build pass.
            // Totals are accumulated inside CreatePanel as a side-effect,
            // so every part must call CreatePanel even when hidden from preview.
            cab.ResetAllMaterialAndEdgeTotals();

            // Single build: accumulates totals for ALL parts; hides geometry per flags.
            var built = BuildCabinetForPreview(cab, leftEndHidden, rightEndHidden, deckHidden, topHidden, doorsHidden);
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
            topHidden: false,
            doorsHidden: false);

    internal static Model3DGroup BuildCabinetForPreview(
        CabinetModel cab,
        bool leftEndHidden,
        bool rightEndHidden,
        bool deckHidden,
        bool topHidden,
        bool doorsHidden)
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
                doorsHidden,
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
                doorsHidden,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow);
        }
        else if (cab is FillerModel filler)
        {
            FillerAndPanelBuilder.BuildFiller(cabinet, filler, getEb);
        }
        else if (cab is PanelModel panel)
        {
            FillerAndPanelBuilder.BuildPanel(cabinet, panel);
        }

        TryFreeze(cabinet);
        return cabinet;
    }

    /// <summary>
    /// Builds the cabinet and returns a <see cref="CabinetBuildResult"/> populated
    /// by the builders as they compute values. Single source of truth for all cabinet types
    /// (Base, Upper, Filler, Panel). Use this in tests to assert against computed values.
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
                doorsHidden: false,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow,
                CabinetBuildHelpers.AddDrawerBoxRow,
                result);
        }
        else if (cab is UpperCabinetModel upperCab)
        {
            UpperCabinetBuilder.BuildUpper(
                cabinet,
                upperCab,
                leftEndHidden: false,
                rightEndHidden: false,
                deckHidden: false,
                topHidden: false,
                doorsHidden: false,
                getEb,
                resolveDoorSpecies,
                CabinetBuildHelpers.AddFrontPartRow,
                result);
        }
        else if (cab is FillerModel fillerCab)
        {
            FillerAndPanelBuilder.BuildFiller(cabinet, fillerCab, getEb);
        }
        else if (cab is PanelModel panelCab)
        {
            FillerAndPanelBuilder.BuildPanel(cabinet, panelCab);
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