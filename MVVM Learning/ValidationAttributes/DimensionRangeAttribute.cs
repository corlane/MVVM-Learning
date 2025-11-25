using CommunityToolkit.Mvvm.ComponentModel;
using MVVM_Learning.Converters;  // Import for your ConvertDimension class
using System.ComponentModel.DataAnnotations;

namespace MVVM_Learning.ValidationAttributes;

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