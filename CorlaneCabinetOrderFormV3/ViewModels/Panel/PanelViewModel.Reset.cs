using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PanelViewModel : ObservableValidator
{
    /// <summary>
    /// Resets the VM to fresh "new job" state — same as what the constructor does.
    /// Called by NewJob() since DI singletons can't be re-constructed.
    /// </summary>
    public void ResetToNewJob()
    {
        _isMapping = true;
        try
        {
            Width = "16";
            Height = "32";
            Depth = "0.75";
            Notes = "";
        }
        finally
        {
            _isMapping = false;
        }

        LoadDefaults();
        UpdatePreview();
    }
}