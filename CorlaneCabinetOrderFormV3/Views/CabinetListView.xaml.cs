//using CorlaneCabinetOrderFormV3.ViewModels;
//using Microsoft.Extensions.DependencyInjection;
//using System.Collections.Specialized;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Threading;

//namespace CorlaneCabinetOrderFormV3.Views;

//public partial class CabinetListView : UserControl
//{
//    private INotifyCollectionChanged? _cabinetCollectionNotifier;

//    public CabinetListView()
//    {
//        InitializeComponent();
//        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();

//        Loaded += OnLoaded;
//        Unloaded += OnUnloaded;
//    }

//    private void OnLoaded(object? sender, RoutedEventArgs e)
//    {
//        // Subscribe to collection changes so we can scroll to newly added items
//        if (DataContext is CabinetListViewModel vm)
//        {
//            var coll = vm.Cabinets;
//            if (coll is INotifyCollectionChanged notifier)
//            {
//                // ensure we don't subscribe twice
//                if (!ReferenceEquals(_cabinetCollectionNotifier, notifier))
//                {
//                    UnsubscribeCollection();
//                    _cabinetCollectionNotifier = notifier;
//                    _cabinetCollectionNotifier.CollectionChanged += Cabinets_CollectionChanged;
//                }
//            }
//        }
//    }

//    private void OnUnloaded(object? sender, RoutedEventArgs e)
//    {
//        UnsubscribeCollection();
//    }

//    private void UnsubscribeCollection()
//    {
//        if (_cabinetCollectionNotifier != null)
//        {
//            _cabinetCollectionNotifier.CollectionChanged -= Cabinets_CollectionChanged;
//            _cabinetCollectionNotifier = null;
//        }
//    }

//    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        if (e == null) return;

//        // Handle adds and removes separately for best UX
//        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
//        {
//            var lastNewItem = e.NewItems[e.NewItems.Count - 1];
//            // Scroll the new item into view and then re-auto-size columns
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    ListViewItems.ScrollIntoView(lastNewItem);
//                }
//                catch
//                {
//                    // swallow
//                }

//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Remove)
//        {
//            // When items are removed, ensure columns shrink if needed.
//            // Scroll to the last item in the list (if any) and then remeasure.
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    if (ListViewItems.Items.Count > 0)
//                    {
//                        var last = ListViewItems.Items[ListViewItems.Items.Count - 1];
//                        ListViewItems.ScrollIntoView(last);
//                    }
//                }
//                catch
//                {
//                    // swallow
//                }

//                // Recalculate columns after removal so they can shrink
//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Reset)
//        {
//            // Full reset (clear or refresh) — recalc as well
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AutoSizeGridViewColumns()));
//        }
//    }

//    // Force GridView to remeasure columns. Keep this inexpensive: only runs after adds/removes.
//    private void AutoSizeGridViewColumns()
//    {
//        if (ListViewItems.View is not GridView gridView) return;

//        // Robust technique: collapse then let WPF auto-size, with a layout pass in between.
//        // This reliably lets columns both grow and shrink to fit content.
//        try
//        {
//            // Collapse quickly
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = 0;
//            }

//            // Force a layout pass so widths update from collapse
//            ListViewItems.UpdateLayout();

//            // Set to Auto so GridView measures to content
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = double.NaN; // "Auto"
//            }

//            // Final layout pass to ensure proper measurement
//            ListViewItems.UpdateLayout();
//        }
//        catch
//        {
//            // best-effort, ignore failures
//        }
//    }
//}




















//using CorlaneCabinetOrderFormV3.ViewModels;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Specialized;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Threading;
//using CorlaneCabinetOrderFormV3.Models;
//using System.Windows.Data;

//namespace CorlaneCabinetOrderFormV3.Views;

//public partial class CabinetListView : UserControl
//{
//    private INotifyCollectionChanged? _cabinetCollectionNotifier;

//    // Drag helpers
//    private Point _dragStartPoint;
//    private CabinetModel? _draggedCabinet;
//    private bool _isDragging;

//    public CabinetListView()
//    {
//        InitializeComponent();
//        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();

//        Loaded += OnLoaded;
//        Unloaded += OnUnloaded;
//    }

//    private void OnLoaded(object? sender, RoutedEventArgs e)
//    {
//        // Subscribe to collection changes so we can scroll to newly added items
//        if (DataContext is CabinetListViewModel vm)
//        {
//            var coll = vm.Cabinets;
//            if (coll is INotifyCollectionChanged notifier)
//            {
//                // ensure we don't subscribe twice
//                if (!ReferenceEquals(_cabinetCollectionNotifier, notifier))
//                {
//                    UnsubscribeCollection();
//                    _cabinetCollectionNotifier = notifier;
//                    _cabinetCollectionNotifier.CollectionChanged += Cabinets_CollectionChanged;
//                }
//            }
//        }
//    }

//    private void OnUnloaded(object? sender, RoutedEventArgs e)
//    {
//        UnsubscribeCollection();
//    }

//    private void UnsubscribeCollection()
//    {
//        if (_cabinetCollectionNotifier != null)
//        {
//            _cabinetCollectionNotifier.CollectionChanged -= Cabinets_CollectionChanged;
//            _cabinetCollectionNotifier = null;
//        }
//    }

//    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        if (e == null) return;

//        // Handle adds and removes separately for best UX
//        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
//        {
//            var lastNewItem = e.NewItems[e.NewItems.Count - 1];
//            // Scroll the new item into view and then re-auto-size columns
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    ListViewItems.ScrollIntoView(lastNewItem);
//                }
//                catch
//                {
//                    // swallow
//                }

//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Remove)
//        {
//            // When items are removed, ensure columns shrink if needed.
//            // Scroll to the last item in the list (if any) and then remeasure.
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    if (ListViewItems.Items.Count > 0)
//                    {
//                        var last = ListViewItems.Items[ListViewItems.Items.Count - 1];
//                        ListViewItems.ScrollIntoView(last);
//                    }
//                }
//                catch
//                {
//                    // swallow
//                }

//                // Recalculate columns after removal so they can shrink
//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Reset)
//        {
//            // Full reset (clear or refresh) — recalc as well
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AutoSizeGridViewColumns()));
//        }
//    }

//    // Force GridView to remeasure columns. Keep this inexpensive: only runs after adds/removes.
//    private void AutoSizeGridViewColumns()
//    {
//        if (ListViewItems.View is not GridView gridView) return;

//        // Robust technique: collapse then let WPF auto-size, with a layout pass in between.
//        // This reliably lets columns both grow and shrink to fit content.
//        try
//        {
//            // Collapse quickly
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = 0;
//            }

//            // Force a layout pass so widths update from collapse
//            ListViewItems.UpdateLayout();

//            // Set to Auto so GridView measures to content
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = double.NaN; // "Auto"
//            }

//            // Final layout pass to ensure proper measurement
//            ListViewItems.UpdateLayout();
//        }
//        catch
//        {
//            // best-effort, ignore failures
//        }
//    }

//    // ---------- Drag & Drop handlers ----------

//    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _dragStartPoint = e.GetPosition(null);
//        _draggedCabinet = null;

//        // find the ListViewItem under mouse
//        var element = e.OriginalSource as DependencyObject;
//        var container = FindAncestor<ListViewItem>(element);
//        if (container != null)
//        {
//            _draggedCabinet = (CabinetModel?)ListViewItems.ItemContainerGenerator.ItemFromContainer(container);
//        }
//    }

//    private void ListView_MouseMove(object sender, MouseEventArgs e)
//    {
//        if (e.LeftButton != MouseButtonState.Pressed) return;
//        if (_isDragging) return;
//        if (_draggedCabinet == null) return;

//        Point current = e.GetPosition(null);
//        var dx = Math.Abs(current.X - _dragStartPoint.X);
//        var dy = Math.Abs(current.Y - _dragStartPoint.Y);
//        if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
//        {
//            _isDragging = true;
//            try
//            {
//                var data = new DataObject("CabinetModel", _draggedCabinet);
//                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
//            }
//            finally
//            {
//                _isDragging = false;
//            }
//        }
//    }

//    private void ListView_DragOver(object sender, DragEventArgs e)
//    {
//        if (e.Data.GetDataPresent("CabinetModel"))
//        {
//            e.Effects = DragDropEffects.Move;
//        }
//        else
//        {
//            e.Effects = DragDropEffects.None;
//        }
//        e.Handled = true;
//    }

//    private void ListView_Drop(object sender, DragEventArgs e)
//    {
//        if (!e.Data.GetDataPresent("CabinetModel")) return;

//        var dragged = e.Data.GetData("CabinetModel") as CabinetModel;
//        if (dragged == null) return;

//        if (DataContext is not CabinetListViewModel vm) return;

//        // determine source index
//        int sourceIndex = vm.Cabinets.IndexOf(dragged);
//        if (sourceIndex < 0) return;

//        // get target container under mouse
//        Point pos = e.GetPosition(ListViewItems);
//        var targetContainer = GetItemContainerAtPoint(ListViewItems, pos);

//        int targetIndex;
//        if (targetContainer == null)
//        {
//            // dropped on empty space -> move to end
//            targetIndex = ListViewItems.Items.Count - 1;
//        }
//        else
//        {
//            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
//        }

//        if (targetIndex < 0) return;

//        // adjust target index if necessary (when inserting after removal)
//        if (sourceIndex != targetIndex)
//        {
//            // ObservableCollection.Move will update the collection and UI
//            // If dragging downward, after removal the target index shifts left by 1.
//            if (sourceIndex < targetIndex)
//                targetIndex--; // because item will be removed from before the insertion point

//            vm.Cabinets.Move(sourceIndex, targetIndex);

//            // ensure selection moves with item
//            ListViewItems.SelectedItem = dragged;

//            // Refresh view if numbering didn't update (defensive)
//            CollectionViewSource.GetDefaultView(vm.Cabinets)?.Refresh();
//        }

//        e.Handled = true;
//    }

//    // Helper: find ancestor of given type
//    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
//    {
//        while (current != null)
//        {
//            if (current is T t) return t;
//            current = VisualTreeHelper.GetParent(current);
//        }
//        return null;
//    }

//    // Helper: get ListViewItem at a point
//    private static ListViewItem? GetItemContainerAtPoint(ListView listView, Point point)
//    {
//        var element = listView.InputHitTest(point) as DependencyObject;
//        if (element == null) return null;
//        return FindAncestor<ListViewItem>(element);
//    }
//}

























//using CorlaneCabinetOrderFormV3.ViewModels;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Threading;
//using CorlaneCabinetOrderFormV3.Models;
//using System.Windows.Data;

//namespace CorlaneCabinetOrderFormV3.Views;

//public partial class CabinetListView : UserControl
//{
//    private INotifyCollectionChanged? _cabinetCollectionNotifier;

//    // Drag helpers
//    private Point _dragStartPoint;
//    private CabinetModel? _draggedCabinet;
//    private bool _isDragging;

//    // Insertion adorner helpers
//    private AdornerLayer? _adornerLayer;
//    private InsertionAdorner? _insertionAdorner;
//    private ListViewItem? _lastDragOverItem;
//    private int _lastInsertionIndex = -1;

//    public CabinetListView()
//    {
//        InitializeComponent();
//        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();

//        Loaded += OnLoaded;
//        Unloaded += OnUnloaded;
//    }

//    private void OnLoaded(object? sender, RoutedEventArgs e)
//    {
//        // Subscribe to collection changes so we can scroll to newly added items
//        if (DataContext is CabinetListViewModel vm)
//        {
//            var coll = vm.Cabinets;
//            if (coll is INotifyCollectionChanged notifier)
//            {
//                // ensure we don't subscribe twice
//                if (!ReferenceEquals(_cabinetCollectionNotifier, notifier))
//                {
//                    UnsubscribeCollection();
//                    _cabinetCollectionNotifier = notifier;
//                    _cabinetCollectionNotifier.CollectionChanged += Cabinets_CollectionChanged;
//                }
//            }
//        }
//    }

//    private void OnUnloaded(object? sender, RoutedEventArgs e)
//    {
//        UnsubscribeCollection();
//    }

//    private void UnsubscribeCollection()
//    {
//        if (_cabinetCollectionNotifier != null)
//        {
//            _cabinetCollectionNotifier.CollectionChanged -= Cabinets_CollectionChanged;
//            _cabinetCollectionNotifier = null;
//        }
//    }

//    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        if (e == null) return;

//        // Handle adds and removes separately for best UX
//        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
//        {
//            var lastNewItem = e.NewItems[e.NewItems.Count - 1];
//            // Scroll the new item into view and then re-auto-size columns
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    ListViewItems.ScrollIntoView(lastNewItem);
//                }
//                catch
//                {
//                    // swallow
//                }

//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Remove)
//        {
//            // When items are removed, ensure columns shrink if needed.
//            // Scroll to the last item in the list (if any) and then remeasure.
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
//            {
//                try
//                {
//                    if (ListViewItems.Items.Count > 0)
//                    {
//                        var last = ListViewItems.Items[ListViewItems.Items.Count - 1];
//                        ListViewItems.ScrollIntoView(last);
//                    }
//                }
//                catch
//                {
//                    // swallow
//                }

//                // Recalculate columns after removal so they can shrink
//                AutoSizeGridViewColumns();
//            }));
//        }
//        else if (e.Action == NotifyCollectionChangedAction.Reset)
//        {
//            // Full reset (clear or refresh) — recalc as well
//            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AutoSizeGridViewColumns()));
//        }
//    }

//    // Force GridView to remeasure columns. Keep this inexpensive: only runs after adds/removes.
//    private void AutoSizeGridViewColumns()
//    {
//        if (ListViewItems.View is not GridView gridView) return;

//        // Robust technique: collapse then let WPF auto-size, with a layout pass in between.
//        // This reliably lets columns both grow and shrink to fit content.
//        try
//        {
//            // Collapse quickly
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = 0;
//            }

//            // Force a layout pass so widths update from collapse
//            ListViewItems.UpdateLayout();

//            // Set to Auto so GridView measures to content
//            foreach (var col in gridView.Columns)
//            {
//                col.Width = double.NaN; // "Auto"
//            }

//            // Final layout pass to ensure proper measurement
//            ListViewItems.UpdateLayout();
//        }
//        catch
//        {
//            // best-effort, ignore failures
//        }
//    }

//    // ---------- Drag & Drop handlers ----------

//    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _dragStartPoint = e.GetPosition(null);
//        _draggedCabinet = null;

//        // find the ListViewItem under mouse
//        var element = e.OriginalSource as DependencyObject;
//        var container = FindAncestor<ListViewItem>(element);
//        if (container != null)
//        {
//            _draggedCabinet = (CabinetModel?)ListViewItems.ItemContainerGenerator.ItemFromContainer(container);
//        }
//    }

//    private void ListView_MouseMove(object sender, MouseEventArgs e)
//    {
//        if (e.LeftButton != MouseButtonState.Pressed) return;
//        if (_isDragging) return;
//        if (_draggedCabinet == null) return;

//        Point current = e.GetPosition(null);
//        var dx = Math.Abs(current.X - _dragStartPoint.X);
//        var dy = Math.Abs(current.Y - _dragStartPoint.Y);
//        if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
//        {
//            _isDragging = true;
//            try
//            {
//                var data = new DataObject("CabinetModel", _draggedCabinet);
//                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
//            }
//            finally
//            {
//                _isDragging = false;
//            }
//        }
//    }

//    private void ListView_DragOver(object sender, DragEventArgs e)
//    {
//        if (!e.Data.GetDataPresent("CabinetModel"))
//        {
//            e.Effects = DragDropEffects.None;
//            e.Handled = true;
//            RemoveInsertionAdorner();
//            return;
//        }

//        e.Effects = DragDropEffects.Move;

//        // Compute insertion position
//        Point pos = e.GetPosition(ListViewItems);
//        var targetContainer = GetItemContainerAtPoint(ListViewItems, pos);

//        int targetIndex;
//        bool showAbove;

//        if (targetContainer == null)
//        {
//            // dropped on empty space -> adorner at end (after last)
//            targetIndex = ListViewItems.Items.Count - 1;
//            showAbove = false;
//            if (targetIndex < 0)
//            {
//                // empty list — nothing to show
//                RemoveInsertionAdorner();
//                e.Handled = true;
//                return;
//            }
//            targetContainer = (ListViewItem?)ListViewItems.ItemContainerGenerator.ContainerFromIndex(targetIndex);
//            if (targetContainer == null)
//            {
//                RemoveInsertionAdorner();
//                e.Handled = true;
//                return;
//            }
//        }
//        else
//        {
//            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
//            // decide above/below by mouse Y relative to item midpoint
//            Point relative = e.GetPosition(targetContainer);
//            showAbove = relative.Y < (targetContainer.ActualHeight / 2.0);
//        }

//        // Determine insertion index for Move call semantics
//        int insertionIndex = showAbove ? targetIndex : targetIndex + 1;

//        // Avoid redundant updates
//        if (_lastInsertionIndex != insertionIndex || _lastDragOverItem != targetContainer)
//        {
//            _lastInsertionIndex = insertionIndex;
//            _lastDragOverItem = targetContainer;
//            ShowInsertionAdorner(targetContainer!, showAbove);
//        }

//        e.Handled = true;
//    }

//    private void ListView_Drop(object sender, DragEventArgs e)
//    {
//        RemoveInsertionAdorner();

//        if (!e.Data.GetDataPresent("CabinetModel")) return;

//        var dragged = e.Data.GetData("CabinetModel") as CabinetModel;
//        if (dragged == null) return;

//        if (DataContext is not CabinetListViewModel vm) return;

//        // determine source index
//        int sourceIndex = vm.Cabinets.IndexOf(dragged);
//        if (sourceIndex < 0) return;

//        // get target container under mouse
//        Point pos = e.GetPosition(ListViewItems);
//        var targetContainer = GetItemContainerAtPoint(ListViewItems, pos);

//        int targetIndex;
//        if (targetContainer == null)
//        {
//            // dropped on empty space -> move to end
//            targetIndex = ListViewItems.Items.Count - 1;
//            if (targetIndex < 0) targetIndex = 0;
//        }
//        else
//        {
//            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
//            // choose before/after based on pointer
//            Point relative = e.GetPosition(targetContainer);
//            if (relative.Y >= (targetContainer.ActualHeight / 2.0))
//                targetIndex = targetIndex + 1;
//        }

//        if (targetIndex < 0) targetIndex = 0;
//        if (targetIndex > vm.Cabinets.Count) targetIndex = vm.Cabinets.Count;

//        // Adjust index because Move uses indices into current collection
//        if (sourceIndex != targetIndex && sourceIndex != targetIndex - 1)
//        {
//            // If moving forward (source < target), removal shifts indices left by 1.
//            if (sourceIndex < targetIndex)
//                targetIndex--; // insertion index after removal

//            vm.Cabinets.Move(sourceIndex, targetIndex);

//            // ensure selection moves with item
//            ListViewItems.SelectedItem = dragged;

//            // Refresh view if numbering didn't update (defensive)
//            CollectionViewSource.GetDefaultView(vm.Cabinets)?.Refresh();
//        }

//        e.Handled = true;
//    }

//    // Helper: find ancestor of given type
//    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
//    {
//        while (current != null)
//        {
//            if (current is T t) return t;
//            current = VisualTreeHelper.GetParent(current);
//        }
//        return null;
//    }

//    // Helper: get ListViewItem at a point
//    private static ListViewItem? GetItemContainerAtPoint(ListView listView, Point point)
//    {
//        var element = listView.InputHitTest(point) as DependencyObject;
//        if (element == null) return null;
//        return FindAncestor<ListViewItem>(element);
//    }

//    // ---------- Insertion Adorner management ----------

//    private void ShowInsertionAdorner(ListViewItem item, bool showAbove)
//    {
//        RemoveInsertionAdorner();

//        _adornerLayer = AdornerLayer.GetAdornerLayer(ListViewItems);
//        if (_adornerLayer == null) return;

//        _insertionAdorner = new InsertionAdorner(item, showAbove);
//        _adornerLayer.Add(_insertionAdorner);
//    }

//    private void RemoveInsertionAdorner()
//    {
//        try
//        {
//            if (_insertionAdorner != null && _adornerLayer != null)
//            {
//                _adornerLayer.Remove(_insertionAdorner);
//            }
//        }
//        catch
//        {
//            // best-effort
//        }
//        finally
//        {
//            _insertionAdorner = null;
//            _adornerLayer = null;
//            _lastDragOverItem = null;
//            _lastInsertionIndex = -1;
//        }
//    }

//    // ---------- InsertionAdorner nested class ----------
//    private sealed class InsertionAdorner : Adorner
//    {
//        private readonly bool _showAbove;
//        private readonly ListViewItem _item;
//        private readonly Pen _pen;
//        private readonly Brush _triangleBrush;
//        private const double TriangleWidth = 8.0;
//        private const double LineThickness = 2.0;

//        public InsertionAdorner(ListViewItem adornedElement, bool showAbove) : base(adornedElement)
//        {
//            _item = adornedElement;
//            _showAbove = showAbove;

//            _pen = new Pen(Brushes.DarkBlue, LineThickness);
//            _pen.Freeze();

//            _triangleBrush = Brushes.DarkBlue;
//            IsHitTestVisible = false;
//        }

//        protected override void OnRender(DrawingContext dc)
//        {
//            base.OnRender(dc);

//            var bounds = new Rect(_item.RenderSize);

//            // y position: top edge or bottom edge of the item
//            double y = _showAbove ? 0.0 : bounds.Height;

//            // horizontal line across the width (slightly inset)
//            double left = -4;
//            double right = bounds.Width + 4;

//            // Draw a horizontal line
//            var start = new Point(left, y);
//            var end = new Point(right, y);
//            dc.DrawLine(_pen, start, end);

//            // draw triangular caps at both ends to make insertion handle
//            var triLeft = CreateTriangle(new Point(left, y), true);
//            var triRight = CreateTriangle(new Point(right, y), false);

//            dc.DrawGeometry(_triangleBrush, null, triLeft);
//            dc.DrawGeometry(_triangleBrush, null, triRight);
//        }

//        private Geometry CreateTriangle(Point origin, bool pointingRight)
//        {
//            // small isosceles triangle pointing inward
//            var g = new StreamGeometry();
//            using (var ctx = g.Open())
//            {
//                if (pointingRight)
//                {
//                    ctx.BeginFigure(new Point(origin.X + TriangleWidth, origin.Y - (TriangleWidth / 2)), true, true);
//                    ctx.LineTo(new Point(origin.X, origin.Y), true, false);
//                    ctx.LineTo(new Point(origin.X + TriangleWidth, origin.Y + (TriangleWidth / 2)), true, false);
//                }
//                else
//                {
//                    ctx.BeginFigure(new Point(origin.X - TriangleWidth, origin.Y - (TriangleWidth / 2)), true, true);
//                    ctx.LineTo(new Point(origin.X, origin.Y), true, false);
//                    ctx.LineTo(new Point(origin.X - TriangleWidth, origin.Y + (TriangleWidth / 2)), true, false);
//                }
//            }
//            g.Freeze();
//            return g;
//        }
//    }
//}






using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Data;
using System.Windows.Shapes;

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
    private Point _ghostMouseOffset; // offset from top-left of item to mouse when drag started

    // Insertion adorner helpers
    private AdornerLayer? _adornerLayer;
    private InsertionAdorner? _insertionAdorner;
    private ListViewItem? _lastDragOverItem;
    private int _lastInsertionIndex = -1;

    public CabinetListView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<CabinetListViewModel>();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Subscribe to collection changes so we can scroll to newly added items
        if (DataContext is CabinetListViewModel vm)
        {
            var coll = vm.Cabinets;
            if (coll is INotifyCollectionChanged notifier)
            {
                // ensure we don't subscribe twice
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

        // Handle adds and removes separately for best UX
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
        {
            var lastNewItem = e.NewItems[e.NewItems.Count - 1];
            // Scroll the new item into view and then re-auto-size columns
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                try
                {
                    ListViewItems.ScrollIntoView(lastNewItem);
                }
                catch
                {
                    // swallow
                }

                AutoSizeGridViewColumns();
            }));
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            // When items are removed, ensure columns shrink if needed.
            // Scroll to the last item in the list (if any) and then remeasure.
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                try
                {
                    if (ListViewItems.Items.Count > 0)
                    {
                        var last = ListViewItems.Items[ListViewItems.Items.Count - 1];
                        ListViewItems.ScrollIntoView(last);
                    }
                }
                catch
                {
                    // swallow
                }

                // Recalculate columns after removal so they can shrink
                AutoSizeGridViewColumns();
            }));
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Full reset (clear or refresh) — recalc as well
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AutoSizeGridViewColumns()));
        }
    }

    // Force GridView to remeasure columns. Keep this inexpensive: only runs after adds/removes.
    private void AutoSizeGridViewColumns()
    {
        if (ListViewItems.View is not GridView gridView) return;

        // Robust technique: collapse then let WPF auto-size, with a layout pass in between.
        // This reliably lets columns both grow and shrink to fit content.
        try
        {
            // Collapse quickly
            foreach (var col in gridView.Columns)
            {
                col.Width = 0;
            }

            // Force a layout pass so widths update from collapse
            ListViewItems.UpdateLayout();

            // Set to Auto so GridView measures to content
            foreach (var col in gridView.Columns)
            {
                col.Width = double.NaN; // "Auto"
            }

            // Final layout pass to ensure proper measurement
            ListViewItems.UpdateLayout();
        }
        catch
        {
            // best-effort, ignore failures
        }
    }

    // ---------- Drag & Drop handlers ----------

    private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _draggedCabinet = null;

        // find the ListViewItem under mouse
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
        if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
        {
            _isDragging = true;
            ListViewItem? sourceContainer = null;
            try
            {
                // capture source ListViewItem for the ghost
                var element = Mouse.DirectlyOver as DependencyObject;
                sourceContainer = FindAncestor<ListViewItem>(element)
                                  ?? FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);

                // fallback: try index-of dragged item
                if (sourceContainer == null && _draggedCabinet != null)
                {
                    int idx = ListViewItems.Items.IndexOf(_draggedCabinet);
                    if (idx >= 0)
                        sourceContainer = (ListViewItem?)ListViewItems.ItemContainerGenerator.ContainerFromIndex(idx);
                }

                // Create ghost adorner from source container BEFORE starting DoDragDrop
                if (sourceContainer != null)
                {
                    // mouse offset inside item (so ghost tracks under cursor the same way)
                    Point mouseInItem = e.GetPosition(sourceContainer);
                    _ghostMouseOffset = mouseInItem;

                    // visual snapshot brush
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

                        // position initial ghost under mouse
                        var start = e.GetPosition(ListViewItems);
                        _dragGhost.SetOffset(new Point(start.X - _ghostMouseOffset.X, start.Y - _ghostMouseOffset.Y));
                        // ensure initial render
                        _ghostAdornerLayer.Update(ListViewItems);
                    }
                }

                var data = new DataObject("CabinetModel", _draggedCabinet);
                try
                {
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                }
                finally
                {
                    // ensure ghost always removed after DoDragDrop completes (successful or cancelled)
                    RemoveDragGhost();
                }
            }
            finally
            {
                _isDragging = false;
            }
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

        // Update drag-ghost position (so it follows cursor)
        if (_dragGhost != null)
        {
            var pos = e.GetPosition(ListViewItems);
            _dragGhost.SetOffset(new Point(pos.X - _ghostMouseOffset.X, pos.Y - _ghostMouseOffset.Y));
            // Force adorner layer to refresh so ghost moves immediately
            _ghostAdornerLayer?.Update(ListViewItems);
        }

        // Compute insertion position
        Point gridPos = e.GetPosition(ListViewItems);
        var targetContainer = GetItemContainerAtPoint(ListViewItems, gridPos);

        int targetIndex;
        bool showAbove;

        if (targetContainer == null)
        {
            // dropped on empty space -> adorner at end (after last)
            targetIndex = ListViewItems.Items.Count - 1;
            showAbove = false;
            if (targetIndex < 0)
            {
                // empty list — nothing to show
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
            // decide above/below by mouse Y relative to item midpoint
            Point relative = e.GetPosition(targetContainer);
            showAbove = relative.Y < (targetContainer.ActualHeight / 2.0);
        }

        // Determine insertion index for Move call semantics
        int insertionIndex = showAbove ? targetIndex : targetIndex + 1;

        // Avoid redundant updates
        if (_lastInsertionIndex != insertionIndex || _lastDragOverItem != targetContainer)
        {
            _lastInsertionIndex = insertionIndex;
            _lastDragOverItem = targetContainer;
            ShowInsertionAdorner(targetContainer!, showAbove);
        }

        e.Handled = true;
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        RemoveInsertionAdorner();
        RemoveDragGhost();

        if (!e.Data.GetDataPresent("CabinetModel")) return;

        var dragged = e.Data.GetData("CabinetModel") as CabinetModel;
        if (dragged == null) return;

        if (DataContext is not CabinetListViewModel vm) return;

        // determine source index
        int sourceIndex = vm.Cabinets.IndexOf(dragged);
        if (sourceIndex < 0) return;

        // get target container under mouse
        Point pos = e.GetPosition(ListViewItems);
        var targetContainer = GetItemContainerAtPoint(ListViewItems, pos);

        int targetIndex;
        if (targetContainer == null)
        {
            // dropped on empty space -> move to end
            targetIndex = ListViewItems.Items.Count - 1;
            if (targetIndex < 0) targetIndex = 0;
        }
        else
        {
            targetIndex = ListViewItems.ItemContainerGenerator.IndexFromContainer(targetContainer);
            // choose before/after based on pointer
            Point relative = e.GetPosition(targetContainer);
            if (relative.Y >= (targetContainer.ActualHeight / 2.0))
                targetIndex = targetIndex + 1;
        }

        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex > vm.Cabinets.Count) targetIndex = vm.Cabinets.Count;

        // Adjust index because Move uses indices into current collection
        if (sourceIndex != targetIndex && sourceIndex != targetIndex - 1)
        {
            // If moving forward (source < target), removal shifts indices left by 1.
            if (sourceIndex < targetIndex)
                targetIndex--; // insertion index after removal

            vm.Cabinets.Move(sourceIndex, targetIndex);

            // ensure selection moves with item
            ListViewItems.SelectedItem = dragged;

            // Refresh view if numbering didn't update (defensive)
            CollectionViewSource.GetDefaultView(vm.Cabinets)?.Refresh();
        }

        e.Handled = true;
    }

    // Helper: find ancestor of given type
    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    // Helper: get ListViewItem at a point
    private static ListViewItem? GetItemContainerAtPoint(ListView listView, Point point)
    {
        var element = listView.InputHitTest(point) as DependencyObject;
        if (element == null) return null;
        return FindAncestor<ListViewItem>(element);
    }

    // ---------- Drag ghost adorner management ----------

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
            // swallow
        }
        finally
        {
            _dragGhost = null;
            _ghostAdornerLayer = null;
        }
    }

    // ---------- Insertion Adorner management ----------

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
            // best-effort
        }
        finally
        {
            _insertionAdorner = null;
            _adornerLayer = null;
            _lastDragOverItem = null;
            _lastInsertionIndex = -1;
        }
    }

    // ---------- InsertionAdorner nested class ----------
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

            // y position: top edge or bottom edge of the item
            double y = _showAbove ? 0.0 : bounds.Height;

            // horizontal line across the width (slightly inset)
            double left = -4;
            double right = bounds.Width + 4;

            // Draw a horizontal line
            var start = new Point(left, y);
            var end = new Point(right, y);
            dc.DrawLine(_pen, start, end);

            // draw triangular caps at both ends to make insertion handle
            var triLeft = CreateTriangle(new Point(left, y), true);
            var triRight = CreateTriangle(new Point(right, y), false);

            dc.DrawGeometry(_triangleBrush, null, triLeft);
            dc.DrawGeometry(_triangleBrush, null, triRight);
        }

        private Geometry CreateTriangle(Point origin, bool pointingRight)
        {
            // small isosceles triangle pointing inward
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

    // ---------- DragGhostAdorner nested class ----------
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

        // Set where the ghost should be drawn (call from UI thread)
        public void SetOffset(Point offset)
        {
            _offset = offset;
            // Request redraw immediately
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return _size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return _size;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_size.Width <= 0 || _size.Height <= 0) return;

            // draw a subtle drop shadow rectangle
            var shadowRect = new Rect(_offset.X + 4, _offset.Y + 4, _size.Width, _size.Height);
            dc.PushOpacity(_shadowOpacity);
            dc.DrawRoundedRectangle(Brushes.Black, null, shadowRect, _cornerRadius, _cornerRadius);
            dc.Pop();

            // draw the actual visual content
            var rect = new Rect(_offset.X, _offset.Y, _size.Width, _size.Height);
            dc.DrawRoundedRectangle(_visualBrush, new Pen(Brushes.Gray, 0.5), rect, _cornerRadius, _cornerRadius);
        }
    }
}




