using Microsoft.Extensions.DependencyInjection;
using CorlaneCabinetOrderFormV3.ViewModels;
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
    /// <summary>
    /// Interaction logic for FillerView.xaml
    /// </summary>
    public partial class FillerView : UserControl
    {
        public FillerView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<FillerViewModel>();
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
