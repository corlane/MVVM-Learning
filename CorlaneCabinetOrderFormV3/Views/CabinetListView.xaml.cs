using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class CabinetListView : UserControl
{
    private INotifyCollectionChanged? _cabinetCollectionNotifier;

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
}