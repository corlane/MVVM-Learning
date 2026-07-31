using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests
{
    public class Drawer_Front_Height_Test_Using_CabinetBuildResults_Output
    {
        /// <summary>
        /// Tests that CabinetLayoutCalculator correctly computes drawer front heights
        /// from opening heights, reveals, and gaps for a 4-drawer base cabinet.
        /// No WPF/STA required — pure math.
        /// </summary>

        [Fact]
        public void DrawerFrontHeights_ComputedFromOpenings_4DrawerBase()
        {
            // 34.5" tall, 4" TK, 4 drawers
            // Opening heights: user sets first 3, last is computed from remainder
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 4,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 6.375,
                Opening2: 7.375,
                Opening3: 8.375,
                Opening4: 0,       // will be computed
                DrwFront1: 0,      // will be computed
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

            // Opening 4 = effectiveHeight - deckThickness - o1 - o2 - o3
            // effectiveHeight = 34.5 - 4 = 30.5
            // deckThickness = (4 + 1) * 0.75 = 3.75
            // o4 = 30.5 - 3.75 - 6.375 - 7.375 - 8.375 = 4.625
            Assert.Equal(4.625, result.Opening4, tolerance: 0.001);

            // f1 = o1 + (1.5 * 0.75) - topReveal - (gap / 2)
            //    = 6.375 + 1.125 - 0.4375 - 0.0625 = 7.0
            Assert.Equal(7.0, result.DrwFront1, tolerance: 0.001);

            // f2 = o2 + 0.75 - 0.125 = 7.375 + 0.625 = 8.0
            Assert.Equal(8.0, result.DrwFront2, tolerance: 0.001);

            // f3 = o3 + 0.75 - 0.125 = 8.375 + 0.625 = 9.0
            Assert.Equal(9.0, result.DrwFront3, tolerance: 0.001);

            // f4 = o4 + (1.5 * 0.75) - bottomReveal - (gap / 2)
            //    = 4.625 + 1.125 - 0.0625 - 0.0625 = 5.625
            Assert.Equal(5.625, result.DrwFront4, tolerance: 0.001);
        }

        /// <summary>
        /// Tests the reverse path: given drawer front heights, compute opening heights.
        /// </summary>

        [Fact]
        public void OpeningHeights_ComputedFromDrawerFronts_4DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 4,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,       // will be computed
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 7,      // user-provided
                DrwFront2: 8,
                DrwFront3: 9,
                DrwFront4: 0);     // will be computed (last is always derived)

            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);

            Assert.Equal(6.375, result.Opening1, tolerance: 0.001);
            Assert.Equal(7.375, result.Opening2, tolerance: 0.001);
            Assert.Equal(8.375, result.Opening3, tolerance: 0.001);
            Assert.Equal(4.625, result.Opening4, tolerance: 0.001);
            Assert.Equal(5.625, result.DrwFront4, tolerance: 0.001);
        }

        /// <summary>
        /// Tests that CabinetLayoutCalculator correctly computes the drawer front height
        /// for a 1-drawer base cabinet. Opening1 is fully overridden internally
        /// (effectiveHeight - 2 shelves), and the front spans the full interior
        /// minus top and bottom reveals — no gap logic applies since there is only one front.
        /// </summary>
        [Fact]
        public void DrawerFrontHeights_ComputedFromOpenings_1DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 1,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,       // always overridden internally for 1-drw
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 0,      // will be computed
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

            // effectiveHeight = 34.5 - 4 = 30.5
            // o1 = 30.5 - (2 * 0.75) = 30.5 - 1.5 = 29.0
            Assert.Equal(29.0, result.Opening1, tolerance: 0.001);

            // f1 = o1 + (2 * 0.75) - topReveal - bottomReveal
            //    = 29.0 + 1.5 - 0.4375 - 0.0625 = 30.0
            Assert.Equal(30.0, result.DrwFront1, tolerance: 0.001);
        }

        /// <summary>
        /// Tests that CabinetLayoutCalculator correctly computes drawer front heights
        /// for a 2-drawer base cabinet. Opening2 is computed from the remainder after Opening1.
        /// Top front uses topReveal; bottom front uses bottomReveal. Both use gap/2.
        /// </summary>
        [Fact]
        public void DrawerFrontHeights_ComputedFromOpenings_2DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 2,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 12.0,
                Opening2: 0,       // will be computed
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 0,      // will be computed
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

            // effectiveHeight = 34.5 - 4 = 30.5
            // deckThickness = (2 + 1) * 0.75 = 2.25
            // o2 = 30.5 - 2.25 - 12.0 = 16.25
            Assert.Equal(16.25, result.Opening2, tolerance: 0.001);

            // f1 = o1 + (1.5 * 0.75) - topReveal - (gap / 2)
            //    = 12.0 + 1.125 - 0.4375 - 0.0625 = 12.625
            Assert.Equal(12.625, result.DrwFront1, tolerance: 0.001);

            // f2 = o2 + (1.5 * 0.75) - bottomReveal - (gap / 2)
            //    = 16.25 + 1.125 - 0.0625 - 0.0625 = 17.25
            Assert.Equal(17.25, result.DrwFront2, tolerance: 0.001);
        }

        /// <summary>
        /// Tests that CabinetLayoutCalculator correctly computes drawer front heights
        /// for a 3-drawer base cabinet. Opening3 is computed from the remainder.
        /// The middle front uses a single shelf thickness minus a full gap (no reveal).
        /// Top and bottom fronts use their respective reveals and gap/2.
        /// </summary>
        [Fact]
        public void DrawerFrontHeights_ComputedFromOpenings_3DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 3,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 8.0,
                Opening2: 9.0,
                Opening3: 0,       // will be computed
                Opening4: 0,
                DrwFront1: 0,      // will be computed
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

            // effectiveHeight = 34.5 - 4 = 30.5
            // deckThickness = (3 + 1) * 0.75 = 3.0
            // o3 = 30.5 - 3.0 - 8.0 - 9.0 = 10.5
            Assert.Equal(10.5, result.Opening3, tolerance: 0.001);

            // f1 = o1 + (1.5 * 0.75) - topReveal - (gap / 2)
            //    = 8.0 + 1.125 - 0.4375 - 0.0625 = 8.625
            Assert.Equal(8.625, result.DrwFront1, tolerance: 0.001);

            // f2 = o2 + 0.75 - gap
            //    = 9.0 + 0.75 - 0.125 = 9.625
            Assert.Equal(9.625, result.DrwFront2, tolerance: 0.001);

            // f3 = o3 + (1.5 * 0.75) - bottomReveal - (gap / 2)
            //    = 10.5 + 1.125 - 0.0625 - 0.0625 = 11.5
            Assert.Equal(11.5, result.DrwFront3, tolerance: 0.001);
        }

        /// <summary>
        /// Tests the reverse path for a 1-drawer base cabinet — given a drawer front height,
        /// verify ComputeFromDrawerFronts produces the correct opening and front values.
        /// Note: o1 is always fully overridden regardless of the input DrwFront1 value,
        /// so this path must still yield o1=29.0 and f1=30.0.
        /// </summary>
        [Fact]
        public void OpeningHeights_ComputedFromDrawerFronts_1DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 1,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 30.0,   // known-good value from ComputeFromOpenings test
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);

            // o1 is always overridden: effectiveHeight - (2 * 0.75) = 30.5 - 1.5 = 29.0
            Assert.Equal(29.0, result.Opening1, tolerance: 0.001);

            // f1 = o1 + (2 * 0.75) - topReveal - bottomReveal
            //    = 29.0 + 1.5 - 0.4375 - 0.0625 = 30.0
            Assert.Equal(30.0, result.DrwFront1, tolerance: 0.001);
        }

        /// <summary>
        /// Tests the reverse path for a 2-drawer base cabinet — given drawer front heights,
        /// verify ComputeFromDrawerFronts round-trips back to the same openings produced
        /// by ComputeFromOpenings. Uses the known-good front values from the forward test.
        /// </summary>
        [Fact]
        public void OpeningHeights_ComputedFromDrawerFronts_2DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 2,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 12.625,  // known-good from forward test
                DrwFront2: 0,       // last is always derived
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);

            // o1 = f1 + topReveal + (gap/2) - (1.5 * 0.75)
            //    = 12.625 + 0.4375 + 0.0625 - 1.125 = 12.0
            Assert.Equal(12.0, result.Opening1, tolerance: 0.001);

            // o2 = effectiveHeight - deckThickness - o1 = 30.5 - 2.25 - 12.0 = 16.25
            Assert.Equal(16.25, result.Opening2, tolerance: 0.001);

            // f2 = o2 + (1.5 * 0.75) - bottomReveal - (gap/2)
            //    = 16.25 + 1.125 - 0.0625 - 0.0625 = 17.25
            Assert.Equal(17.25, result.DrwFront2, tolerance: 0.001);
        }

        /// <summary>
        /// Tests the reverse path for a 3-drawer base cabinet — given drawer front heights,
        /// verify ComputeFromDrawerFronts round-trips back to the same openings produced
        /// by ComputeFromOpenings. Uses known-good front values from the forward test.
        /// </summary>
        [Fact]
        public void OpeningHeights_ComputedFromDrawerFronts_3DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Drawer,
                DrwCount: 3,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 8.625,  // known-good from forward test
                DrwFront2: 9.625,
                DrwFront3: 0,      // last is always derived
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);

            // o1 = f1 + topReveal + (gap/2) - (1.5 * 0.75)
            //    = 8.625 + 0.4375 + 0.0625 - 1.125 = 8.0
            Assert.Equal(8.0, result.Opening1, tolerance: 0.001);

            // o2 = f2 + gap - mat = 9.625 + 0.125 - 0.75 = 9.0
            Assert.Equal(9.0, result.Opening2, tolerance: 0.001);

            // o3 = effectiveHeight - deckThickness - o1 - o2 = 30.5 - 3.0 - 8.0 - 9.0 = 10.5
            Assert.Equal(10.5, result.Opening3, tolerance: 0.001);

            // f3 = o3 + (1.5 * 0.75) - bottomReveal - (gap/2)
            //    = 10.5 + 1.125 - 0.0625 - 0.0625 = 11.5
            Assert.Equal(11.5, result.DrwFront3, tolerance: 0.001);
        }

        /// <summary>
        /// Tests that CabinetLayoutCalculator correctly computes the drawer front height
        /// for a Base Standard (1-door-1-drawer) cabinet. The user supplies the drawer
        /// opening height (o1); the front height is derived using the top reveal and half-gap.
        /// Only the drawer portion is computed here — the door opening is handled separately by the VM.
        /// </summary>
        [Fact]
        public void DrawerFrontHeights_ComputedFromOpenings_1Door1DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Standard,
                DrwCount: 1,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 6.0,     // user-supplied drawer opening height
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 0,      // will be computed
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

            // f1 = o1 + (1.5 * 0.75) - topReveal - (gap / 2)
            //    = 6.0 + 1.125 - 0.4375 - 0.0625 = 6.625
            Assert.Equal(6.625, result.DrwFront1, tolerance: 0.001);
        }

        /// <summary>
        /// Tests the reverse path for a Base Standard (1-door-1-drawer) cabinet — given the
        /// drawer front height, verify ComputeFromDrawerFronts derives the correct opening
        /// and round-trips f1 back to the same value.
        /// </summary>
        [Fact]
        public void OpeningHeights_ComputedFromDrawerFronts_1Door1DrawerBase()
        {
            var input = new CabinetLayoutCalculator.LayoutInputs(
                Style: CabinetStyles.Base.Standard,
                DrwCount: 1,
                Height: 34.5,
                TkHeight: 4,
                HasTK: true,
                TopReveal: 0.4375,
                BottomReveal: 0.0625,
                GapWidth: 0.125,
                Opening1: 0,
                Opening2: 0,
                Opening3: 0,
                Opening4: 0,
                DrwFront1: 6.625,  // known-good from forward test
                DrwFront2: 0,
                DrwFront3: 0,
                DrwFront4: 0);

            var result = CabinetLayoutCalculator.ComputeFromDrawerFronts(input);

            // o1 = f1 + topReveal + (gap / 2) - (1.5 * 0.75)
            //    = 6.625 + 0.4375 + 0.0625 - 1.125 = 6.0
            Assert.Equal(6.0, result.Opening1, tolerance: 0.001);

            // f1 round-trips back: o1 + 1.125 - 0.4375 - 0.0625 = 6.625
            Assert.Equal(6.625, result.DrwFront1, tolerance: 0.001);
        }
    }
}