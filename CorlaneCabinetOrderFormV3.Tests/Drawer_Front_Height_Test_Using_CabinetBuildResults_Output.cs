using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
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
    }
}