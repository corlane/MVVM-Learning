using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class EndPanels
{
    public static List<Point3D> BuildEndPanels(BaseCabinetModel baseCab, double height, double depth, double tk_Height, double tk_Depth)
    {
        List<Point3D> endPanelPoints;
        if (baseCab.HasTK)
        {
            endPanelPoints =
            [
                new (depth,tk_Height,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0),
                new (3,0,0),
                new (3,.5,0),
                new (depth-tk_Depth-3,.5,0),
                new (depth-tk_Depth-3,0,0),
                new (depth-tk_Depth,0,0),
                new (depth-tk_Depth,tk_Height,0)
            ];
        }
        else
        {
            endPanelPoints =
            [
                new (depth,0,0),
                new (depth,height,0),
                new (0,height,0),
                new (0,0,0)
            ];
        }

        return endPanelPoints;
    }
}
