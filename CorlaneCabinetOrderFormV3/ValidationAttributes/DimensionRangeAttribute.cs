using CorlaneCabinetOrderFormV3.Converters;  // Import for your ConvertDimension class
using System.ComponentModel.DataAnnotations;

namespace CorlaneCabinetOrderFormV3.ValidationAttributes;

// DimensionRangeAttribute.cs
// General-purpose DataAnnotations validation attribute for cabinet dimension
// fields (width, height, depth, etc.) that are entered as fraction or decimal
// strings (e.g., "18", "18 1/2", "18.5"). Both the minimum and maximum allowed
// values are supplied at the attribute declaration site in inches. The attribute
// uses ConvertDimension.FractionToDouble to parse the input, rejects unparseable
// strings with a format error, and rejects values outside the declared range with
// a descriptive message that includes the display name and the allowed bounds.
// Unlike BaseCabinetDepthRangeAttribute, the bounds here are static (no runtime
// reflection); this attribute is the standard choice for straightforward fixed-
// range dimension validation across base, upper, and other cabinet view models.

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class DimensionRangeAttribute(double minimum, double maximum) : ValidationAttribute
{
    public double Minimum { get; } = minimum;
    public double Maximum { get; } = maximum;
    //public string Unit { get; } = unit;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string strValue || string.IsNullOrWhiteSpace(strValue))
        {
            return new ValidationResult("Dimension is required");
        }

        double parsedValue = ConvertDimension.FractionToDouble(strValue);  // Use your existing converter

        // Check for parse failure: Your method returns 0 on invalid input, but if input is "0" it's valid.
        // To detect true failure, we can add a simple check (assuming dimensions can't be 0 or negative typically)
        if (parsedValue <= 0 && strValue != "0" && strValue != "0/1")  // Adjust based on your domain (e.g., dimensions >0)
        {
            return new ValidationResult("Invalid dimension format");
        }

        if (parsedValue < Minimum || parsedValue > Maximum)
        {
            return new ValidationResult($"{validationContext.DisplayName} range: {Minimum}\" to {Maximum}\"");
        }

        return ValidationResult.Success;
    }
}