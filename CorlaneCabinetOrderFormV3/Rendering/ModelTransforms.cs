using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

// =============================================================================
// ModelTransforms.cs
// CorlaneCabinetOrderFormV3.Rendering
//
// Utility class for applying 3D spatial transforms to Model3DGroup instances
// produced by CabinetPartFactory. Used by all cabinet builders (BaseCabinetBuilder,
// UpperCabinetBuilder, FillerAndPanelBuilder) to position and orient panels after
// they are created in local/origin space.
//
// ApplyTransform builds a Transform3DGroup with the following operation order:
//   1. Optional pivot translation (translate to center point before rotating),
//      allowing rotation around an arbitrary point rather than the world origin.
//   2. X, Y, Z axis-angle rotations (in that order).
//   3. Final translation — either back from the pivot + offset, or a straight
//      translate-then-rotate if no center point is provided (legacy behavior
//      preserved for existing call sites that rely on origin-based rotation).
//
// Notes:
//   - All panels from CabinetPartFactory are built flat at Z=0 in local space;
//     ApplyTransform is what places them into the correct cabinet-relative position.
//   - The optional centerX/Y/Z overload exists specifically for parts that need
//     to rotate around their own midpoint (e.g., doors, flip-up panels) rather
//     than the world origin.
//   - Null-checks geometryModel before assigning the transform, making it safe
//     to call even when a builder conditionally skips creating a part.
// =============================================================================

internal static class ModelTransforms
{
    internal static void ApplyTransform(
        Model3DGroup geometryModel,
        double translateX,
        double translateY,
        double translateZ,
        double rotateXDegrees,
        double rotateYDegrees,
        double rotateZDegrees,
        double? centerX = null,
        double? centerY = null,
        double? centerZ = null)
    {
        var transformGroup = new Transform3DGroup();

        if (centerX.HasValue && centerY.HasValue && centerZ.HasValue)
        {
            transformGroup.Children.Add(new TranslateTransform3D(-centerX.Value, -centerY.Value, -centerZ.Value));

            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotateXDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateYDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotateZDegrees)));

            transformGroup.Children.Add(new TranslateTransform3D(centerX.Value + translateX, centerY.Value + translateY, centerZ.Value + translateZ));
        }
        else
        {
            // Backwards-compatible: previous behavior was translate then rotate around origin.
            transformGroup.Children.Add(new TranslateTransform3D(translateX, translateY, translateZ));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotateXDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateYDegrees)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotateZDegrees)));
        }

        if (geometryModel != null)
        {
            geometryModel.Transform = transformGroup;

        }
    }
}