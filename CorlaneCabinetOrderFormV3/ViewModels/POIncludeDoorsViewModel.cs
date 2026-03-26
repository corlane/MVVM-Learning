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

public partial class POIncludeDoorsViewModel : ObservableObject
{
    private const string TabId = "IncludeDoors";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    private bool _refreshQueued;
    private bool _isRefreshing;

    public POIncludeDoorsViewModel()
    {
        // design-time support
        DefaultIncDoors = false;
        UpdateTabHeaderBrush();
    }

    public POIncludeDoorsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultIncDoors = false;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += Cabinets_CollectionChanged;

            // Also track existing cabinet property changes so the exception list stays live.
            foreach (var cab in _cabinetService.Cabinets)
            {
                HookCabinet(cab);
            }
        }

        Refresh();
    }

    [ObservableProperty]
    public partial bool DefaultIncDoors { get; set; } = false;

    public ObservableCollection<IncDoorsChangeRow> DoorsToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultIncDoorsChanged(bool value) => RequestRefresh();

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var ni in e.NewItems)
            {
                if (ni is CabinetModel cab)
                {
                    HookCabinet(cab);
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (var oi in e.OldItems)
            {
                if (oi is CabinetModel cab)
                {
                    UnhookCabinet(cab);
                }
            }
        }

        // On Reset (bulk load), NewItems is null — re-hook all current items
        if (e.Action == NotifyCollectionChangedAction.Reset && _cabinetService?.Cabinets != null)
        {
            foreach (var cab in _cabinetService.Cabinets)
            {
                HookCabinet(cab);
            }
        }

        RequestRefresh();
    }



    private void HookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
        {
            // Listen for all properties; we filter in the handler.
            PropertyChangedEventManager.AddHandler(inpc, Cabinet_PropertyChanged, string.Empty);
        }
    }

    private void UnhookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
        {
            PropertyChangedEventManager.RemoveHandler(inpc, Cabinet_PropertyChanged, string.Empty);
        }
    }

    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Keep this focused to the properties that affect this list.
        if (e.PropertyName is nameof(BaseCabinetModel.IncDoors)
            or nameof(UpperCabinetModel.IncDoors)
            or nameof(BaseCabinetModel.DrwCount)
            or nameof(BaseCabinetModel.IncDrwFront1)
            or nameof(BaseCabinetModel.IncDrwFront2)
            or nameof(BaseCabinetModel.IncDrwFront3)
            or nameof(BaseCabinetModel.IncDrwFront4)
            or nameof(BaseCabinetModel.DoorSpecies)
            or nameof(BaseCabinetModel.CustomDoorSpecies)
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

    private static string GetFrontSpecies(CabinetModel cab)
    {
        string doorSpecies = cab switch
        {
            BaseCabinetModel b => (b.DoorSpecies ?? "").Trim(),
            UpperCabinetModel u => (u.DoorSpecies ?? "").Trim(),
            _ => ""
        };

        if (string.Equals(doorSpecies, "Custom", StringComparison.OrdinalIgnoreCase))
        {
            string custom = cab switch
            {
                BaseCabinetModel b => (b.CustomDoorSpecies ?? "").Trim(),
                UpperCabinetModel u => (u.CustomDoorSpecies ?? "").Trim(),
                _ => ""
            };

            if (!string.IsNullOrEmpty(custom))
            {
                doorSpecies = custom;
            }
        }

        return doorSpecies;
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

        DoorsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        void TrackRow(IncDoorsChangeRow row)
        {
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IncDoorsChangeRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            DoorsToChange.Add(row);
        }

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;
            bool anyRowsAddedForCab = false;

            var frontSpecies = GetFrontSpecies(cab);

            // Doors exception (Base/Upper only)
            bool incDoors = cab switch
            {
                BaseCabinetModel b => b.IncDoors,
                UpperCabinetModel u => u.IncDoors,
                _ => DefaultIncDoors
            };

            bool isDoorException = cab is BaseCabinetModel or UpperCabinetModel
                && incDoors != DefaultIncDoors;

            if (isDoorException)
            {
                TrackRow(new IncDoorsChangeRow
                {
                    CabinetId = cab.Id,
                    CabinetNumber = cabNumber,
                    CabinetName = cab.Name ?? "",
                    Type = "Door",
                    FrontSpecies = frontSpecies,
                    IncDoors = incDoors,
                    DefaultIncDoors = DefaultIncDoors,
                    IsDone = savedKeys?.Contains(MakeKey(cab.Id, "Door")) == true
                });

                anyRowsAddedForCab = true;
            }

            // Drawer front exceptions (BaseCabinetModel only) - independent of doors exception
            if (cab is BaseCabinetModel baseCab && baseCab.DrwCount > 0)
            {
                for (int i = 1; i <= baseCab.DrwCount; i++)
                {
                    bool incDrwFront = i switch
                    {
                        1 => baseCab.IncDrwFront1,
                        2 => baseCab.IncDrwFront2,
                        3 => baseCab.IncDrwFront3,
                        4 => baseCab.IncDrwFront4,
                        _ => DefaultIncDoors
                    };

                    if (incDrwFront == DefaultIncDoors)
                    {
                        continue;
                    }

                    string type = $"Drawer Front {i}";

                    TrackRow(new IncDoorsChangeRow
                    {
                        CabinetId = cab.Id,
                        CabinetNumber = cabNumber,
                        CabinetName = cab.Name ?? "",
                        Type = type,
                        FrontSpecies = frontSpecies,
                        IncDoors = incDrwFront,
                        DefaultIncDoors = DefaultIncDoors,
                        IsDone = savedKeys?.Contains(MakeKey(cab.Id, type)) == true
                    });

                    anyRowsAddedForCab = true;
                }
            }

            if (anyRowsAddedForCab)
            {
                TotalCabsNeedingChange += Math.Max(1, cab.Qty);
            }
        }

        UpdateTabHeaderBrush();
        _isRefreshing = false;
    }

    [RelayCommand]
    private void RefreshList() => Refresh();

    private static string MakeKey(Guid cabinetId, string type)
        => $"{cabinetId:N}|{type}";

    private void SnapshotDoneKeys()
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        foreach (var row in DoorsToChange)
        {
            var key = MakeKey(row.CabinetId, row.Type);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(IncDoorsChangeRow row)
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        var key = MakeKey(row.CabinetId, row.Type);
        if (row.IsDone) set.Add(key); else set.Remove(key);

        _cabinetService.RaiseExceptionDoneStateChanged();
    }

    private void UpdateTabHeaderBrush()
    {
        if (DoorsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = DoorsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class IncDoorsChangeRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string Type { get; set; } = "";

        [ObservableProperty] public partial string FrontSpecies { get; set; } = "";

        [ObservableProperty] public partial bool IncDoors { get; set; }
        [ObservableProperty] public partial bool DefaultIncDoors { get; set; }
    }
}