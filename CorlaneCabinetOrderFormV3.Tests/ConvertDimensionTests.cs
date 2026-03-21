using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.Tests;

public class ConvertDimensionTests
{
    //############################################################################################################
    // FractionToDouble
    //############################################################################################################

    [Theory]
    [InlineData("12", 12.0)]
    [InlineData("12.5", 12.5)]
    [InlineData("0", 0.0)]
    [InlineData("1/2", 0.5)]
    [InlineData("3/4", 0.75)]
    [InlineData("12 1/2", 12.5)]
    [InlineData("34 1/4", 34.25)]
    public void FractionToDouble_ValidInputs(string input, double expected)
    {
        Assert.Equal(expected, ConvertDimension.FractionToDouble(input), precision: 10);
    }

    [Fact]
    public void FractionToDouble_NegativeMixedNumber_IsCorrect()
    {
        // Changelog 3.0.1.18: fixed negative number conversion
        Assert.Equal(-1.5, ConvertDimension.FractionToDouble("-1 1/2"), precision: 10);
    }

    [Fact]
    public void FractionToDouble_ZeroDenominator_ReturnsZero()
    {
        // Changelog 3.0.1.32: fixed divide-by-zero for "5/0"
        Assert.Equal(0.0, ConvertDimension.FractionToDouble("5/0"));
    }

    [Fact]
    public void FractionToDouble_ZeroNumeratorFraction_ReturnsZero()
    {
        // Changelog 3.0.1.10: fixed infinite loop when numerator = 0
        Assert.Equal(0.0, ConvertDimension.FractionToDouble("0/4"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    public void FractionToDouble_InvalidInput_ReturnsZero(string? input)
    {
        Assert.Equal(0.0, ConvertDimension.FractionToDouble(input!));
    }

    //############################################################################################################
    // DoubleToFraction
    //############################################################################################################

    [Theory]
    [InlineData(12.0, "12")]
    [InlineData(0.5, "1/2")]
    [InlineData(0.75, "3/4")]
    [InlineData(12.5, "12 1/2")]
    [InlineData(34.25, "34 1/4")]
    [InlineData(0.0, "0")]
    public void DoubleToFraction_KnownValues(double input, string expected)
    {
        Assert.Equal(expected, ConvertDimension.DoubleToFraction(input));
    }

    [Fact]
    public void DoubleToFraction_NegativeValue_IncludesSign()
    {
        Assert.Equal("-12 1/2", ConvertDimension.DoubleToFraction(-12.5));
    }

    //############################################################################################################
    // Round-trip
    //############################################################################################################

    [Theory]
    [InlineData(0.25)]
    [InlineData(0.75)]
    [InlineData(12.5)]
    [InlineData(34.25)]
    [InlineData(4.0)]
    [InlineData(0.125)]
    [InlineData(-3.75)]
    public void RoundTrip_DoubleToFractionToDouble_PreservesValue(double original)
    {
        string fraction = ConvertDimension.DoubleToFraction(original);
        double backToDouble = ConvertDimension.FractionToDouble(fraction);
        Assert.Equal(original, backToDouble, precision: 4);
    }
}