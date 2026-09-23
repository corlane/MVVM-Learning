using System.Windows;

namespace CorlaneCabinetOrderFormV3.Views;
/// <summary>
/// Interaction logic for CustomMessageBox.xaml
/// </summary>
public partial class CustomMessageBox : Window
{
    public CustomMessageBox(string message)
    {
        InitializeComponent();
        txtMessage.Text = message;
    }

    private void btnYes_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void btnNo_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
