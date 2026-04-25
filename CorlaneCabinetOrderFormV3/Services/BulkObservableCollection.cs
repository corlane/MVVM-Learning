using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// A performance-optimized ObservableCollection used throughout the cabinet order form
/// wherever a list is populated all at once — for example, loading the parts list,
/// populating door/drawer size grids, or refreshing cabinet summary rows.
///
/// The standard ObservableCollection raises a CollectionChanged event for every single
/// item added, which causes the WPF UI to re-measure and re-render on each one. For lists
/// with dozens of parts or cabinet entries, this creates noticeable UI stutter.
///
/// AddRange() bypasses per-item notifications by writing directly to the underlying Items
/// list, then fires a single Reset event at the end. The bound ItemsControl or DataGrid
/// rebuilds its layout exactly once regardless of how many items were added.
/// </summary>
/// 
public class BulkObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Adds all <paramref name="items"/> to the collection without raising a
    /// CollectionChanged event per item. A single Reset notification is raised
    /// after all items have been added, triggering one UI refresh pass.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            Items.Add(item);           // bypasses per-item notification
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}