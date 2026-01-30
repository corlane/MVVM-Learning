using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetSimpleBuilders
{
    internal static void BuildFiller(
        Model3DGroup cabinet,
        FillerModel filler,
        Func<string?, string> getMatchingEdgebandingSpecies)
    {
        Model3DGroup leftEnd;
        Model3DGroup back;

        List<Point3D> endPanelPoints;
        List<Point3D> backPoints;

        double MaterialThickness34 = 0.75;

        double width = ConvertDimension.FractionToDouble(filler.Width);
        double height = ConvertDimension.FractionToDouble(filler.Height);
        double depth = ConvertDimension.FractionToDouble(filler.Depth);
        bool topDeck90 = false;
        bool isPanel = false;
        string panelEBEdges = "";

        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, filler.Species, "None", "Vertical", filler, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
        ModelTransforms.ApplyTransform(leftEnd, 0, 0, -MaterialThickness34, 0, 270, 0);

        backPoints =
        [
            new (0,height,0),
            new (0,0,0),
            new (width,0,0),
            new (width,height,0)
        ];

        back = CabinetPartFactory.CreatePanel(backPoints, MaterialThickness34, filler.Species, getMatchingEdgebandingSpecies(filler.Species), "Vertical", filler, topDeck90, isPanel: true, panelEBEdges: "NNLR", isFaceUp: false);
        ModelTransforms.ApplyTransform(back, 0, 0, depth, 0, 0, 0);

        cabinet.Children.Add(leftEnd);
        cabinet.Children.Add(back);
    }

    internal static void BuildPanel(Model3DGroup cabinet, PanelModel panel)
    {
        Model3DGroup back;

        List<Point3D> backPoints;

        double width = ConvertDimension.FractionToDouble(panel.Width);
        double height = ConvertDimension.FractionToDouble(panel.Height);
        double depth = ConvertDimension.FractionToDouble(panel.Depth);
        bool topDeck90 = false;
        bool isPanel = true;

        string panelEBEdges = "";
        if (panel.PanelEBTop) { panelEBEdges += "T"; }
        if (panel.PanelEBBottom) { panelEBEdges += "B"; }
        if (panel.PanelEBLeft) { panelEBEdges += "L"; }
        if (panel.PanelEBRight) { panelEBEdges += "R"; }

        backPoints =
        [
            new (0,0,0),
            new (width,0,0),
            new (width,height,0),
            new (0,height,0)
        ];

        back = CabinetPartFactory.CreatePanel(backPoints, depth, panel.Species, panel.EBSpecies, "Vertical", panel, topDeck90, isPanel, panelEBEdges, isFaceUp: false);
        ModelTransforms.ApplyTransform(back, 0, 0, depth / 2, 0, 0, 0);

        cabinet.Children.Add(back);
    }
}