using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Material area and edgebanding tests for Fillers and Panels.
/// Panels have selective EB edges (PanelEBTop/Bottom/Left/Right) — these tests
/// verify that partial-EB combinations produce correct and stable totals.
/// </summary>
public class FillerAndPanelMatAndEBTests
{
    private static void RunOnSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { caught = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
            throw caught;
    }

    //############################################################################################################
    // Filler — produces material area and edgebanding
    //############################################################################################################

    [Fact]
    public void Filler_3x34_5_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var filler = MakeFiller();
            filler.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(filler);

            Assert.True(filler.TotalMaterialAreaFt2 > 0,
                "Filler should produce material area > 0");
        });
    }

    [Fact]
    public void Filler_3x34_5_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var filler = MakeFiller();
            filler.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(filler);

            Assert.True(filler.TotalEdgeBandingFeet > 0,
                "Filler should produce edgebanding > 0");
        });
    }

    [Fact]
    public void Filler_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var filler = MakeFiller();

            filler.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(filler);
            double mat1 = filler.TotalMaterialAreaFt2;
            double eb1 = filler.TotalEdgeBandingFeet;

            filler.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(filler);
            double mat2 = filler.TotalMaterialAreaFt2;
            double eb2 = filler.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    //############################################################################################################
    // Panel — all 4 EB edges ON
    //############################################################################################################

    [Fact]
    public void Panel_AllEdgesEB_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var panel = MakePanel(ebTop: true, ebBottom: true, ebLeft: true, ebRight: true);
            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);

            Assert.True(panel.TotalMaterialAreaFt2 > 0,
                "Panel with all EB edges should produce material area > 0");
        });
    }

    [Fact]
    public void Panel_AllEdgesEB_EdgeBanding_IsNonZero()
    {
        RunOnSta(() =>
        {
            var panel = MakePanel(ebTop: true, ebBottom: true, ebLeft: true, ebRight: true);
            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);

            Assert.True(panel.TotalEdgeBandingFeet > 0,
                "Panel with all EB edges should produce edgebanding > 0");
        });
    }

    //############################################################################################################
    // Panel — NO EB edges
    //############################################################################################################

    [Fact]
    public void Panel_NoEdgesEB_MaterialArea_IsNonZero()
    {
        RunOnSta(() =>
        {
            var panel = MakePanel(ebTop: false, ebBottom: false, ebLeft: false, ebRight: false);
            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);

            Assert.True(panel.TotalMaterialAreaFt2 > 0,
                "Panel with no EB should still produce material area > 0");
        });
    }

    [Fact]
    public void Panel_NoEdgesEB_EdgeBanding_IsZero()
    {
        RunOnSta(() =>
        {
            var panel = MakePanel(ebTop: false, ebBottom: false, ebLeft: false, ebRight: false);
            panel.EBSpecies = "None";
            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);

            Assert.Equal(0.0, panel.TotalEdgeBandingFeet, precision: 6);
        });
    }

    //############################################################################################################
    // Panel — partial EB produces less EB than full EB
    //############################################################################################################

    [Fact]
    public void Panel_PartialEB_ProducesLessEBThanAllEdges()
    {
        RunOnSta(() =>
        {
            var fullEB = MakePanel(ebTop: true, ebBottom: true, ebLeft: true, ebRight: true);
            fullEB.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(fullEB);
            double ebFull = fullEB.TotalEdgeBandingFeet;

            var partialEB = MakePanel(ebTop: true, ebBottom: false, ebLeft: true, ebRight: false);
            partialEB.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(partialEB);
            double ebPartial = partialEB.TotalEdgeBandingFeet;

            Assert.True(ebPartial > 0, "Partial EB should produce some edgebanding");
            Assert.True(ebPartial < ebFull, "Partial EB should produce less EB than all 4 edges");
        });
    }

    //############################################################################################################
    // Panel — material area is the same regardless of EB flags
    //############################################################################################################

    [Fact]
    public void Panel_MaterialArea_UnchangedByEBFlags()
    {
        RunOnSta(() =>
        {
            var allEB = MakePanel(ebTop: true, ebBottom: true, ebLeft: true, ebRight: true);
            allEB.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(allEB);
            double matAll = allEB.TotalMaterialAreaFt2;

            var noEB = MakePanel(ebTop: false, ebBottom: false, ebLeft: false, ebRight: false);
            noEB.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(noEB);
            double matNone = noEB.TotalMaterialAreaFt2;

            Assert.Equal(matAll, matNone, precision: 4);
        });
    }

    //############################################################################################################
    // Panel — totals stable between builds
    //############################################################################################################

    [Fact]
    public void Panel_Totals_StableBetweenBuilds()
    {
        RunOnSta(() =>
        {
            var panel = MakePanel(ebTop: true, ebBottom: false, ebLeft: true, ebRight: false);

            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);
            double mat1 = panel.TotalMaterialAreaFt2;
            double eb1 = panel.TotalEdgeBandingFeet;

            panel.ResetAllMaterialAndEdgeTotals();
            _ = CabinetPreviewBuilder.BuildCabinetForTotals(panel);
            double mat2 = panel.TotalMaterialAreaFt2;
            double eb2 = panel.TotalEdgeBandingFeet;

            Assert.Equal(mat1, mat2, precision: 6);
            Assert.Equal(eb1, eb2, precision: 6);
        });
    }

    //############################################################################################################
    // Helpers
    //############################################################################################################

    private static FillerModel MakeFiller() => new()
    {
        Name = "TestFiller",
        Qty = 1,
        Width = "3",
        Height = "34.5",
        Depth = "0.75",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
    };

    private static PanelModel MakePanel(
        bool ebTop, bool ebBottom, bool ebLeft, bool ebRight) => new()
    {
        Name = "TestPanel",
        Qty = 1,
        Width = "24",
        Height = "34.5",
        Depth = "0.75",
        Species = "Maple",
        CustomSpecies = "",
        EBSpecies = "Wood Maple",
        CustomEBSpecies = "",
        MaterialThickness34 = 0.75,
        MaterialThickness14 = 0.25,
        Notes = "",
        PanelEBTop = ebTop,
        PanelEBBottom = ebBottom,
        PanelEBLeft = ebLeft,
        PanelEBRight = ebRight,
    };
}