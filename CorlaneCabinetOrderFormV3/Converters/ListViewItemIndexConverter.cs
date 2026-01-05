using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CorlaneCabinetOrderFormV3.Converters;

public sealed class ListViewItemIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ListViewItem item)
            return "1";

        var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
        if (listView == null)
            return "1";

        int index = listView.ItemContainerGenerator.IndexFromContainer(item);
        return (index + 1).ToString(culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}