using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Additional edge case tests for ConvertDimension: in-progress fractions,
/// whitespace, large values.
/// </summary>
public class ConvertDimensionEdgeCaseTests
{
    //############################################################################################################
    // In-progress fraction states (must not crash per copilot-instructions)
    //############################################################################################################

    [Theory]
    [InlineData("1/")]      // partial fraction, no denominator
    [InlineData("1 1/")]    // mixed number, partial fraction
    [InlineData("/4")]       // no numerator
    [InlineData("1 /4")]    // space before slash
    public void FractionToDouble_InProgressInput_DoesNotThrow(string input)
    {
        var ex = Record.Exception(() => ConvertDimension.FractionToDouble(input));
        Assert.Null(ex);
    }

    //############################################################################################################
    // Whitespace handling
    //############################################################################################################

    [Theory]
    [InlineData(" 12 ", 12.0)]
    [InlineData("  12.5  ", 12.5)]
    public void FractionToDouble_WhitespacePadded_ParsesCorrectly(string input, double expected)
    {
        Assert.Equal(expected, ConvertDimension.FractionToDouble(input), precision: 10);
    }

    //############################################################################################################
    // Large values
    //############################################################################################################

    [Fact]
    public void FractionToDouble_LargeWholeNumber_ParsesCorrectly()
    {
        Assert.Equal(120.0, ConvertDimension.FractionToDouble("120"), precision: 10);
    }

    [Fact]
    public void FractionToDouble_LargeMixedNumber_ParsesCorrectly()
    {
        Assert.Equal(120.75, ConvertDimension.FractionToDouble("120 3/4"), precision: 10);
    }

    //############################################################################################################
    // DoubleToFraction edge cases
    //############################################################################################################

    [Fact]
    public void DoubleToFraction_VerySmallFraction_TruncatesToNearest32nd()
    {
        // 1/64 = 0.015625 → rounds down to 0/32 → returns "0"
        Assert.Equal("0", ConvertDimension.DoubleToFraction(0.015625));
    }

    [Theory]
    [InlineData(0.03125, "1/32")]   // exactly 1/32
    [InlineData(0.09375, "3/32")]   // exactly 3/32
    [InlineData(0.15625, "5/32")]   // exactly 5/32
    public void DoubleToFraction_ThirtySecondths_ArePreserved(double input, string expected)
    {
        Assert.Equal(expected, ConvertDimension.DoubleToFraction(input));
    }

    //############################################################################################################
    // Round-trip edge cases
    //############################################################################################################

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    [InlineData(-0.5)]
    public void RoundTrip_AdditionalEdgeCases_PreservesValue(double original)
    {
        string fraction = ConvertDimension.DoubleToFraction(original);
        double backToDouble = ConvertDimension.FractionToDouble(fraction);
        Assert.Equal(original, backToDouble, precision: 4);
    }
}