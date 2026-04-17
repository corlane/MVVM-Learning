using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POEdgebandDoorsViewModel : ObservableObject
{
    private const string TabId = "EdgebandDoors";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    private bool _refreshQueued;
    private bool _isRefreshing;

    public POEdgebandDoorsViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public POEdgebandDoorsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += Cabinets_CollectionChanged;

            foreach (var cab in _cabinetService.Cabinets)
            {
                HookCabinet(cab);
            }
        }

        Refresh();
    }

    public ObservableCollection<EdgebandDoorsChangeRow> CabsToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var ni in e.NewItems)
            {
                if (ni is CabinetModel cab)
                    HookCabinet(cab);
            }
        }

        if (e.OldItems != null)
        {
            foreach (var oi in e.OldItems)
            {
                if (oi is CabinetModel cab)
                    UnhookCabinet(cab);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset && _cabinetService?.Cabinets != null)
        {
            foreach (var cab in _cabinetService.Cabinets)
                HookCabinet(cab);
        }

        RequestRefresh();
    }

    private void HookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
            PropertyChangedEventManager.AddHandler(inpc, Cabinet_PropertyChanged, string.Empty);
    }

    private void UnhookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
            PropertyChangedEventManager.RemoveHandler(inpc, Cabinet_PropertyChanged, string.Empty);
    }

    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BaseCabinetModel.EdgebandDoorsAndDrawers)
            or nameof(BaseCabinetModel.IncDoors)
            or nameof(UpperCabinetModel.IncDoors)
            or nameof(BaseCabinetModel.DoorCount)
            or nameof(UpperCabinetModel.DoorCount)
            or nameof(BaseCabinetModel.IncDrwFronts)
            or nameof(BaseCabinetModel.IncDrwFront1)
            or nameof(BaseCabinetModel.IncDrwFront2)
            or nameof(BaseCabinetModel.IncDrwFront3)
            or nameof(BaseCabinetModel.IncDrwFront4)
            or nameof(BaseCabinetModel.DrwCount)
            or nameof(CabinetModel.Name)
            or nameof(CabinetModel.Qty))
        {
            RequestRefresh();
        }
    }

    private void RequestRefresh()
    {
        if (Application.Current?.Dispatcher == null)
        {
            Refresh();
            return;
        }

        if (_refreshQueued) return;

        _refreshQueued = true;
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _refreshQueued = false;
            Refresh();
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// A cabinet is flagged when EdgebandDoorsAndDrawers is false AND it has
    /// doors or drawer fronts that are included (i.e. they would need edgebanding).
    /// </summary>
    private static bool IsFlagged(CabinetModel cab)
    {
        bool ebDoors = cab switch
        {
            BaseCabinetModel b => b.EdgebandDoorsAndDrawers,
            UpperCabinetModel u => u.EdgebandDoorsAndDrawers,
            _ => true // not applicable
        };

        if (ebDoors)
            return false; // edgebanding is on — no exception

        // Check doors
        bool hasDoors = cab switch
        {
            BaseCabinetModel b => b.IncDoors && b.DoorCount > 0,
            UpperCabinetModel u => u.IncDoors && u.DoorCount > 0,
            _ => false
        };

        if (hasDoors)
            return true;

        // Check drawer fronts (BaseCabinetModel only)
        if (cab is BaseCabinetModel baseCab)
        {
            if (baseCab.IncDrwFronts)
                return true;

            for (int i = 1; i <= baseCab.DrwCount; i++)
            {
                bool inc = i switch
                {
                    1 => baseCab.IncDrwFront1,
                    2 => baseCab.IncDrwFront2,
                    3 => baseCab.IncDrwFront3,
                    4 => baseCab.IncDrwFront4,
                    _ => false
                };
                if (inc) return true;
            }
        }

        return false;
    }

    private static string DescribeReason(CabinetModel cab)
    {
        var parts = new List<string>();

        bool hasDoors = cab switch
        {
            BaseCabinetModel b => b.IncDoors && b.DoorCount > 0,
            UpperCabinetModel u => u.IncDoors && u.DoorCount > 0,
            _ => false
        };
        if (hasDoors) parts.Add("Doors");

        if (cab is BaseCabinetModel baseCab)
        {
            if (baseCab.IncDrwFronts)
            {
                parts.Add("Drawer Fronts");
            }
            else
            {
                for (int i = 1; i <= baseCab.DrwCount; i++)
                {
                    bool inc = i switch
                    {
                        1 => baseCab.IncDrwFront1,
                        2 => baseCab.IncDrwFront2,
                        3 => baseCab.IncDrwFront3,
                        4 => baseCab.IncDrwFront4,
                        _ => false
                    };
                    if (inc) parts.Add($"Drw Front {i}");
                }
            }
        }

        return string.Join(", ", parts);
    }

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        CabsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            if (cab is not (BaseCabinetModel or UpperCabinetModel))
                continue;

            if (!IsFlagged(cab))
                continue;

            var row = new EdgebandDoorsChangeRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                Reason = DescribeReason(cab),
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EdgebandDoorsChangeRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            CabsToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
        _isRefreshing = false;
    }

    [RelayCommand]
    private void RefreshList() => Refresh();

    private static string MakeKey(Guid cabinetId) => cabinetId.ToString("N");

    private void SnapshotDoneKeys()
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        foreach (var row in CabsToChange)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(EdgebandDoorsChangeRow row)
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        var key = MakeKey(row.CabinetId);
        if (row.IsDone) set.Add(key); else set.Remove(key);

        _cabinetService.RaiseExceptionDoneStateChanged();
    }

    private void UpdateTabHeaderBrush()
    {
        if (CabsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = CabsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class EdgebandDoorsChangeRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string Reason { get; set; } = "";
    }
}