using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3.Views
{
    /// <summary>
    /// Interaction logic for FillerViewV2.xaml
    /// </summary>
    public partial class FillerViewV2 : UserControl
    {
        public FillerViewV2()
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