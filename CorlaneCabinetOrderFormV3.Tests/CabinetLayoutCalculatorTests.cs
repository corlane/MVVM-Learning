using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

public class CabinetLayoutCalculatorTests
{
    private static CabinetLayoutCalculator.LayoutInputs MakeDrawerInputs(
        int drwCount, double height = 30, double tkHeight = 4, bool hasTK = true,
        double topReveal = 0.125, double bottomReveal = 0.125, double gapWidth = 0.125,
        double o1 = 8, double o2 = 8, double o3 = 8, double o4 = 0,
        double f1 = 0, double f2 = 0, double f3 = 0, double f4 = 0)
        => new(CabinetStyles.Base.Drawer, drwCount, height, tkHeight, hasTK,
               topReveal, bottomReveal, gapWidth, o1, o2, o3, o4, f1, f2, f3, f4);

    [Fact]
    public void ComputeFromOpenings_Drawer2_LastOpeningFillsRemainder()
    {
        var input = MakeDrawerInputs(drwCount: 2, height: 34.5, o1: 10);
        var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

        double effectiveHeight = 34.5 - 4.0; // 30.5
        double deckThickness = (2 + 1) * 0.75; // 2.25
        double expectedO2 = effectiveHeight - deckThickness - 10;

        Assert.Equal(expectedO2, result.Opening2, precision: 4);
    }

    [Fact]
    public void ComputeFromOpenings_Drawer2_FrontHeightsSumToTotal()
    {
        var input = MakeDrawerInputs(drwCount: 2, height: 34.5, o1: 10);
        var result = CabinetLayoutCalculator.ComputeFromOpenings(input);

        double effectiveHeight = 34.5 - 4.0;
        double totalFronts = result.DrwFront1 + result.DrwFront2;
        double totalGaps = input.TopReveal + input.BottomReveal + input.GapWidth;

        Assert.Equal(effectiveHeight, totalFronts + totalGaps, precision: 4);
    }

    [Fact]
    public void ComputeFromDrawerFronts_RoundTrips_BackToOpenings()
    {
        // Start with known openings → get fronts → feed fronts back → should get same openings
        var input = MakeDrawerInputs(drwCount: 3, height: 34.5, o1: 8, o2: 7);
        var fromOpenings = CabinetLayoutCalculator.ComputeFromOpenings(input);

        var reverseInput = input with
        {
            DrwFront1 = fromOpenings.DrwFront1,
            DrwFront2 = fromOpenings.DrwFront2,
            DrwFront3 = fromOpenings.DrwFront3
        };

        var fromFronts = CabinetLayoutCalculator.ComputeFromDrawerFronts(reverseInput);

        Assert.Equal(fromOpenings.Opening1, fromFronts.Opening1, precision: 10);
        Assert.Equal(fromOpenings.Opening2, fromFronts.Opening2, precision: 10);
        Assert.Equal(fromOpenings.Opening3, fromFronts.Opening3, precision: 10);
    }

    [Fact]
    public void ComputeAngleFrontWidth_KnownValues()
    {
        // 24" depths, 36" backs → known triangle
        double width = CabinetLayoutCalculator.ComputeAngleFrontWidth(
            leftDepth: 24, rightDepth: 24, leftBackWidth: 36, rightBackWidth: 36);

        Assert.True(width > 0);
        // p0=(24, 0.75), p1=(35.25, 12) → sqrt(126.5625 + 126.5625) ≈ 15.91
        Assert.Equal(15.9099, width, precision: 3);
    }

    [Fact]
    public void EqualizeAll_MatchesExistingTestFormula()
    {
        double each = CabinetLayoutCalculator.EqualizeAll(30, 0.125, 0.125, 0.125, 2);
        Assert.Equal(14.8125, each, precision: 4);
    }

    [Fact]
    public void EqualizeBottom_MatchesExistingTestFormula()
    {
        double eachBottom = CabinetLayoutCalculator.EqualizeBottom(30, 0.125, 0.125, 0.125, 3, 8);
        Assert.Equal(10.75, eachBottom, precision: 4);
    }
}