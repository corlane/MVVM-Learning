using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POHingeHolesViewModel : ObservableObject
{
    private const string TabId = "HingeHoles";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public POHingeHolesViewModel()
    {
        // design-time support
        DefaultDrillHingeHoles = true;
        UpdateTabHeaderBrush();
    }

    public POHingeHolesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrillHingeHoles = true;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial bool DefaultDrillHingeHoles { get; set; } = true;

    public ObservableCollection<HingeHolesChangeRow> HingeHolesToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultDrillHingeHolesChanged(bool value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        HingeHolesToChange.Clear();
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

            // Base Type2 (Drawer) must ignore hinge holes entirely.
            if (cab is BaseCabinetModel b2 &&
                string.Equals(b2.Style, "Drawer", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool drillHingeHoles = cab switch
            {
                BaseCabinetModel b => b.DrillHingeHoles,
                UpperCabinetModel u => u.DrillHingeHoles,
                _ => DefaultDrillHingeHoles
            };

            if (drillHingeHoles == DefaultDrillHingeHoles)
            {
                continue;
            }

            var row = new HingeHolesChangeRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                DrillHingeHoles = drillHingeHoles,
                DefaultDrillHingeHoles = DefaultDrillHingeHoles,
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HingeHolesChangeRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            HingeHolesToChange.Add(row);
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

        foreach (var row in HingeHolesToChange)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(HingeHolesChangeRow row)
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
        if (HingeHolesToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = HingeHolesToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class HingeHolesChangeRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";

        [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
        [ObservableProperty] public partial bool DefaultDrillHingeHoles { get; set; }
    }
}