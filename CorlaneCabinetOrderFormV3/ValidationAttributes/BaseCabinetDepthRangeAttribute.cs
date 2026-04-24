using CorlaneCabinetOrderFormV3.Converters;
using System.ComponentModel.DataAnnotations;

namespace CorlaneCabinetOrderFormV3.ValidationAttributes;

// BaseCabinetDepthRangeAttribute.cs
// Custom DataAnnotations validation attribute for the Depth field on base cabinet
// view models. Accepts a fraction or decimal dimension string and validates it
// falls within an allowed range. The maximum is supplied at the attribute
// declaration site. The minimum is dynamic: if the cabinet has a toe kick (HasTK
// = true), the minimum is 6.5" plus the toe kick depth (TKDepth); otherwise the
// minimum is a fixed 4.0". Both HasTK and TKDepth are read from the validated
// object at runtime via reflection so the attribute remains self-contained.
// Invalid dimension strings (unparseable by ConvertDimension) are rejected with
// a format error. Empty/null values are allowed through so [Required] can handle
// them separately without producing duplicate validation messages.

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class BaseCabinetDepthRangeAttribute(double maximum) : ValidationAttribute
{
    public double Maximum { get; } = maximum;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Let [Required] handle empty values (avoids duplicate errors).
        if (value is not string depthText || string.IsNullOrWhiteSpace(depthText))
            return ValidationResult.Success;

        double depth = ConvertDimension.FractionToDouble(depthText);

        // ConvertDimension returns 0 on invalid input; treat as invalid unless user literally entered 0.
        if (depth <= 0 && depthText != "0" && depthText != "0/1")
            return new ValidationResult("Invalid dimension format");

        bool hasTk = false;
        string tkDepthText = "0";

        try
        {
            var instance = validationContext.ObjectInstance;
            if (instance is not null)
            {
                var hasTkProp = validationContext.ObjectType.GetProperty("HasTK");
                if (hasTkProp?.PropertyType == typeof(bool))
                    hasTk = (bool)(hasTkProp.GetValue(instance) ?? false);

                var tkDepthProp = validationContext.ObjectType.GetProperty("TKDepth");
                if (tkDepthProp?.PropertyType == typeof(string))
                    tkDepthText = (string?)tkDepthProp.GetValue(instance) ?? "0";
            }
        }
        catch
        {
            // If reflection fails, fall back to the non-TK minimum.
            hasTk = false;
            tkDepthText = "0";
        }

        double tkDepth = ConvertDimension.FractionToDouble(tkDepthText);
        if (tkDepth < 0) tkDepth = 0;

        double min = hasTk ? (6.5 + tkDepth) : 4.0;

        if (depth < min || depth > Maximum)
        {
            return new ValidationResult(
                $"{validationContext.DisplayName} range: {min:0.####}\" to {Maximum:0.####}\"");
        }

        return ValidationResult.Success;
    }
}