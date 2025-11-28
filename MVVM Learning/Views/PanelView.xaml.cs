using Microsoft.Extensions.DependencyInjection;
using MVVM_Learning.ViewModels;
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

namespace MVVM_Learning.Views
{
    /// <summary>
    /// Interaction logic for PanelView.xaml
    /// </summary>
    public partial class PanelView : UserControl
    {
        public PanelView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PanelViewModel>();
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
