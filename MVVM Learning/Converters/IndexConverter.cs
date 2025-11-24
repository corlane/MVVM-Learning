using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MVVM_Learning.Converters;

public class IndexConverter : IValueConverter
{
    // This assigns the cabinet numbers in the listview


    public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
    {
        ListViewItem item = (ListViewItem)value;
        ListView? listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
        int index = listView!.ItemContainerGenerator.IndexFromContainer(item) + 1;
        return index.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
