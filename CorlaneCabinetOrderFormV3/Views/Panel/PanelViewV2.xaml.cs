using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views
{
    /// <summary>
    /// Interaction logic for PanelViewV2.xaml
    /// </summary>
    public partial class PanelViewV2 : UserControl
    {
        public PanelViewV2()
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