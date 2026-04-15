using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class UpperCabinetBuilder
{
    private static void BuildEndPanels(UpperCabinetModel upperCab, double MaterialThickness34, double height, double depth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> endPanelPoints)
    {
        endPanelPoints =
        [
            new (depth,0,0),
            new (depth,height,0),
            new (0,height,0),
            new (0,0,0)
        ];

        leftEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, isFaceUp: true, CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(endPanelPoints, MaterialThickness34, upperCab.Species, upperCab.EBSpecies, "Vertical", upperCab, isFaceUp: true, CabinetPartKind.RightEnd);
    }
}