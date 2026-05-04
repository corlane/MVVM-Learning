//    /// <summary>
//    /// Drills all end-panel holes (construction, back vertical, hinge, shelf, drawer slide)
//    /// into the left and right end panels in local coordinates.
//    /// Must be called before ApplyTransform on the end panels.
//    /// </summary>


using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    private static void DrillEndPanelHoles(
        Model3DGroup leftEnd,
        Model3DGroup rightEnd,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        LockDadoSettings settings) // <-- Added dependency for truth calculator
    {
        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double holeDepth = MaterialThickness34 / 2;
        double holeDiameter = 0.1968; // 5mm

        double depth = dim.Depth;
        double height = dim.Height;
        double tk_Height = dim.TKHeight;
        double backThickness = dim.BackThickness;

        // Construction holes (outside face) - Depth direction
        {
            bool topIsStretcher = string.Equals(baseCab.TopType, CabinetOptions.TopType.Stretcher, StringComparison.OrdinalIgnoreCase);
            double stretcherWidth = 6.0; // Truth: Top Stretcher Front width

            // Top edge holes
            var topHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                partDepth: topIsStretcher ? stretcherWidth : depth,
                mortiseBottomY: height - MaterialThickness34, // Truth: Y = height - 3/4"
                flushFace: topIsStretcher ? TenonFlushFace.Bottom : TenonFlushFace.Top,
                s: settings,
                xOffset: topIsStretcher ? depth - stretcherWidth : 0, // 0 places stretcher at front edge
                forceTwoTenons: topIsStretcher); // Truth: always exactly 2 tenons for stretchers

            foreach (var h in topHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
            }

            // Top Stretcher Back hole (single centered hole)
            if (topIsStretcher)
            {
                double backStretcherCenterX = 1.5; // 3" width / 2 from back edge
                double backStretcherCenterY = height - (MaterialThickness34 / 2.0); // Centered vertically on top edge

                leftEnd.Children.Add(CabinetPartFactory.CreateHole(backStretcherCenterX, backStretcherCenterY, MaterialThickness34, holeDepth, settings.ScrewPilotHoleDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(backStretcherCenterX, backStretcherCenterY, 0, holeDepth, settings.ScrewPilotHoleDiameter));
            }

            // Bottom edge holes (unchanged)
            var bottomHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                partDepth: depth,
                mortiseBottomY: tk_Height,
                flushFace: TenonFlushFace.Bottom,
                s: settings,
                xOffset: 0,
                forceTwoTenons: false);

            foreach (var h in bottomHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
            }
        }




        // Back vertical construction holes (outside face) - Height direction
        {
            var backHoles = PartOutlineBuilder.ComputeHeightDirectionScrewHoles(
                edgeLength: height - tk_Height,
                xPosition: MaterialThickness34 / 2,
                bottomY: tk_Height,
                flushFace: TenonFlushFace.Back,
                s: settings,
                forceTwoTenons: backThickness == 0.25); // Maps original thickness conditional to truth calculator

            foreach (var h in backHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
            }
        }

        // Hinge holes (inside face)
        if (baseCab.DrillHingeHoles && baseCab.Style != CabinetStyles.Base.Drawer)
        {
            var hingeBores = HingeHolesCalculator.Compute(baseCab, dim, MaterialThickness34);
            foreach (var h in hingeBores)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, 0, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, MaterialThickness34, holeDepth, holeDiameter));
            }
        }

        // Shelf pin holes (inside face)
        if (baseCab.DrillShelfHoles && baseCab.Style != CabinetStyles.Base.Drawer)
        {
            var shelfHoles = ShelfHoleCalculator.ComputeShelfHoles(baseCab, dim);
            foreach (var h in shelfHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, 0, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, MaterialThickness34, holeDepth, holeDiameter));
            }
        }

        // Drawer slide holes (inside face)
        if (baseCab.DrwCount > 0)
        {
            var slideHoles = DrawerSlideHolesCalculator.Compute(baseCab, dim);
            foreach (var h in slideHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, 0, holeDepth, holeDiameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.X, h.Y, MaterialThickness34, holeDepth, holeDiameter));
            }
        }

        // Toekick holes (height-direction, flush Back face)
        if (dim.TKHeight > 0 && dim.TKDepth > 0)
        {
            // Create component-specific settings as specified in MortiseSpecBuilder
            var toekickSettings = settings with
            {
                BlindStart = 0,
                BlindStop = 0,
                TenonThickness = MaterialThickness34 * 0.4 // 0.3" comb profile
            };

            var toekickHoles = PartOutlineBuilder.ComputeHeightDirectionScrewHoles(
                edgeLength: dim.TKHeight - 0.5,
                xPosition: depth - dim.TKDepth - MaterialThickness34, // flush with back face
                bottomY: 0.5,
                flushFace: TenonFlushFace.Back,
                s: toekickSettings,
                forceTwoTenons: true); // Structural consistency per truth calculator

            foreach (var h in toekickHoles)
            {
                leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
            }
        }

        // Drawer Stretcher holes Standard Base with 1 Drawer (depth-direction, flush Bottom face)
        if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount > 0)
        {
            // Component-specific settings per truth calculator
            var drwStretcherSettings = settings with
            {
                TenonThickness = MaterialThickness34, // 3/4" structural reinforcement
                BlindStart = 1.25,
                BlindStop = 1.25
            };

            double[] openings = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
            double runningY = height - (2 * MaterialThickness34); // Start below top panel
            double stretcherWidth = 6.0;

            // Standard cabinet with single drawer: one stretcher below opening 1
            if (baseCab.Style == CabinetStyles.Base.Standard && baseCab.DrwCount == 1)
            {
                runningY -= openings[0];
                var stretcherHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                    partDepth: stretcherWidth,
                    mortiseBottomY: runningY,
                    flushFace: TenonFlushFace.Bottom,
                    s: drwStretcherSettings,
                    xOffset: depth - stretcherWidth,
                    forceTwoTenons: true);

                foreach (var h in stretcherHoles)
                {
                    leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
                }
            }
        }


        // Drawer Stretcher holes Drawer Base with > 1 Drawer (depth-direction, flush Bottom face)
        if (baseCab.Style == CabinetStyles.Base.Drawer && baseCab.DrwCount > 1)
        {
            // Component-specific settings per truth calculator
            var drwStretcherSettings = settings with
            {
                TenonThickness = MaterialThickness34, // 3/4" structural reinforcement
                BlindStart = 1.25,
                BlindStop = 1.25
            };

            double[] openings = [dim.Opening1Height, dim.Opening2Height, dim.Opening3Height, dim.Opening4Height];
            double runningY = height - (2 * MaterialThickness34); // Start Y at top of first drawer opening
            double stretcherWidth = 6.0;

            // Place stretchers below each drawer opening
            for (int i = 0; i < baseCab.DrwCount - 1 && i < openings.Length; i++)
            {
                runningY -= openings[i]; // Advance Y to the bottom of the current drawer opening

                var stretcherHoles = PartOutlineBuilder.ComputeDepthDirectionScrewHoles(
                    partDepth: stretcherWidth,
                    mortiseBottomY: runningY,
                    flushFace: TenonFlushFace.Bottom,
                    s: drwStretcherSettings,
                    xOffset: depth - stretcherWidth, // Aligns stretcher to front edge (X=depth)
                    forceTwoTenons: true); // Truth calculator: stretchers always use exactly 2 tenons

                foreach (var h in stretcherHoles)
                {
                    leftEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, MaterialThickness34, holeDepth, h.Diameter));
                    rightEnd.Children.Add(CabinetPartFactory.CreateHole(h.CenterX, h.CenterY, 0, holeDepth, h.Diameter));
                }

                // CRITICAL: Subtract stretcher thickness so the next iteration starts 
                // at the top of the next drawer opening, matching MortiseSpecBuilder logic.
                runningY -= MaterialThickness34;
            }
        }
    }
}