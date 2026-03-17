using System.ComponentModel.DataAnnotations;
using CorlaneCabinetOrderFormV3.ValidationAttributes;

namespace CorlaneCabinetOrderFormV3.Tests;

public class DimensionRangeAttributeTests
{
    private class FakeCab
    {
        [DimensionRange(4, 48)]
        public string Width { get; set; } = "";
    }

    private static ValidationResult? Validate(FakeCab cab)
    {
        var ctx = new ValidationContext(cab) { MemberName = nameof(FakeCab.Width) };
        var results = new List<ValidationResult>();
        Validator.TryValidateProperty(cab.Width, ctx, results);
        return results.FirstOrDefault();
    }

    [Theory]
    [InlineData("4")]
    [InlineData("24")]
    [InlineData("48")]
    [InlineData("12 1/2")]
    public void WithinRange_IsValid(string width)
    {
        var cab = new FakeCab { Width = width };
        Assert.Null(Validate(cab));
    }

    [Fact]
    public void BelowMinimum_IsInvalid()
    {
        var cab = new FakeCab { Width = "3" };
        Assert.NotNull(Validate(cab));
    }

    [Fact]
    public void AboveMaximum_IsInvalid()
    {
        var cab = new FakeCab { Width = "49" };
        Assert.NotNull(Validate(cab));
    }

    [Fact]
    public void EmptyString_IsInvalid()
    {
        var cab = new FakeCab { Width = "" };
        Assert.NotNull(Validate(cab));
    }

    [Fact]
    public void GarbageInput_IsInvalid()
    {
        var cab = new FakeCab { Width = "abc" };
        Assert.NotNull(Validate(cab));
    }

    [Fact]
    public void FractionInput_ParsedAndValidated()
    {
        // 3 3/4 = 3.75 → below min of 4 → invalid
        var cab = new FakeCab { Width = "3 3/4" };
        Assert.NotNull(Validate(cab));
    }
}