using CorlaneCabOrderFormV3.Converters;
using System.ComponentModel.DataAnnotations;
using CorlaneCabOrderFormV3.Model;

namespace CorlaneCabOrderFormV3.Attributes;

public class ToekickDepthRangeAttribute : ValidationAttribute
{
    double minValue;
    double maxValue;

    public override bool IsValid(object? value)
    {
        minValue = 0;
        maxValue = ConvertDimension.FractionToDouble(CurrentCab.Depth!) - 3;

        string valueString = value!.ToString()!;
        double valueDouble = ConvertDimension.FractionToDouble(valueString);

        // Retrieve min and max from your static class


        if (valueDouble is double intValue)
        {
            return intValue >= minValue && intValue <= maxValue;
        }
        return false; // Or handle other types if needed
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format("Valid Range {1} to {2}.", name, minValue, maxValue);
    }

}
