using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Builds and positions drawer stretchers for Standard (1-drawer)
    /// and Drawer style cabinets, including sink stretcher/clips when applicable.
    /// </summary>
    private static void BuildDrawerStretchers(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        bool hasLeftEnd,
        bool hasRightEnd)

    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double doubleMaterialThickness34 = MaterialThickness34 * 2;
        double StretcherWidth = 6;

        string style1 = CabinetStyles.Base.Standard;
        string style2 = CabinetStyles.Base.Drawer;
        string? cabType = baseCab.Style;

        double width = dim.Width;
        double height = dim.Height;
        double depth = dim.Depth;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double opening1Height = dim.Opening1Height;
        double opening2Height = dim.Opening2Height;
        double opening3Height = dim.Opening3Height;

        List<Point3D> stretcherPoints =
        [
            new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
            new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,StretcherWidth,0),
            new (hasLeftEnd ? 0 : -MaterialThickness34,StretcherWidth,0)
        ];

        // Standard style with 1 drawer
        if (cabType == style1 && baseCab.DrwCount == 1)
        {
            double topDeckAndStretcherThickness = (baseCab.DrwCount + 1) * MaterialThickness34;

            var points = baseCab.TrashDrawer
                ? new List<Point3D>
                  {
                      new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                      new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                      new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,interiorDepth,0),
                      new (hasLeftEnd ? 0 : -MaterialThickness34,interiorDepth,0)
                  }
                : stretcherPoints;

            var stretcher = CabinetPartFactory.CreatePanel(points, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.DrawerStretcher);
            if (baseCab.SinkCabinet)
            {
                AddSinkCuts(stretcher, interiorWidth, width, StretcherWidth, MaterialThickness34);
            }

            ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - topDeckAndStretcherThickness - opening1Height, 270, 0, 0);
            cabinet.Children.Add(stretcher);

            if (baseCab.SinkCabinet)
            {
                List<Point3D> sinkStretcherPoints =
                [
                    new (hasLeftEnd ? 0 : -MaterialThickness34,0,0),
                    new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,0,0),
                    new (hasRightEnd ? interiorWidth : interiorWidth + MaterialThickness34,opening1Height,0),
                    new (hasLeftEnd ? 0 : -MaterialThickness34,opening1Height,0)
                ];

                stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.SinkStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                cabinet.Children.Add(stretcher);
            }
        }

        // Drawer style (2–4 drawers)
        if (cabType == style2)
        {
            double[] openingHeights = [opening1Height, opening2Height, opening3Height];

            // First stretcher always gets doubleMaterialThickness34 added; subsequent ones get single
            double[] thicknessOffsets = [doubleMaterialThickness34, MaterialThickness34, MaterialThickness34];

            int stretcherCount = baseCab.DrwCount - 1;
            double cumulativeHeight = 0;

            for (int i = 0; i < stretcherCount; i++)
            {
                double adjustedOpening = openingHeights[i] + thicknessOffsets[i];
                cumulativeHeight += adjustedOpening;

                var stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, isFaceUp: false, CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - cumulativeHeight, 270, 0, 0);
                cabinet.Children.Add(stretcher);
            }
        }
    }
}
