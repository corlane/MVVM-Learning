using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views.BaseCabinetFlyouts;

public class FlyoutUserControlBase : UserControl
{
    protected void TextBoxGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Dispatcher.BeginInvoke(() => textBox.SelectAll());
        }
        e.Handled = true;
    }
}