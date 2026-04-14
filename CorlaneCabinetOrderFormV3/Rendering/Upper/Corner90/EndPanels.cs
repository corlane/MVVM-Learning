using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildEndPanels90(UpperCabinetModel upperCab, double MaterialThickness34, double height, double leftDepth, double rightDepth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints)
    {
        leftEndPanelPoints =
        [
            new (leftDepth,0,0),
            new (leftDepth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        rightEndPanelPoints =
        [
            new (rightDepth,0,0),
            new (rightDepth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);
    }
}