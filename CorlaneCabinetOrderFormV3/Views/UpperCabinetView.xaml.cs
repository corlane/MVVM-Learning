using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class UpperCabinetView : UserControl
{
    public UpperCabinetView()
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