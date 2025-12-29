namespace CorlaneCabinetOrderFormV3.ViewModels;

/// <summary>
/// Simple interface allowing MainWindowViewModel to request validation from tab viewmodels.
/// Implemented by viewmodels that derive from ObservableValidator so they can call ValidateAllProperties().
/// </summary>
public interface IValidatableViewModel
{
    /// <summary>
    /// Run full validation for the viewmodel (wraps protected ValidateAllProperties()).
    /// </summary>
    void RunValidation();

    // New: validate only fields that are currently visible in the form
    void RunValidationVisible();
}