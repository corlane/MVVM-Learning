using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Adds the three sink clip cuts (left tab, right tab, center slot)
    /// to a stretcher panel in its local coordinate space.
    /// </summary>
    private static void AddSinkCuts(
        Model3DGroup panel, 
        double interiorWidth, 
        double width, 
        double stretcherWidth, 
        double materialThickness)

    {
        double cutWidth = 0.5;
        double cutLength = 4.5;
        double cutRimZ = materialThickness;
        double cutBottomZ = 0;

        double cut1CenterX = 2;
        double cut1CenterY = stretcherWidth - (cutLength / 2);

        panel.Children.Add(CabinetPartFactory.CreateRectangularCut(
            cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));

        cut1CenterX = width - materialThickness - 2.75;

        panel.Children.Add(CabinetPartFactory.CreateRectangularCut(
            cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));

        cut1CenterX = interiorWidth / 2;
        cut1CenterY = 1.75;

        cutLength = .5;
        cutWidth = width - (2.75 * 2);

        panel.Children.Add(CabinetPartFactory.CreateRectangularCut(
            cut1CenterX, cut1CenterY, cutRimZ, cutBottomZ, cutWidth, cutLength));
    }

}
