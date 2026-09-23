using System.Windows;

namespace CorlaneCabinetOrderFormV3.Views;
/// <summary>
/// Interaction logic for CustomMessageBox.xaml
/// </summary>
public partial class CustomMessageBoxWithCancel : Window
{
    // Custom result: true = Yes, false = No, null = Cancel
    public bool? Result { get; private set; } = null;

    public CustomMessageBoxWithCancel(string message)
    {
        InitializeComponent();
        txtMessage.Text = message;
    }

    private void btnYes_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void btnNo_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }
}
