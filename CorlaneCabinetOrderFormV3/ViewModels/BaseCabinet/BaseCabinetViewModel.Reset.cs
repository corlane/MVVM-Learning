using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
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
            Width = "18";
            Height = "34.5";
            Depth = "24";
            LeftFrontWidth = "12";
            RightFrontWidth = "12";
            LeftDepth = "24";
            RightDepth = "24";
            LeftBackWidth = "36";
            RightBackWidth = "36";
            Style = Style1;
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
