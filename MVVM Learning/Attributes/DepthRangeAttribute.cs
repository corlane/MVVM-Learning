using CorlaneCabOrderFormV3.Converters;
using System.ComponentModel.DataAnnotations;
using CorlaneCabOrderFormV3.ViewModel;
using CorlaneCabOrderFormV3.Model;

namespace CorlaneCabOrderFormV3.Attributes;

public class DepthRangeAttribute : ValidationAttribute
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
                    minValue = 12;
                    maxValue = 47;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeBase2)
                {
                    minValue = 12;
                    maxValue = 47;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeUpper1)
                {
                    minValue = 6;
                    maxValue = 47;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypePanel)
                {
                    minValue = .25;
                    maxValue = .75;
                }

                if (CurrentCab.CabinetType == CabTypeStrings.CabTypeFiller)
                {
                    minValue = 3;
                    maxValue = 47;
                }
            }
            //value = Convert.ToDouble(value);
            string valueString = value!.ToString()!;
            double valueDouble = ConvertDimension.FractionToDouble(valueString);

            // Retrieve min and max from your static class


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
