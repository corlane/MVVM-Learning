namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests the drawer front equalization math used in BaseCabinetViewModel.
/// The formulas are extracted here so the core arithmetic can be verified
/// independently of the full ViewModel and its property-change handlers.
/// </summary>
public class DrawerFrontEqualizationTests
{
    // Mirrors the "Equalize All" formula from ApplyDrawerFrontEqualization
    private static double EqualizeAll(double height, double topReveal, double bottomReveal, double gapWidth, int drwCount)
    {
        double total = height - topReveal - bottomReveal - (gapWidth * (drwCount - 1));
        return total / drwCount;
    }

    // Mirrors the "Equalize Bottom" formula — top drawer is fixed, rest split the remainder
    private static double EqualizeBottom(double height, double topReveal, double bottomReveal, double gapWidth, int drwCount, double topDrawerHeight)
    {
        double total = height - topReveal - bottomReveal - (gapWidth * (drwCount - 1)) - topDrawerHeight;
        return total / (drwCount - 1);
    }

    [Fact]
    public void EqualizeAll_TwoDrawers_SplitsEvenly()
    {
        // 30" cabinet, 1/8 reveals, 1/8 gap
        double each = EqualizeAll(30, 0.125, 0.125, 0.125, 2);

        // total = 30 - 0.125 - 0.125 - (0.125 * 1) = 29.625
        // each  = 29.625 / 2 = 14.8125
        Assert.Equal(14.8125, each, precision: 4);
    }

    [Fact]
    public void EqualizeAll_FourDrawers_SplitsEvenly()
    {
        double each = EqualizeAll(30, 0.125, 0.125, 0.125, 4);

        // total = 30 - 0.125 - 0.125 - (0.125 * 3) = 29.375
        // each  = 29.375 / 4 = 7.34375
        Assert.Equal(7.34375, each, precision: 5);
    }

    [Fact]
    public void EqualizeBottom_ThreeDrawers_TopFixed()
    {
        // Top drawer = 8", rest split remaining space
        double eachBottom = EqualizeBottom(30, 0.125, 0.125, 0.125, 3, 8);

        // total = 30 - 0.125 - 0.125 - (0.125 * 2) - 8 = 21.5
        // each  = 21.5 / 2 = 10.75
        Assert.Equal(10.75, eachBottom, precision: 4);
    }

    [Fact]
    public void EqualizeAll_WithToeKick_UsesReducedHeight()
    {
        // 34.5" cabinet with 4" toekick → effective height = 30.5"
        double height = 34.5 - 4.0;
        double each = EqualizeAll(height, 0.125, 0.125, 0.125, 3);

        // total = 30.5 - 0.125 - 0.125 - (0.125 * 2) = 30.0
        // each  = 30.0 / 3 = 10.0
        Assert.Equal(10.0, each, precision: 4);
    }

    [Fact]
    public void EqualizeAll_AllDrawerHeights_SumBackToTotal()
    {
        int count = 3;
        double topReveal = 0.125, bottomReveal = 0.125, gap = 0.125;
        double height = 30;

        double each = EqualizeAll(height, topReveal, bottomReveal, gap, count);
        double reconstructed = (each * count) + topReveal + bottomReveal + (gap * (count - 1));

        Assert.Equal(height, reconstructed, precision: 10);
    }
}