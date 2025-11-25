using CorlaneCabOrderFormV3.Converters;
using CorlaneCabOrderFormV3.Model;
using System.ComponentModel.DataAnnotations;


namespace CorlaneCabOrderFormV3.Attributes;

public class ToekickHeightRangeAttribute : ValidationAttribute
{

    double minValue;
    double maxValue;

    public override bool IsValid(object? value)
    {
        minValue = 2;
        maxValue = ConvertDimension.FractionToDouble(CurrentCab.Height!) - 8;
        if (value != null)
        {
            string valueString = value!.ToString()!;
            double valueDouble = ConvertDimension.FractionToDouble(valueString);

            // Retrieve min and max from your static class
            if (valueDouble is double intValue)
            {
                return intValue >= minValue && intValue <= maxValue;
            }


        }
        return false; // Or handle other types if needed

    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format("Valid Range {1} to {2}.", name, minValue, maxValue);
    }

}
