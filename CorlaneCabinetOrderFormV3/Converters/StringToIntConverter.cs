using System;
using System.Globalization;
using System.Windows.Data;

namespace CorlaneCabinetOrderFormV3.Converters;

public class StringToIntConverter : IValueConverter
{
    // Convert source (int) -> target (string)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i) return i.ToString(culture);
        return string.Empty;
    }

    // ConvertBack target (string) -> source (int)
    // Return 0 for empty/invalid input so validation attributes (Range/Required) can run
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            if (int.TryParse(s.Trim(), NumberStyles.Integer, culture, out var result))
                return result;

            // treat empty or invalid as 0 (Range attribute can mark it invalid)
            return 0;
        }

        return 0;
    }
}