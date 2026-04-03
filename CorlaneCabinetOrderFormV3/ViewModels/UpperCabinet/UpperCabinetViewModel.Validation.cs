namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : IValidatableViewModel
{
    public void RunValidation() => ValidateAllProperties();

    public void RunValidationVisible()
    {
        // Clear any previous errors so only current visible fields show errors.
        ClearErrors();

        try
        {
            // General fields shown on most layouts
            if (StandardDimsVisibility)
            {
                ValidateProperty(Width, nameof(Width));
                ValidateProperty(Height, nameof(Height));
                ValidateProperty(Depth, nameof(Depth));
                ValidateProperty(Species, nameof(Species));
                ValidateProperty(Qty, nameof(Qty));
            }

            // Corner-90 specific fields
            if (Corner90DimsVisibility)
            {
                ValidateProperty(LeftFrontWidth, nameof(LeftFrontWidth));
                ValidateProperty(RightFrontWidth, nameof(RightFrontWidth));
                ValidateProperty(LeftDepth, nameof(LeftDepth));
                ValidateProperty(RightDepth, nameof(RightDepth));
            }

            // 45° (angle-front) specific fields
            if (Corner45DimsVisibility)
            {
                ValidateProperty(LeftDepth, nameof(LeftDepth));
                ValidateProperty(RightDepth, nameof(RightDepth));
                ValidateProperty(LeftBackWidth, nameof(LeftBackWidth));
                ValidateProperty(RightBackWidth, nameof(RightBackWidth));
            }

            // Back
            if (BackThicknessVisible)
            {
                ValidateProperty(BackThickness, nameof(BackThickness));
            }



            // Any always-visible fields you want validated every time:
            ValidateProperty(Name, nameof(Name));
        }
        catch (Exception)
        {
            // Swallow: validation should be best-effort and not crash UI
        }
    }

}