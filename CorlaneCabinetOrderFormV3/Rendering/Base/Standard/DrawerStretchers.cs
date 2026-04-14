using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class DrawerStretchers
{
    /// <summary>
    /// Builds and positions drawer stretchers for Standard (1-drawer)
    /// and Drawer style cabinets, including sink stretcher/clips when applicable.
    /// </summary>
    public static void BuildDrawerStretchers(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim)
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
            new (0,0,0),
            new (interiorWidth,0,0),
            new (interiorWidth,StretcherWidth,0),
            new (0,StretcherWidth,0)
        ];

        // Standard style with 1 drawer
        if (cabType == style1 && baseCab.DrwCount == 1)
        {
            double topDeckAndStretcherThickness = (baseCab.DrwCount + 1) * MaterialThickness34;

            var points = baseCab.TrashDrawer
                ? new List<Point3D>
                  {
                      new (0,0,0),
                      new (interiorWidth,0,0),
                      new (interiorWidth,interiorDepth,0),
                      new (0,interiorDepth,0)
                  }
                : stretcherPoints;

            var stretcher = CabinetPartFactory.CreatePanel(points, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);

            if (baseCab.SinkCabinet)
            {
                SinkCuts.AddSinkCuts(stretcher, interiorWidth, width, StretcherWidth, MaterialThickness34);
            }

            ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - topDeckAndStretcherThickness - opening1Height, 270, 0, 0);
            cabinet.Children.Add(stretcher);

            if (baseCab.SinkCabinet)
            {
                List<Point3D> sinkStretcherPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,opening1Height,0),
                    new (0,opening1Height,0)
                ];

                stretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.SinkStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                cabinet.Children.Add(stretcher);
            }
        }

        // Drawer style (2–4 drawers)
        if (cabType == style2)
        {
            double opening1HeightAdjusted = opening1Height;
            double opening2HeightAdjusted = opening2Height;
            double opening3HeightAdjusted = opening3Height;

            if (baseCab.DrwCount == 2)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                var stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);
            }

            if (baseCab.DrwCount == 3)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                var stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                opening2HeightAdjusted += MaterialThickness34;
                var stretcher2 = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher2, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher2);
            }

            if (baseCab.DrwCount == 4)
            {
                opening1HeightAdjusted += doubleMaterialThickness34;
                var stretcher = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher);

                opening2HeightAdjusted += MaterialThickness34;
                var stretcher2 = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher2, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher2);

                opening3HeightAdjusted += MaterialThickness34;
                var stretcher3 = CabinetPartFactory.CreatePanel(stretcherPoints, MaterialThickness34, baseCab.Species, baseCab.EBSpecies, "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.DrawerStretcher);
                ModelTransforms.ApplyTransform(stretcher3, -(interiorWidth / 2), -depth, height - opening1HeightAdjusted - opening2HeightAdjusted - opening3HeightAdjusted, 270, 0, 0);
                cabinet.Children.Add(stretcher3);
            }

            if (baseCab.SinkCabinet)
            {
                List<Point3D> sinkStretcherPoints =
                [
                    new (0,0,0),
                    new (interiorWidth,0,0),
                    new (interiorWidth,opening1Height,0),
                    new (0,opening1Height,0)
                ];

                var sinkStretcher = CabinetPartFactory.CreatePanel(sinkStretcherPoints, MaterialThickness34, baseCab.Species, "None", "Horizontal", baseCab, false, false, "", isFaceUp: false, partKind: CabinetPartKind.SinkStretcher);
                ModelTransforms.ApplyTransform(sinkStretcher, -(interiorWidth / 2), -height + MaterialThickness34, -depth, 180, 0, 0);
                cabinet.Children.Add(sinkStretcher);
            }
        }
    }

}
