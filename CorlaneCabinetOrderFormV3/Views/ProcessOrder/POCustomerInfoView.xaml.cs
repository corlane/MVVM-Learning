using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views
{
    /// <summary>
    /// Interaction logic for POCustomerInfoView.xaml
    /// </summary>
    public partial class POCustomerInfoView : UserControl
    {
        public POCustomerInfoView()
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
}
