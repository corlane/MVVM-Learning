using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CorlaneCabinetOrderFormV3.Views
{
    public partial class PlaceOrderView : UserControl
    {
        public PlaceOrderView()
        {
            InitializeComponent();
            //DataContext = App.ServiceProvider.GetRequiredService<PlaceOrderViewModel>();
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
}
