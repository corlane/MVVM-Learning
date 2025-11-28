using System.Globalization;
using System.Windows.Data;

namespace CorlaneCabinetOrderFormV3.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class BoolInverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool booleanValue = (bool)value;
        return !booleanValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool booleanValue = (bool)value;
        return !booleanValue;
    }
}
