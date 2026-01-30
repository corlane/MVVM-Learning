using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

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

        geometryModel.Transform = transformGroup;
    }
}