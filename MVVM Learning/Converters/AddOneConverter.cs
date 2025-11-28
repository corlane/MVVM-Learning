using System;
using System.Globalization;
using System.Windows.Data;

namespace MVVM_Learning.Converters;

public class AddOneConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int i ? (i + 1).ToString() : "1";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}