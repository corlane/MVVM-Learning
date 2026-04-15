using Microsoft.Extensions.DependencyInjection;
using CorlaneCabinetOrderFormV3.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class BaseCabinetViewV2 : UserControl
{
    public BaseCabinetViewV2()
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