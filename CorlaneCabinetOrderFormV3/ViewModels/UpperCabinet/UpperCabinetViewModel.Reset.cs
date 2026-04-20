using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
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
            Style = Style1;
            Width = "16";
            Height = "42";
            Depth = "12";
            LeftFrontWidth = "12";
            RightFrontWidth = "12";
            LeftDepth = "12";
            RightDepth = "12";
            LeftBackWidth = "24";
            RightBackWidth = "24";
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