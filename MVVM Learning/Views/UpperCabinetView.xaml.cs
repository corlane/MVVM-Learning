using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MVVM_Learning.Views;

public partial class UpperCabinetView : UserControl
{
    public UpperCabinetView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<UpperCabinetViewModel>();
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