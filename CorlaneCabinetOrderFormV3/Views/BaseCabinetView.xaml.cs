using Microsoft.Extensions.DependencyInjection;
using CorlaneCabinetOrderFormV3.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class BaseCabinetView : UserControl
{
    public BaseCabinetView()
    {
        InitializeComponent();
        //DataContext = App.ServiceProvider.GetRequiredService<BaseCabinetViewModel>();
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