using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// ObservableCollection that supports adding many items with a single Reset notification.
/// </summary>
public class BulkObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Adds all items silently, then raises a single
    /// <see cref="NotifyCollectionChangedAction.Reset"/> event.
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