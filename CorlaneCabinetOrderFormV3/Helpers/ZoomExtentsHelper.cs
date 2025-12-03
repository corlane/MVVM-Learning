using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Helpers;

public static class ZoomExtentsHelper
{
    public static readonly DependencyProperty BoundsProperty =
        DependencyProperty.RegisterAttached(
            "Bounds",
            typeof(Rect3D),
            typeof(ZoomExtentsHelper),
            new PropertyMetadata(Rect3D.Empty, OnBoundsChanged));

    public static Rect3D GetBounds(DependencyObject obj) => (Rect3D)obj.GetValue(BoundsProperty);
    public static void SetBounds(DependencyObject obj, Rect3D value) => obj.SetValue(BoundsProperty, value);

    private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HelixViewport3D viewport && e.NewValue is Rect3D bounds && !bounds.IsEmpty)
        {
            //viewport.Camera.Position = new Point3D(width / 2, height / 2, depth * 2);
            viewport.Camera.LookDirection = new Vector3D(0, 0, -1);
            viewport.Camera.UpDirection = new Vector3D(0, 1, 0);
            //viewport.ZoomExtents(newBounds); // Zoom to the new model's bounds

            viewport.ZoomExtents(bounds);

        }
    }
}