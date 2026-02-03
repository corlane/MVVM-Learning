using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Data;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class CabinetListView : UserControl
{
    private INotifyCollectionChanged? _cabinetCollectionNotifier;

    // Drag helpers
    private Point _dragStartPoint;
    private CabinetModel? _draggedCabinet;
    private bool _isDragging;

    // Ghost adorner helpers
    private AdornerLayer? _ghostAdornerLayer;
    private DragGhostAdorner? _dragGhost;
    private Point _ghostMouseOffset;

    // Insertion adorner helpers
    private AdornerLayer? _adornerLayer;
    private InsertionAdorner? _insertionAdorner;
    private ListViewItem? _lastDragOverItem;
    private int _lastInsertionIndex = -1;

    // Debounce UI refresh during bulk loads (prevents phantom rows + keeps drag/drop stable)
    private DispatcherTimer? _postChangeTimer;
    private bool _pendingScrollToEnd;

    // Drag auto-scroll
    private ScrollViewer? _listScrollViewer;
    private const double DragAutoScrollThresholdPx = 24.0;

    // Scrolling felt too fast because DragOver fires very frequently.
    // Throttle the scroll and make the speed proportional to edge proximity.
    private static readonly TimeSpan DragAutoScrollMinInterval = TimeSpan.FromMilliseconds(16); // ~60Hz
    private DateTime _lastAutoScrollTimeUtc = DateTime.MinValue;
    private const double DragAutoScrollMaxStepPx = 6.0;

    public CabinetListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_postChangeTimer == null)
        {
            _postChangeTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(75)
            };
            _postChangeTimer.Tick += PostChangeTimer_Tick;
        }

        _listScrollViewer ??= FindDescendant<ScrollViewer>(ListViewItems);

        if (DataContext is CabinetListViewModel vm)
        {
            var coll = vm.Cabinets;
            if (coll is INotifyCollectionChanged notifier)
            {
                if (!ReferenceEquals(_cabinetCollectionNotifier, notifier))
                {
                    UnsubscribeCollection();
                    _cabinetCollectionNotifier = notifier;
                    _cabinetCollectionNotifier.CollectionChanged += Cabinets_CollectionChanged;
                }
            }
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        UnsubscribeCollection();

        if (_postChangeTimer != null)
        {
            _postChangeTimer.Stop();
            _postChangeTimer.Tick -= PostChangeTimer_Tick;
            _postChangeTimer = null;
        }
    }

    private void UnsubscribeCollection()
    {
        if (_cabinetCollectionNotifier != null)
        {
            _cabinetCollectionNotifier.CollectionChanged -= Cabinets_CollectionChanged;
            _cabinetCollectionNotifier = null;
        }
    }

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e == null) return;

        // Critical: while dragging, do NOT ScrollIntoView/UpdateLayout; it breaks drag/drop visuals/hit-testing.
        if (_isDragging)
            return;

        // We used to do heavy layout work on every Add; that causes phantom rows during LoadAsync
        // and interferes with container generation. Debounce to a single pass.
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            _pendingScrollToEnd = true;
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // On load, do not auto-scroll. Just remeasure once after all adds settle.
            _pendingScrollToEnd = false;
        }

        _postChangeTimer?.Stop();
        _postChangeTimer?.Start();
    }

    private void PostChangeTimer_Tick(object? sender, EventArgs e)
    {
        _postChangeTimer?.Stop();

        if (_isDragging)
            return;

        try
        {
            if (_pendingScrollToEnd && ListViewItems.Items.Count > 0)
            {
                ListViewItems.ScrollIntoView(ListViewItems.Items[^1]);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _pendingScrollToEnd = false;
        }

        AutoSizeGridViewColumns();

        // Force a safe refresh of the view after bulk changes (helps flush any stale containers)
        try
        {
            if (DataContext is CabinetListViewModel vm)
            {
                CollectionViewSource.GetDefaultView(vm.Cabinets)?.Refresh();
            }
        }
        catch
        {
            // ignore
        }
    }

    private void AutoSizeGridViewColumns()
    {
        if (ListViewItems.View is not GridView gridView) return;

        try
        {
            foreach (var col in gridView.Columns)
            {
                col.Width = 0;
            }

            ListViewItems.UpdateLayout();

            foreach (var col in gridView.Columns)
            {
                col.Width = double.NaN;
            }

            ListViewItems.UpdateLayout();
        }
        catch
        {
            // best-effort
        }
    }

    // ---------- Drag & Drop handlers ----------

    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _draggedCabinet = null;

        var element = e.OriginalSource as DependencyObject;
        var container = FindAncestor<ListViewItem>(element);
        if (container != null)
        {
            _draggedCabinet = (CabinetModel?)ListViewItems.ItemContainerGenerator.ItemFromContainer(container);
        }
    }

    private void ListView_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (_isDragging) return;
        if (_draggedCabinet == null) return;

        Point current = e.GetPosition(null);
        var dx = Math.Abs(current.X - _dragStartPoint.X);
        var dy = Math.Abs(current.Y - _dragStartPoint.Y);
        if (dx <= SystemParameters.MinimumHorizontalDragDistance &&
            dy <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _isDragging = true;

        try
        {
            ListViewItem? sourceContainer = null;

            var element = Mouse.DirectlyOver as DependencyObject;
            sourceContainer = FindAncestor<ListViewItem>(element)
                              ?? FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);

            if (sourceContainer == null && _draggedCabinet != null)
            {
                int idx = ListViewItems.Items.IndexOf(_draggedCabinet);
                if (idx >= 0)
                    sourceContainer = (ListViewItem?)ListViewItems.ItemContainerGenerator.ContainerFromIndex(idx);
            }

            if (sourceContainer != null)
            {
                Point mouseInItem = e.GetPosition(sourceContainer);
                _ghostMouseOffset = mouseInItem;

                var vb = new VisualBrush(sourceContainer)
                {
                    Opacity = 0.9,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };

                _ghostAdornerLayer = AdornerLayer.GetAdornerLayer(ListViewItems);
                if (_ghostAdornerLayer != null)
                {
                    _dragGhost = new DragGhostAdorner(ListViewItems, vb, sourceContainer.RenderSize);
                    _ghostAdornerLayer.Add(_dragGhost);

                    var start = e.GetPosition(ListViewItems);
                    _dragGhost.SetOffset(new Point(start.X - _ghostMouseOffset.X, start.Y - _ghostMouseOffset.Y));
                    _ghostAdornerLayer.Update(ListViewItems);
                }
            }

            var data = new DataObject("CabinetModel", _draggedCabinet);
            DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
        }
        finally
        {
            RemoveInsertionAdorner();
            RemoveDragGhost();
            _isDragging = false;
        }
    }

    private void ListView_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("CabinetModel"))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            RemoveInsertionAdorner();
            RemoveDragGhost();
            return;
        }

        e.Effects = DragDropEffects.Move;

        _listScrollViewer ??= FindDescendant<ScrollViewer>(ListViewItems);
        PerformDragAutoScroll(e);

        if (_dragGhost != null)
        {
            var pos = e.GetPosition(ListViewItems);
            _dragGhost.SetOffset(new Point(pos.X - _ghostMouseOffset.X, pos.Y - _ghostMouseOffset.Y));
            _ghostAdornerLayer?.Update(ListViewItems);
        }

        Point gridPos = e.GetPosition(ListViewItems);
        var targetContainer = GetItemContainerAtPoint(ListViewItems, gridPos);

        int targetIndex;
        bool showAbove;

        if (targetContainer == null)
        {
            targetIndex = ListViewItems.Items.Count - 1;
            showAbove = false;
            if (targetIndex < 0)
            {
                RemoveInsertionAdorner();
                e.Handled = true;
                return;
            }

            targetContainer = (ListViewItem?)ListViewItems.ItemContainerGenerator.ContainerFromIndex(targetIndex);
            if (targetContainer == null)
            {
                RemoveInsertionAdorner();
                e.Handled = true;
                return;
            }
        }
        else
        {
            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
            Point relative = e.GetPosition(targetContainer);
            showAbove = relative.Y < (targetContainer.ActualHeight / 2.0);
        }

        int insertionIndex = showAbove ? targetIndex : targetIndex + 1;

        if (_lastInsertionIndex != insertionIndex || _lastDragOverItem != targetContainer)
        {
            _lastInsertionIndex = insertionIndex;
            _lastDragOverItem = targetContainer;
            ShowInsertionAdorner(targetContainer!, showAbove);
        }

        e.Handled = true;
    }

    private void PerformDragAutoScroll(DragEventArgs e)
    {
        if (_listScrollViewer == null) return;

        var nowUtc = DateTime.UtcNow;
        if (nowUtc - _lastAutoScrollTimeUtc < DragAutoScrollMinInterval)
            return;

        var pos = e.GetPosition(ListViewItems);

        // Within the threshold area, scroll proportionally:
        // closer to the edge => larger step, farther => smaller step.
        if (pos.Y < DragAutoScrollThresholdPx)
        {
            double ratio = (DragAutoScrollThresholdPx - pos.Y) / DragAutoScrollThresholdPx; // 0..1
            double step = Math.Max(.2, DragAutoScrollMaxStepPx * ratio);
            _listScrollViewer.ScrollToVerticalOffset(_listScrollViewer.VerticalOffset - step);
            _lastAutoScrollTimeUtc = nowUtc;
        }
        else if (pos.Y > ListViewItems.ActualHeight - DragAutoScrollThresholdPx)
        {
            double distanceFromBottom = ListViewItems.ActualHeight - pos.Y;
            double ratio = (DragAutoScrollThresholdPx - distanceFromBottom) / DragAutoScrollThresholdPx; // 0..1
            double step = Math.Max(.2, DragAutoScrollMaxStepPx * ratio);
            _listScrollViewer.ScrollToVerticalOffset(_listScrollViewer.VerticalOffset + step);
            _lastAutoScrollTimeUtc = nowUtc;
        }
    }

    private static T? FindDescendant<T>(DependencyObject? root) where T : DependencyObject
    {
        if (root == null) return null;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T t) return t;

            var match = FindDescendant<T>(child);
            if (match != null) return match;
        }

        return null;
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        RemoveInsertionAdorner();
        RemoveDragGhost();

        if (!e.Data.GetDataPresent("CabinetModel")) return;

        var dragged = e.Data.GetData("CabinetModel") as CabinetModel;
        if (dragged == null) return;

        if (DataContext is not CabinetListViewModel vm) return;

        int sourceIndex = vm.Cabinets.IndexOf(dragged);
        if (sourceIndex < 0) return;

        Point pos = e.GetPosition(ListViewItems);
        var targetContainer = GetItemContainerAtPoint(ListViewItems, pos);

        int targetIndex;
        if (targetContainer == null)
        {
            targetIndex = ListViewItems.Items.Count - 1;
            if (targetIndex < 0) targetIndex = 0;
        }
        else
        {
            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
            Point relative = e.GetPosition(targetContainer);
            if (relative.Y >= (targetContainer.ActualHeight / 2.0))
                targetIndex++;
        }

        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex > vm.Cabinets.Count) targetIndex = vm.Cabinets.Count;

        if (sourceIndex != targetIndex && sourceIndex != targetIndex - 1)
        {
            if (sourceIndex < targetIndex)
                targetIndex--;

            vm.Cabinets.Move(sourceIndex, targetIndex);
            ListViewItems.SelectedItem = dragged;

            CollectionViewSource.GetDefaultView(vm.Cabinets)?.Refresh();
        }

        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static ListViewItem? GetItemContainerAtPoint(ListView listView, Point point)
    {
        var element = listView.InputHitTest(point) as DependencyObject;
        if (element == null) return null;
        return FindAncestor<ListViewItem>(element);
    }

    private void RemoveDragGhost()
    {
        try
        {
            if (_dragGhost != null && _ghostAdornerLayer != null)
            {
                _ghostAdornerLayer.Remove(_dragGhost);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _dragGhost = null;
            _ghostAdornerLayer = null;
        }
    }

    private void ShowInsertionAdorner(ListViewItem item, bool showAbove)
    {
        RemoveInsertionAdorner();

        _adornerLayer = AdornerLayer.GetAdornerLayer(ListViewItems);
        if (_adornerLayer == null) return;

        _insertionAdorner = new InsertionAdorner(item, showAbove);
        _adornerLayer.Add(_insertionAdorner);
    }

    private void RemoveInsertionAdorner()
    {
        try
        {
            if (_insertionAdorner != null && _adornerLayer != null)
            {
                _adornerLayer.Remove(_insertionAdorner);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _insertionAdorner = null;
            _adornerLayer = null;
            _lastDragOverItem = null;
            _lastInsertionIndex = -1;
        }
    }

    private sealed class InsertionAdorner : Adorner
    {
        private readonly bool _showAbove;
        private readonly ListViewItem _item;
        private readonly Pen _pen;
        private readonly Brush _triangleBrush;
        private const double TriangleWidth = 8.0;
        private const double LineThickness = 2.0;

        public InsertionAdorner(ListViewItem adornedElement, bool showAbove) : base(adornedElement)
        {
            _item = adornedElement;
            _showAbove = showAbove;

            _pen = new Pen(Brushes.DarkBlue, LineThickness);
            _pen.Freeze();

            _triangleBrush = Brushes.DarkBlue;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var bounds = new Rect(_item.RenderSize);

            double y = _showAbove ? 0.0 : bounds.Height;

            double left = -4;
            double right = bounds.Width + 4;

            var start = new Point(left, y);
            var end = new Point(right, y);
            dc.DrawLine(_pen, start, end);

            var triLeft = CreateTriangle(new Point(left, y), true);
            var triRight = CreateTriangle(new Point(right, y), false);

            dc.DrawGeometry(_triangleBrush, null, triLeft);
            dc.DrawGeometry(_triangleBrush, null, triRight);
        }

        private Geometry CreateTriangle(Point origin, bool pointingRight)
        {
            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                if (pointingRight)
                {
                    ctx.BeginFigure(new Point(origin.X + TriangleWidth, origin.Y - (TriangleWidth / 2)), true, true);
                    ctx.LineTo(new Point(origin.X, origin.Y), true, false);
                    ctx.LineTo(new Point(origin.X + TriangleWidth, origin.Y + (TriangleWidth / 2)), true, false);
                }
                else
                {
                    ctx.BeginFigure(new Point(origin.X - TriangleWidth, origin.Y - (TriangleWidth / 2)), true, true);
                    ctx.LineTo(new Point(origin.X, origin.Y), true, false);
                    ctx.LineTo(new Point(origin.X - TriangleWidth, origin.Y + (TriangleWidth / 2)), true, false);
                }
            }
            g.Freeze();
            return g;
        }
    }

    private sealed class DragGhostAdorner : Adorner
    {
        private readonly Brush _visualBrush;
        private readonly Size _size;
        private Point _offset;
        private readonly double _cornerRadius = 2.0;
        private readonly double _shadowOpacity = 0.25;

        public DragGhostAdorner(UIElement adornedElement, Brush visualBrush, Size size) : base(adornedElement)
        {
            _visualBrush = visualBrush;
            _size = size;
            IsHitTestVisible = false;
        }

        public void SetOffset(Point offset)
        {
            _offset = offset;
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint) => _size;

        protected override Size ArrangeOverride(Size finalSize) => _size;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_size.Width <= 0 || _size.Height <= 0) return;

            var shadowRect = new Rect(_offset.X + 4, _offset.Y + 4, _size.Width, _size.Height);
            dc.PushOpacity(_shadowOpacity);
            dc.DrawRoundedRectangle(Brushes.Black, null, shadowRect, _cornerRadius, _cornerRadius);
            dc.Pop();

            var rect = new Rect(_offset.X, _offset.Y, _size.Width, _size.Height);
            dc.DrawRoundedRectangle(_visualBrush, new Pen(Brushes.Gray, 0.5), rect, _cornerRadius, _cornerRadius);
        }
    }
}