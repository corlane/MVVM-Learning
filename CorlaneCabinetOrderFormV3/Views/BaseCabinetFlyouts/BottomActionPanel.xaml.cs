using System.Windows;
using System.Windows.Input;

namespace CorlaneCabinetOrderFormV3.Views.BaseCabinetFlyouts;

public partial class BottomActionPanel : FlyoutUserControlBase
{
    public BottomActionPanel() => InitializeComponent();

    private void TextName_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Block the # character as it is typed
        if (e.Text.Contains('#'))
            e.Handled = true;
    }

    private void TextName_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        // Also block pasting text that contains #
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string) e.DataObject.GetData(typeof(string));
            if (text != null && text.Contains('#'))
                e.CancelCommand();
        }
    }
}