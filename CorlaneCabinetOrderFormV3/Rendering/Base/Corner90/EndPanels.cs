using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void BuildEndPanels(BaseCabinetModel baseCab, double MaterialThickness34, double height, double leftDepth, double rightDepth, double tk_Height, double tk_Depth, bool topDeck90, bool isPanel, string panelEBEdges, out Model3DGroup leftEnd, out Model3DGroup rightEnd, out List<Point3D> leftEndPanelPoints, out List<Point3D> rightEndPanelPoints)
    {
        // End Panels
        if (baseCab.HasTK)
        {
            leftEndPanelPoints =
                [
                    new (leftDepth,tk_Height,0),
                    new (leftDepth,height,0),
                    new (0,height,0),
                    new (0,0,0),
                    new (3,0,0),
                    new (3,.5,0),
                    new (leftDepth-tk_Depth-3,.5,0),
                    new (leftDepth-tk_Depth-3,0,0),
                    new (leftDepth-tk_Depth,0,0),
                    new (leftDepth-tk_Depth,tk_Height,0)
                ];

            rightEndPanelPoints =
                [
                    new (rightDepth,tk_Height,0),
                    new (rightDepth,height,0),
                    new (0,height,0),
                    new (0,0,0),
                    new (3,0,0),
                    new (3,.5,0),
                    new (rightDepth-tk_Depth-3,.5,0),
                    new (rightDepth-tk_Depth-3,0,0),
                    new (rightDepth-tk_Depth,0,0),
                    new (rightDepth-tk_Depth,tk_Height,0)
                ];
        }
        else
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
        }

        leftEnd = CabinetPartFactory.CreatePanel(leftEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.LeftEnd);
        rightEnd = CabinetPartFactory.CreatePanel(rightEndPanelPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Vertical", baseCab, topDeck90, isPanel, panelEBEdges, isFaceUp: true, partKind: CabinetPartKind.RightEnd);
    }
}
