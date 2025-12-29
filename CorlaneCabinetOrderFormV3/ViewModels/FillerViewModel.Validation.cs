namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class FillerViewModel : IValidatableViewModel
{
    public void RunValidation() => ValidateAllProperties();

    // Validate only the properties that are currently visible on the form.
    // Add/remove properties here to match your XAML visibility rules.
    public void RunValidationVisible() => ValidateAllProperties();

}