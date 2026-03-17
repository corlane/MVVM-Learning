using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.Tests;

public class ConvertDimensionTests
{
    // ── FractionToDouble ──────────────────────────────────────────

    [Theory]
    [InlineData("3", 3.0)]
    [InlineData("34.5", 34.5)]
    [InlineData("1/2", 0.5)]
    [InlineData("3/4", 0.75)]
    [InlineData("1 1/2", 1.5)]
    [InlineData("24 3/8", 24.375)]
    [InlineData("5/0", 0)]

    public void FractionToDouble_ValidInput_ReturnsExpected(string input, double expected)
    {
        double result = ConvertDimension.FractionToDouble(input);
        Assert.Equal(expected, result, precision: 5);
    }

    [Fact]
    public void FractionToDouble_NegativeMixedNumber_ReturnsNegative()
    {
        // Bug fixed in 3.0.1.18
        double result = ConvertDimension.FractionToDouble("-1 1/2");
        Assert.Equal(-1.5, result, precision: 5);
    }

    [Fact]
    public void FractionToDouble_ZeroNumerator_DoesNotHang()
    {
        // Bug fixed in 3.0.1.10 — infinite loop when numerator = 0
        double result = ConvertDimension.FractionToDouble("0/4");
        Assert.Equal(0.0, result, precision: 5);
    }

    [Fact]
    public void FractionToDouble_Null_ReturnsZero()
    {
        Assert.Equal(0.0, ConvertDimension.FractionToDouble(null!));
    }

    [Fact]
    public void FractionToDouble_EmptyString_ReturnsZero()
    {
        Assert.Equal(0.0, ConvertDimension.FractionToDouble(""));
    }

    // ── DoubleToFraction ──────────────────────────────────────────

    [Theory]
    [InlineData(3.0, "3")]
    [InlineData(0.5, "1/2")]
    [InlineData(0.75, "3/4")]
    [InlineData(1.5, "1 1/2")]
    [InlineData(24.375, "24 3/8")]
    public void DoubleToFraction_ValidInput_ReturnsExpected(double input, string expected)
    {
        string result = ConvertDimension.DoubleToFraction(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DoubleToFraction_NegativeValue_IncludesSign()
    {
        // Bug fixed in 3.0.1.18
        string result = ConvertDimension.DoubleToFraction(-1.5);
        Assert.Equal("-1 1/2", result);
    }

    [Fact]
    public void DoubleToFraction_Zero_ReturnsZeroString()
    {
        Assert.Equal("0", ConvertDimension.DoubleToFraction(0.0));
    }

    // ── Round-trip ────────────────────────────────────────────────

    [Theory]
    [InlineData("3 1/4")]
    [InlineData("24 3/8")]
    [InlineData("1/2")]
    [InlineData("36")]
    [InlineData("36 23/32")]

    public void RoundTrip_FractionToDoubleAndBack_PreservesValue(string original)
    {
        double asDouble = ConvertDimension.FractionToDouble(original);
        string backToFraction = ConvertDimension.DoubleToFraction(asDouble);
        Assert.Equal(original, backToFraction);
    }

    [Theory]
    [InlineData("5/0")]
    [InlineData("1 3/0")]
    public void FractionToDouble_ZeroDenominator_ReturnsZero(string input)
    {
        Assert.Equal(0.0, ConvertDimension.FractionToDouble(input));
    }
}