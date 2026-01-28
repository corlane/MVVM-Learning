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


    private void DrwFrontHeight1_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is not BaseCabinetViewModel vm)
            return;

        e.Handled = true;

        // Commit any pending text to the binding source before recalculating.
        if (sender is TextBox tb)
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

        // Now do the recalculation once, after editing is complete.
        // ResizeDrwFrontHeights is private, so we trigger it by calling the same public-side effects:
        // The simplest is to raise a property change by reassigning the value to itself.
        // (This will invoke OnDrwFrontHeight1Changed once with a stable string.)
        vm.DrwFrontHeight1 = vm.DrwFrontHeight1;
    }
}