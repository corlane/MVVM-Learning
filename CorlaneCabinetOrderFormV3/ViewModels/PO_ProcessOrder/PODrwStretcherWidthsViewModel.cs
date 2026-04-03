using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PODrwStretcherWidthsViewModel : ObservableObject
{
    private const string TabId = "DrwStretchers";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private const double DepthThresholdIn = 7.0;
    private const double ReferenceStretcherWidthIn = 6.0;

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public PODrwStretcherWidthsViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public PODrwStretcherWidthsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    public ObservableCollection<DrwStretcherWidthExceptionRow> Exceptions { get; } = new();

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

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            if (baseCab.DrwCount <= 0)
            {
                continue;
            }

            double depthIn = ConvertDimension.FractionToDouble(baseCab.Depth ?? "");
            if (depthIn <= 0)
            {
                continue;
            }

            if (depthIn >= DepthThresholdIn)
            {
                continue;
            }

            var row = new DrwStretcherWidthExceptionRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                Depth = baseCab.Depth ?? "",
                DrwCount = baseCab.DrwCount,
                ReferenceStretcherWidth = ReferenceStretcherWidthIn.ToString("0.##"),
                Instruction = $"Depth < {DepthThresholdIn:0.##}\" with drawers: change drawer stretcher widths to {ReferenceStretcherWidthIn:0.##}\" (ref).",
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DrwStretcherWidthExceptionRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            Exceptions.Add(row);
            TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
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

        foreach (var row in Exceptions)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(DrwStretcherWidthExceptionRow row)
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
        if (Exceptions.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = Exceptions.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class DrwStretcherWidthExceptionRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";

        [ObservableProperty] public partial string Depth { get; set; } = "";
        [ObservableProperty] public partial int DrwCount { get; set; }

        [ObservableProperty] public partial string ReferenceStretcherWidth { get; set; } = "";
        [ObservableProperty] public partial string Instruction { get; set; } = "";
    }
}