using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class UpperCabinetViewV2 : UserControl
{
    public UpperCabinetViewV2()
    {
        InitializeComponent();
    }

    private void TextBoxGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Dispatcher.BeginInvoke(() => textBox.SelectAll());
        }

        e.Handled = true;
    }
}