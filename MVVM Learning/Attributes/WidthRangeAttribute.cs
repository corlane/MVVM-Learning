using CorlaneCabOrderFormV3.Converters;
using CorlaneCabOrderFormV3.Model;
using System.ComponentModel.DataAnnotations;


namespace CorlaneCabOrderFormV3.Attributes;

public class WidthRangeAttribute : ValidationAttribute
{
    double minValue;
    double maxValue;

    public override bool IsValid(object? value)
    {
        if (value != null)
        {
            if (CurrentCab.CabinetType is not null)
            {
                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeBase1)
                {
                    minValue = 8;
                    maxValue = 95;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeBase2)
                {
                    minValue = 10;
                    maxValue = 95;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeUpper1)
                {
                    minValue = 8;
                    maxValue = 95;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypePanel)
                {
                    minValue = 3;
                    maxValue = 47;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeFiller)
                {
                    minValue = 3;
                    maxValue = 47;
                }

                if (CurrentCab.CabinetType.Contains("Corner"))
                {
                    minValue = 8;
                    maxValue = 95;
                }
            }
            string valueString = value!.ToString()!;
            double valueDouble = ConvertDimension.FractionToDouble(valueString);

            if (valueDouble is double intValue)
            {
                return intValue >= minValue && intValue <= maxValue;
            }
            return false; // Or handle other types if needed
        }
        return false; // Or handle other types if needed

    }

    // Optional: Format the error message to include the actual range.
    public override string FormatErrorMessage(string name)
    {
        return string.Format("Valid Range {1} to {2}.", name, minValue, maxValue);
    }

}
