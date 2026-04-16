using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views
{
    /// <summary>
    /// Interaction logic for DefaultSettingsViewV2.xaml
    /// </summary>
    public partial class DefaultSettingsViewV2 : UserControl
    {
        public DefaultSettingsViewV2()
        {
            InitializeComponent();
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.SelectAll();
        }
    }
}