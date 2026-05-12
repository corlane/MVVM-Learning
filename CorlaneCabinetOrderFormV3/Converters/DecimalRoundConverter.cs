using System;
using System.Globalization;
using System.Windows.Data; // Or Microsoft.UI.Xaml.Data for WinUI/MAUI

namespace CorlaneCabinetOrderFormV3.Converters;

/// <summary>
/// This converter rounds a double value to two decimal places when converting to a string, and parses a string back to a double, rounding it to two decimal places as well.
/// It is used in the POBatchListViewModel Material Thickness textboxes to ensure that the displayed value is always rounded to two decimal places, and that any user input is also rounded appropriately when converted back to a double.
/// </summary>

public class DecimalRoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d.ToString("F2", CultureInfo.InvariantCulture);
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return Math.Round(result, 2, MidpointRounding.AwayFromZero);
        }
        return value;
    }
}