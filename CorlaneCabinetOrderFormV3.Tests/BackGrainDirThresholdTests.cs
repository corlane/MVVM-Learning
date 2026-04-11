using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Tests the back grain direction threshold logic (3.0.1.47):
/// Standard base/upper cabinets wider than the threshold need horizontal back grain.
/// 3/4" back threshold: 49.25"  (47.75 + 2 × 0.75)
/// 1/4" back threshold: 47.75"
/// These thresholds match POBackGrainDirViewModel constants.
/// </summary>
public class BackGrainDirThresholdTests
{
    private const double ThreeQuarterThreshold = 49.25;
    private const double QuarterThreshold = 47.75;

    //############################################################################################################
    // 3/4" back — at and around the threshold
    //############################################################################################################

    [Theory]
    [InlineData("49", false)]      // below threshold → no change needed
    [InlineData("49.25", false)]   // at threshold → no change needed (> not >=)
    [InlineData("49.5", true)]     // above threshold → needs horizontal grain
    [InlineData("60", true)]       // well above → needs horizontal grain
    public void ThreeQuarterBack_FlagsCorrectly(string width, bool shouldFlag)
    {
        double widthIn = ConvertDimension.FractionToDouble(width);
        bool needsChange = NeedsBackGrainChange(widthIn, isThreeQuarterBack: true);

        Assert.Equal(shouldFlag, needsChange);
    }

    //############################################################################################################
    // 1/4" back — at and around the threshold
    //############################################################################################################

    [Theory]
    [InlineData("47.5", false)]    // below threshold
    [InlineData("47.75", false)]   // at threshold
    [InlineData("48", true)]       // above threshold
    [InlineData("60", true)]       // well above
    public void QuarterBack_FlagsCorrectly(string width, bool shouldFlag)
    {
        double widthIn = ConvertDimension.FractionToDouble(width);
        bool needsChange = NeedsBackGrainChange(widthIn, isThreeQuarterBack: false);

        Assert.Equal(shouldFlag, needsChange);
    }

    //############################################################################################################
    // Corner/Angle cabinets are NOT applicable (only Standard & Drawer)
    //############################################################################################################

    [Theory]
    [InlineData(CabinetStyles.Base.Corner90)]
    [InlineData(CabinetStyles.Base.AngleFront)]
    public void CornerAndAngleStyles_AreNotApplicable(string style)
    {
        Assert.False(IsBackGrainApplicable(style));
    }

    [Theory]
    [InlineData(CabinetStyles.Base.Standard)]
    [InlineData(CabinetStyles.Base.Drawer)]
    public void StandardAndDrawerStyles_AreApplicable(string style)
    {
        Assert.True(IsBackGrainApplicable(style));
    }

    //############################################################################################################
    // IsThreeQuarterBack recognizes both decimal and fractional representations
    //############################################################################################################

    [Theory]
    [InlineData("0.75", true)]
    [InlineData("3/4", true)]
    [InlineData("0.25", false)]
    [InlineData("1/4", false)]
    public void IsThreeQuarterBack_RecognizesBothFormats(string backThickness, bool expected)
    {
        Assert.Equal(expected, IsThreeQuarterBack(backThickness));
    }

    //############################################################################################################
    // Helpers — mirror POBackGrainDirViewModel logic
    //############################################################################################################

    private static bool NeedsBackGrainChange(double widthIn, bool isThreeQuarterBack)
    {
        double threshold = isThreeQuarterBack ? ThreeQuarterThreshold : QuarterThreshold;
        return widthIn > threshold;
    }

    private static bool IsBackGrainApplicable(string style)
    {
        return style == CabinetStyles.Base.Standard
            || style == CabinetStyles.Base.Drawer;
    }

    private static bool IsThreeQuarterBack(string backThickness)
    {
        return backThickness == CabinetOptions.BackThickness.ThreeQuarterDecimal
            || backThickness == CabinetOptions.BackThickness.ThreeQuarterFraction;
    }
}