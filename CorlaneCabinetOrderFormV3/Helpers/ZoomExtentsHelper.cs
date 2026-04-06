using HelixToolkit.Wpf;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Helpers;

public static class ZoomExtentsHelper
{
    private static readonly ConditionalWeakTable<HelixViewport3D, LastAppliedBoundsHolder> _lastAppliedBounds = new();

    public static readonly DependencyProperty BoundsProperty =
        DependencyProperty.RegisterAttached(
            "Bounds",
            typeof(Rect3D),
            typeof(ZoomExtentsHelper),
            new PropertyMetadata(Rect3D.Empty, OnBoundsChanged));

    public static Rect3D GetBounds(DependencyObject obj) => (Rect3D)obj.GetValue(BoundsProperty);
    public static void SetBounds(DependencyObject obj, Rect3D value) => obj.SetValue(BoundsProperty, value);

    public static readonly DependencyProperty BoundsChangeToleranceProperty =
        DependencyProperty.RegisterAttached(
            "BoundsChangeTolerance",
            typeof(double),
            typeof(ZoomExtentsHelper),
            new PropertyMetadata(0d));

    public static double GetBoundsChangeTolerance(DependencyObject obj) => (double)obj.GetValue(BoundsChangeToleranceProperty);
    public static void SetBoundsChangeTolerance(DependencyObject obj, double value) => obj.SetValue(BoundsChangeToleranceProperty, value);

    public static readonly DependencyProperty ZoomMultiplierProperty =
        DependencyProperty.RegisterAttached(
            "ZoomMultiplier",
            typeof(double),
            typeof(ZoomExtentsHelper),
            new PropertyMetadata(1.0));

    public static double GetZoomMultiplier(DependencyObject obj) => (double)obj.GetValue(ZoomMultiplierProperty);
    public static void SetZoomMultiplier(DependencyObject obj, double value) => obj.SetValue(ZoomMultiplierProperty, value);

    private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HelixViewport3D viewport)
            return;

        if (e.NewValue is not Rect3D newBounds || newBounds.IsEmpty)
            return;

        var tolerance = Math.Max(0d, GetBoundsChangeTolerance(viewport));
        var holder = _lastAppliedBounds.GetOrCreateValue(viewport);

        if (holder.HasValue && tolerance > 0d && !HasSignificantSizeChange(holder.Value, newBounds, tolerance))
            return;

        viewport.Camera.LookDirection = new Vector3D(0, 0, -1);
        viewport.Camera.UpDirection = new Vector3D(0, 1, 0);

        viewport.ZoomExtents(newBounds);

        // After ZoomExtents, apply a zoom multiplier to bring the camera closer
        // for styles whose rotated geometry inflates the bounding box.
        var multiplier = GetZoomMultiplier(viewport);
        if (multiplier > 0d && Math.Abs(multiplier - 1.0) > 0.001)
        {
            var cam = viewport.Camera;
            var lookAt = cam.Position + cam.LookDirection;
            var offset = cam.Position - lookAt;
            var newOffset = offset / multiplier;
            cam.Position = lookAt + newOffset;
            cam.LookDirection = lookAt - cam.Position;
        }

        holder.Value = newBounds;
        holder.HasValue = true;
    }

    private static bool HasSignificantSizeChange(Rect3D oldBounds, Rect3D newBounds, double tolerance)
    {
        // Only compare size deltas (cabinet size changes). Ignores position changes.
        return Math.Abs(newBounds.SizeX - oldBounds.SizeX) > tolerance
            || Math.Abs(newBounds.SizeY - oldBounds.SizeY) > tolerance
            || Math.Abs(newBounds.SizeZ - oldBounds.SizeZ) > tolerance;
    }

    private sealed class LastAppliedBoundsHolder
    {
        public bool HasValue { get; set; }
        public Rect3D Value { get; set; }
    }
}