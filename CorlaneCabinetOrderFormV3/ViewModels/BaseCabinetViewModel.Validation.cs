namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : IValidatableViewModel
{
    // Expose the protected ValidateAllProperties for external requests
    public void RunValidation() => ValidateAllProperties();

    // Validate only the properties that are currently visible on the form.
    // Add/remove properties here to match your XAML visibility rules.
    public void RunValidationVisible()
    {
        // Clear any previous errors so only current visible fields show errors.
        ClearErrors();

        try
        {
            ValidateProperty(LeftReveal, nameof(LeftReveal));
            ValidateProperty(RightReveal, nameof(RightReveal));
            ValidateProperty(TopReveal, nameof(TopReveal));
            ValidateProperty(BottomReveal, nameof(BottomReveal));
            ValidateProperty(GapWidth, nameof(GapWidth));

            // General fields shown on most layouts
            if (StdOrDrwBaseVisibility)
            {
                ValidateProperty(Width, nameof(Width));
                ValidateProperty(Height, nameof(Height));
                ValidateProperty(Depth, nameof(Depth));
                ValidateProperty(Species, nameof(Species));
                ValidateProperty(Qty, nameof(Qty));
            }

            // Corner-90 specific fields
            if (Corner90Visibility)
            {
                ValidateProperty(LeftFrontWidth, nameof(LeftFrontWidth));
                ValidateProperty(RightFrontWidth, nameof(RightFrontWidth));
                ValidateProperty(LeftDepth, nameof(LeftDepth));
                ValidateProperty(RightDepth, nameof(RightDepth));
            }

            // 45° (angle-front) specific fields
            if (Corner45Visibility)
            {
                ValidateProperty(LeftDepth, nameof(LeftDepth));
                ValidateProperty(RightDepth, nameof(RightDepth));
                ValidateProperty(LeftBackWidth, nameof(LeftBackWidth));
                ValidateProperty(RightBackWidth, nameof(RightBackWidth));
            }

            // Toekick
            if (HasTK)
            {
                ValidateProperty(TKHeight, nameof(TKHeight));
                ValidateProperty(TKDepth, nameof(TKDepth));
            }

            // Drawer / openings validation (only validate openings that are visible)
            if (GroupDrawersVisibility)
            {
                ValidateProperty(DrwCount, nameof(DrwCount));

                if (Opening1Visible) ValidateProperty(OpeningHeight1, nameof(OpeningHeight1));
                if (Opening2Visible) ValidateProperty(OpeningHeight2, nameof(OpeningHeight2));
                if (Opening3Visible) ValidateProperty(OpeningHeight3, nameof(OpeningHeight3));
                if (Opening4Visible) ValidateProperty(OpeningHeight4, nameof(OpeningHeight4));

                // Drawer front heights (only when those front inputs are enabled/visible)
                if (DrwFront1Visible && !DrwFront1Disabled) ValidateProperty(DrwFrontHeight1, nameof(DrwFrontHeight1));
                if (DrwFront2Visible && !DrwFront2Disabled) ValidateProperty(DrwFrontHeight2, nameof(DrwFrontHeight2));
                if (DrwFront3Visible && !DrwFront3Disabled) ValidateProperty(DrwFrontHeight3, nameof(DrwFrontHeight3));
                if (DrwFront4Visible) ValidateProperty(DrwFrontHeight4, nameof(DrwFrontHeight4));
            }

            // Door-related visible fields
            if (GroupDoorsVisibility)
            {
                ValidateProperty(DoorCount, nameof(DoorCount));
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