using System.ComponentModel.DataAnnotations;
using CorlaneCabinetOrderFormV3.ValidationAttributes;

namespace CorlaneCabinetOrderFormV3.Tests;

public class BaseCabinetDepthValidationTests
{
    // Simple stand-in so the validator can reflect on HasTK / TKDepth
    private class FakeBaseCab
    {
        [BaseCabinetDepthRange(36)]
        public string Depth { get; set; } = "";
        public bool HasTK { get; set; }
        public string TKDepth { get; set; } = "0";
    }

    private static ValidationResult? Validate(FakeBaseCab cab)
    {
        var ctx = new ValidationContext(cab) { MemberName = nameof(FakeBaseCab.Depth) };
        var results = new List<ValidationResult>();
        Validator.TryValidateProperty(cab.Depth, ctx, results);
        return results.FirstOrDefault();
    }

    [Fact]
    public void NoToeKick_DepthOf4_IsValid()
    {
        var cab = new FakeBaseCab { Depth = "4", HasTK = false };
        Assert.Null(Validate(cab));
    }

    [Fact]
    public void NoToeKick_DepthBelow4_IsInvalid()
    {
        var cab = new FakeBaseCab { Depth = "3", HasTK = false };
        Assert.NotNull(Validate(cab));
    }

    [Fact]
    public void WithToeKick_DepthMustExceedTKPlusMinimum()
    {
        // TKDepth 4 → min = 4 + 6.5 = 10.5
        var cab = new FakeBaseCab { Depth = "10", HasTK = true, TKDepth = "4" };
        Assert.NotNull(Validate(cab)); // 10 < 10.5 → invalid

        cab.Depth = "11";
        Assert.Null(Validate(cab)); // 11 > 10.5 → valid
    }

    [Fact]
    public void DepthAboveMaximum_IsInvalid()
    {
        var cab = new FakeBaseCab { Depth = "37", HasTK = false };
        Assert.NotNull(Validate(cab));
    }
}