using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Renders small thumbnail images for cabinets in the list.
/// Fully reactive: subscribes to the shared Cabinets collection and
/// auto-generates thumbnails when cabinets are added or geometry changes.
/// </summary>
public class ThumbnailService
{
    private const int ThumbWidth = 128;
    private const int ThumbHeight = 128;

    // Tracks the GeometryVersion at which each cabinet was last rendered.
    private readonly Dictionary<Guid, int> _versionCache = new();
    private readonly HashSet<Guid> _pendingIds = new();
    private readonly ICabinetService _cabinetService;
    private DispatcherTimer? _debounceTimer;

    public ThumbnailService(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService;

        ((INotifyCollectionChanged)_cabinetService.Cabinets).CollectionChanged += OnCollectionChanged;

        // Hook any items already present (normally empty at startup)
        foreach (var cab in _cabinetService.Cabinets)
            cab.PropertyChanged += OnCabinetPropertyChanged;
    }

    // ── Reactive hooks ────────────────────────────────────────────────

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CabinetModel cab in e.OldItems)
                cab.PropertyChanged -= OnCabinetPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (CabinetModel cab in e.NewItems)
            {
                cab.PropertyChanged += OnCabinetPropertyChanged;
                QueueRegeneration(cab.Id);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _versionCache.Clear();
            _pendingIds.Clear();
            foreach (var cab in _cabinetService.Cabinets)
            {
                cab.PropertyChanged += OnCabinetPropertyChanged;
                QueueRegeneration(cab.Id);
            }
        }
    }

    private void OnCabinetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CabinetModel cab) return;
        if (e.PropertyName == nameof(CabinetModel.Thumbnail)) return; // avoid feedback loop

        // Only regenerate if the geometry version has actually bumped
        if (_versionCache.TryGetValue(cab.Id, out var cached) && cab.GeometryVersion == cached)
            return;

        QueueRegeneration(cab.Id);
    }

    // ── Debounced batch processing ────────────────────────────────────

    private void QueueRegeneration(Guid id)
    {
        _pendingIds.Add(id);
        EnsureDebounceTimer();
        _debounceTimer!.Stop();
        _debounceTimer.Start();
    }

    private void EnsureDebounceTimer()
    {
        if (_debounceTimer is not null) return;
        _debounceTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _debounceTimer.Tick += DebounceTimer_Tick;
    }

    private void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        _debounceTimer?.Stop();

        if (_pendingIds.Count == 0) return;

        // Snapshot the pending IDs then clear, so new requests during
        // processing start a fresh debounce cycle.
        var batch = _pendingIds.ToList();
        _pendingIds.Clear();

        ProcessBatch(batch, 0);
    }

    /// <summary>
    /// Processes one thumbnail per Dispatcher frame at Background priority
    /// so the UI stays responsive during bulk loads.
    /// </summary>
    private void ProcessBatch(List<Guid> ids, int index)
    {
        if (index >= ids.Count) return;

        Application.Current?.Dispatcher?.BeginInvoke(
            DispatcherPriority.Background,
            () =>
            {
                var cab = _cabinetService.Cabinets.FirstOrDefault(c => c.Id == ids[index]);
                if (cab is not null)
                    GenerateThumbnail(cab);

                ProcessBatch(ids, index + 1);
            });
    }

    // ── Rendering ─────────────────────────────────────────────────────

    private void GenerateThumbnail(CabinetModel cab)
    {
        try
        {
            var bmp = RenderThumbnail(cab);
            _versionCache[cab.Id] = cab.GeometryVersion;
            cab.Thumbnail = bmp;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Catch] ThumbnailService: render failed for {cab.Id}: {ex.Message}");
        }
    }

    private static BitmapSource? RenderThumbnail(CabinetModel cab)
    {
        // BuildPreviewModel resets & recomputes material/edge totals as a side-effect.
        // The totals end up correct (no hide flags, all parts), equivalent to
        // AccumulateMaterialAndEdgeTotals. Single-threaded UI, so no race.
        var model = CabinetPreviewBuilder.BuildPreviewModel(
            cab,
            leftEndHidden: false,
            rightEndHidden: false,
            deckHidden: false,
            topHidden: false,
            doorsHidden: false);

        if (model?.Bounds is not Rect3D bounds || bounds.IsEmpty)
            return null;

        // ── Offscreen Viewport3D ──────────────────────────────────────
        var viewport = new Viewport3D();

        // Extra ambient light so shadow side isn't pitch-black
        viewport.Children.Add(new ModelVisual3D
        {
            Content = new AmbientLight(Color.FromRgb(90, 90, 90))
        });

        viewport.Children.Add(new ModelVisual3D { Content = model });

        // Orthographic front-view camera (matches main viewport look direction)
        var center = new Point3D(
            bounds.X + bounds.SizeX / 2,
            bounds.Y + bounds.SizeY / 2,
            bounds.Z + bounds.SizeZ / 2);

        double maxExtent = Math.Max(bounds.SizeX, bounds.SizeY);

        // Slight 3D perspective: 10° from the right, 10° looking down
        const double azimuthDeg = 10.0;
        const double elevationDeg = 10.0;
        double azimuthRad = azimuthDeg * Math.PI / 180.0;
        double elevationRad = elevationDeg * Math.PI / 180.0;
        double distance = bounds.SizeZ + maxExtent;

        double offsetX = distance * Math.Sin(azimuthRad);
        double offsetY = distance * Math.Sin(elevationRad);
        double offsetZ = distance * Math.Cos(azimuthRad) * Math.Cos(elevationRad);

        var cameraPos = new Point3D(
            center.X + offsetX,
            center.Y + offsetY,
            center.Z + offsetZ);

        var lookDir = center - cameraPos;

        viewport.Camera = new OrthographicCamera
        {
            Position = cameraPos,
            LookDirection = lookDir,
            UpDirection = new Vector3D(0, 1, 0),
            Width = maxExtent * 1.4 // slightly wider to account for angled view
        };

        // Wrap in a Border so background renders behind the 3D content
        var border = new Border
        {
            Background = Brushes.White,
            Child = viewport,
            Width = ThumbWidth,
            Height = ThumbHeight
        };

        // Force WPF layout pass (required for offscreen rendering)
        border.Measure(new Size(ThumbWidth, ThumbHeight));
        border.Arrange(new Rect(0, 0, ThumbWidth, ThumbHeight));
        border.UpdateLayout();

        var rtb = new RenderTargetBitmap(ThumbWidth, ThumbHeight, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(border);
        rtb.Freeze(); // thread-safe, good for WPF perf
        return rtb;
    }
}