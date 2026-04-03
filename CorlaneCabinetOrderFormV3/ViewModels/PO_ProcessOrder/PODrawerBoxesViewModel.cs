using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PODrawerBoxesViewModel : ObservableObject
{
    private const string TabId = "DrawerBoxes";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public PODrawerBoxesViewModel()
    {
        // design-time support
        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultIncDrwBoxes = true;
        UpdateTabHeaderBrush();
    }

    public PODrawerBoxesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultIncDrwBoxes = true;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDrwStyle { get; set; } = "Blum Tandem H/Equivalent Undermount";
    partial void OnDefaultDrwStyleChanged(string value) => Refresh();

    // Invert comparison by toggling this (default = true).
    [ObservableProperty]
    public partial bool DefaultIncDrwBoxes { get; set; } = true;
    partial void OnDefaultIncDrwBoxesChanged(bool value) => Refresh();

    public ObservableCollection<DrawerBoxExceptionRow> Exceptions { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        Exceptions.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        string defaultStyle = (DefaultDrwStyle ?? "").Trim();

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            // Do not check for drawer boxes in sink cabinets even if DrwCount > 0
            if (cab.SinkCabinet)
            {
                continue;
            }

            bool anyRowsAddedForCab = false;

            int drwCount = Math.Clamp(baseCab.DrwCount, 0, 4);

            if (drwCount > 0)
            {
                for (int opening = 1; opening <= drwCount; opening++)
                {
                    bool inc = GetIncDrwBoxForOpening(baseCab, opening);

                    if (inc == DefaultIncDrwBoxes)
                    {
                        continue;
                    }

                    string exType = "Drawer Box Include";
                    string drwBoxType = $"Opening {opening}";

                    TrackRow(new DrawerBoxExceptionRow
                    {
                        CabinetId = cab.Id,
                        CabinetNumber = cabNumber,
                        CabinetName = baseCab.Name ?? "",
                        ExceptionType = exType,
                        DrawerBoxType = drwBoxType,
                        Actual = IncludeLabel(inc),
                        Default = IncludeLabel(DefaultIncDrwBoxes),
                        IsDone = savedKeys?.Contains(MakeKey(cab.Id, exType, drwBoxType)) == true
                    });

                    anyRowsAddedForCab = true;
                }
            }

            bool anyDrawerBoxIncluded = drwCount > 0
                && Enumerable.Range(1, drwCount).Any(o => GetIncDrwBoxForOpening(baseCab, o));

            if (anyDrawerBoxIncluded)
            {
                string actualStyle = (baseCab.DrwStyle ?? "").Trim();

                if (!string.Equals(actualStyle, defaultStyle, StringComparison.OrdinalIgnoreCase))
                {
                    string exType = "Drawer Slide Type";
                    string drwBoxType = "";

                    TrackRow(new DrawerBoxExceptionRow
                    {
                        CabinetId = cab.Id,
                        CabinetNumber = cabNumber,
                        CabinetName = baseCab.Name ?? "",
                        ExceptionType = exType,
                        DrawerBoxType = drwBoxType,
                        Actual = actualStyle,
                        Default = defaultStyle,
                        IsDone = savedKeys?.Contains(MakeKey(cab.Id, exType, drwBoxType)) == true
                    });

                    anyRowsAddedForCab = true;
                }
            }

            // Rollout include exception – same logic as drawer box openings
            if (baseCab.RolloutCount > 0 && baseCab.IncRollouts != DefaultIncDrwBoxes)
            {
                string exType = "Rollout Include";
                string drwBoxType = "";

                TrackRow(new DrawerBoxExceptionRow
                {
                    CabinetId = cab.Id,
                    CabinetNumber = cabNumber,
                    CabinetName = baseCab.Name ?? "",
                    ExceptionType = exType,
                    DrawerBoxType = drwBoxType,
                    Actual = IncludeLabel(baseCab.IncRollouts),
                    Default = IncludeLabel(DefaultIncDrwBoxes),
                    IsDone = savedKeys?.Contains(MakeKey(cab.Id, exType, drwBoxType)) == true
                });

                anyRowsAddedForCab = true;
            }

            // Trash drawer box include exception
            if (baseCab.TrashDrawer && baseCab.IncTrashDrwBox != DefaultIncDrwBoxes)
            {
                string exType = "Trash Drawer Box Include";
                string drwBoxType = "";

                TrackRow(new DrawerBoxExceptionRow
                {
                    CabinetId = cab.Id,
                    CabinetNumber = cabNumber,
                    CabinetName = baseCab.Name ?? "",
                    ExceptionType = exType,
                    DrawerBoxType = drwBoxType,
                    Actual = IncludeLabel(baseCab.IncTrashDrwBox),
                    Default = IncludeLabel(DefaultIncDrwBoxes),
                    IsDone = savedKeys?.Contains(MakeKey(cab.Id, exType, drwBoxType)) == true
                });

                anyRowsAddedForCab = true;
            }

            if (anyRowsAddedForCab)
            {
                TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
            }
        }

        UpdateTabHeaderBrush();
        _isRefreshing = false;
    }





    [RelayCommand]
    private void RefreshList() => Refresh();

    private void TrackRow(DrawerBoxExceptionRow row)
    {
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DrawerBoxExceptionRow.IsDone))
            {
                if (!_isRefreshing) UpdateDoneKey(row);
                UpdateTabHeaderBrush();
            }
        };

        Exceptions.Add(row);
    }

    private static string MakeKey(Guid cabinetId, string exceptionType, string drawerBoxType)
        => $"{cabinetId:N}|{exceptionType}|{drawerBoxType}";

    private void SnapshotDoneKeys()
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        foreach (var row in Exceptions)
        {
            var key = MakeKey(row.CabinetId, row.ExceptionType, row.DrawerBoxType);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(DrawerBoxExceptionRow row)
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        var key = MakeKey(row.CabinetId, row.ExceptionType, row.DrawerBoxType);
        if (row.IsDone) set.Add(key); else set.Remove(key);

        _cabinetService.RaiseExceptionDoneStateChanged();
    }

    private static bool GetIncDrwBoxForOpening(BaseCabinetModel baseCab, int opening) => opening switch
    {
        1 => baseCab.IncDrwBoxOpening1,
        2 => baseCab.IncDrwBoxOpening2,
        3 => baseCab.IncDrwBoxOpening3,
        4 => baseCab.IncDrwBoxOpening4,
        _ => false
    };

    private static string IncludeLabel(bool include) => include ? "Include" : "Remove";

    private void UpdateTabHeaderBrush()
    {
        if (Exceptions.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = Exceptions.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class DrawerBoxExceptionRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";

        [ObservableProperty] public partial string ExceptionType { get; set; } = "";
        [ObservableProperty] public partial string DrawerBoxType { get; set; } = "";

        [ObservableProperty] public partial string Actual { get; set; } = "";
        [ObservableProperty] public partial string Default { get; set; } = "";
    }
}