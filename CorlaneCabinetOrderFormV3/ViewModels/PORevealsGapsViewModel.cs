using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PORevealsGapsViewModel : ObservableObject
{
    private const string TabId = "RevealsGaps";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private readonly DefaultSettingsService? _defaults;
    private bool _isRefreshing;

    public PORevealsGapsViewModel()
    {
        // design-time support
        DefaultBaseTopReveal = "0.4375";
        DefaultBaseLeftReveal = "0.0625";
        DefaultBaseRightReveal = "0.0625";
        DefaultBaseBottomReveal = "0.0625";

        DefaultUpperTopReveal = "0.125";
        DefaultUpperBottomReveal = "0.125";
        DefaultUpperLeftReveal = "0.0625";
        DefaultUpperRightReveal = "0.0625";

        DefaultGapWidth = "0.125";

        UpdateTabHeaderBrush();
    }

    public PORevealsGapsViewModel(ICabinetService cabinetService, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));

        LoadBaselineFromDefaults();

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        // If user changes defaults elsewhere, keep this tab in sync (but preserve user edits if they've already started typing).
        _defaults.PropertyChanged += Defaults_PropertyChanged;

        Refresh();
    }

    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null) return;

        if (e.PropertyName == nameof(DefaultSettingsService.DefaultBaseTopReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultBaseLeftReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultBaseRightReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultBaseBottomReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultUpperTopReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultUpperBottomReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultUpperLeftReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultUpperRightReveal)
            || e.PropertyName == nameof(DefaultSettingsService.DefaultGapWidth))
        {
            LoadBaselineFromDefaults();
            Refresh();
        }
    }

    private void LoadBaselineFromDefaults()
    {
        if (_defaults is null) return;

        DefaultBaseTopReveal = _defaults.DefaultBaseTopReveal;
        DefaultBaseLeftReveal = _defaults.DefaultBaseLeftReveal;
        DefaultBaseRightReveal = _defaults.DefaultBaseRightReveal;
        DefaultBaseBottomReveal = _defaults.DefaultBaseBottomReveal;

        DefaultUpperTopReveal = _defaults.DefaultUpperTopReveal;
        DefaultUpperBottomReveal = _defaults.DefaultUpperBottomReveal;
        DefaultUpperLeftReveal = _defaults.DefaultUpperLeftReveal;
        DefaultUpperRightReveal = _defaults.DefaultUpperRightReveal;

        DefaultGapWidth = _defaults.DefaultGapWidth;
    }

    [ObservableProperty] public partial string DefaultBaseTopReveal { get; set; } = "0.4375";
    [ObservableProperty] public partial string DefaultBaseLeftReveal { get; set; } = "0.0625";
    [ObservableProperty] public partial string DefaultBaseRightReveal { get; set; } = "0.0625";
    [ObservableProperty] public partial string DefaultBaseBottomReveal { get; set; } = "0.0625";

    [ObservableProperty] public partial string DefaultUpperTopReveal { get; set; } = "0.125";
    [ObservableProperty] public partial string DefaultUpperBottomReveal { get; set; } = "0.125";
    [ObservableProperty] public partial string DefaultUpperLeftReveal { get; set; } = "0.0625";
    [ObservableProperty] public partial string DefaultUpperRightReveal { get; set; } = "0.0625";

    [ObservableProperty] public partial string DefaultGapWidth { get; set; } = "0.125";

    public ObservableCollection<RevealsGapsChangeRow> RevealsGapsToChange { get; } = new();

    [ObservableProperty] public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty] public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultBaseTopRevealChanged(string value) => Refresh();
    partial void OnDefaultBaseLeftRevealChanged(string value) => Refresh();
    partial void OnDefaultBaseRightRevealChanged(string value) => Refresh();
    partial void OnDefaultBaseBottomRevealChanged(string value) => Refresh();

    partial void OnDefaultUpperTopRevealChanged(string value) => Refresh();
    partial void OnDefaultUpperBottomRevealChanged(string value) => Refresh();
    partial void OnDefaultUpperLeftRevealChanged(string value) => Refresh();
    partial void OnDefaultUpperRightRevealChanged(string value) => Refresh();

    partial void OnDefaultGapWidthChanged(string value) => Refresh();


    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        RevealsGapsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        double defBaseTop = ConvertDimension.FractionToDouble(DefaultBaseTopReveal ?? "");
        double defBaseLeft = ConvertDimension.FractionToDouble(DefaultBaseLeftReveal ?? "");
        double defBaseRight = ConvertDimension.FractionToDouble(DefaultBaseRightReveal ?? "");
        double defBaseBottom = ConvertDimension.FractionToDouble(DefaultBaseBottomReveal ?? "");

        double defUpperTop = ConvertDimension.FractionToDouble(DefaultUpperTopReveal ?? "");
        double defUpperBottom = ConvertDimension.FractionToDouble(DefaultUpperBottomReveal ?? "");
        double defUpperLeft = ConvertDimension.FractionToDouble(DefaultUpperLeftReveal ?? "");
        double defUpperRight = ConvertDimension.FractionToDouble(DefaultUpperRightReveal ?? "");

        double defGap = ConvertDimension.FractionToDouble(DefaultGapWidth ?? "");

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            bool isBase = cab is BaseCabinetModel;
            bool isUpper = cab is UpperCabinetModel;

            if (!isBase && !isUpper)
            {
                continue;
            }

            // Skip cabinets that don't actually have doors, or have no included fronts (doors + all drawer fronts off).
            if (!ShouldShowInExceptionList(cab))
            {
                continue;
            }

            string left = cab switch
            {
                BaseCabinetModel b => b.LeftReveal ?? "",
                UpperCabinetModel u => u.LeftReveal ?? "",
                _ => ""
            };

            string right = cab switch
            {
                BaseCabinetModel b => b.RightReveal ?? "",
                UpperCabinetModel u => u.RightReveal ?? "",
                _ => ""
            };

            string top = cab switch
            {
                BaseCabinetModel b => b.TopReveal ?? "",
                UpperCabinetModel u => u.TopReveal ?? "",
                _ => ""
            };

            string bottom = cab switch
            {
                BaseCabinetModel b => b.BottomReveal ?? "",
                UpperCabinetModel u => u.BottomReveal ?? "",
                _ => ""
            };

            string gap = cab switch
            {
                BaseCabinetModel b => b.GapWidth ?? "",
                UpperCabinetModel u => u.GapWidth ?? "",
                _ => ""
            };

            double cabLeft = ConvertDimension.FractionToDouble(left);
            double cabRight = ConvertDimension.FractionToDouble(right);
            double cabTop = ConvertDimension.FractionToDouble(top);
            double cabBottom = ConvertDimension.FractionToDouble(bottom);
            double cabGap = ConvertDimension.FractionToDouble(gap);

            bool differs =
                isBase
                    ? (!NearlyEqual(cabTop, defBaseTop)
                       || !NearlyEqual(cabLeft, defBaseLeft)
                       || !NearlyEqual(cabRight, defBaseRight)
                       || !NearlyEqual(cabBottom, defBaseBottom)
                       || !NearlyEqual(cabGap, defGap))
                    : (!NearlyEqual(cabTop, defUpperTop)
                       || !NearlyEqual(cabLeft, defUpperLeft)
                       || !NearlyEqual(cabRight, defUpperRight)
                       || !NearlyEqual(cabBottom, defUpperBottom)
                       || !NearlyEqual(cabGap, defGap));

            if (!differs)
            {
                continue;
            }

            var row = new RevealsGapsChangeRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetType = cab.CabinetType,

                LeftReveal = left,
                RightReveal = right,
                TopReveal = top,
                BottomReveal = bottom,
                GapWidth = gap,

                DefaultLeftReveal = isBase ? (DefaultBaseLeftReveal ?? "") : (DefaultUpperLeftReveal ?? ""),
                DefaultRightReveal = isBase ? (DefaultBaseRightReveal ?? "") : (DefaultUpperRightReveal ?? ""),
                DefaultTopReveal = isBase ? (DefaultBaseTopReveal ?? "") : (DefaultUpperTopReveal ?? ""),
                DefaultBottomReveal = isBase ? (DefaultBaseBottomReveal ?? "") : (DefaultUpperBottomReveal ?? ""),
                DefaultGapWidth = DefaultGapWidth ?? "",

                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RevealsGapsChangeRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            RevealsGapsToChange.Add(row);
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

        foreach (var row in RevealsGapsToChange)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(RevealsGapsChangeRow row)
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
        if (RevealsGapsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = RevealsGapsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    private static bool NearlyEqual(double a, double b)
        => Math.Abs(a - b) < 0.0001;

    public sealed partial class RevealsGapsChangeRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetType { get; set; } = "";

        [ObservableProperty] public partial string LeftReveal { get; set; } = "";
        [ObservableProperty] public partial string RightReveal { get; set; } = "";
        [ObservableProperty] public partial string TopReveal { get; set; } = "";
        [ObservableProperty] public partial string BottomReveal { get; set; } = "";
        [ObservableProperty] public partial string GapWidth { get; set; } = "";

        [ObservableProperty] public partial string DefaultLeftReveal { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightReveal { get; set; } = "";
        [ObservableProperty] public partial string DefaultTopReveal { get; set; } = "";
        [ObservableProperty] public partial string DefaultBottomReveal { get; set; } = "";
        [ObservableProperty] public partial string DefaultGapWidth { get; set; } = "";
    }

    private static bool ShouldShowInExceptionList(CabinetModel cab)
    {
        return cab switch
        {
            // Uppers: only doors exist, so require doors to exist + be included
            UpperCabinetModel u => u.DoorCount > 0 && u.IncDoors,

            // Bases: show if (doors exist + included) OR (any drawer front included for existing openings)
            BaseCabinetModel b => (b.DoorCount > 0 && b.IncDoors) || AnyIncludedDrawerFront(b),

            _ => false
        };
    }

    private static bool AnyIncludedDrawerFront(BaseCabinetModel baseCab)
    {
        int openingCount = Math.Clamp(baseCab.DrwCount, 0, 4);

        for (int i = 1; i <= openingCount; i++)
        {
            bool included = i switch
            {
                1 => baseCab.IncDrwFront1,
                2 => baseCab.IncDrwFront2,
                3 => baseCab.IncDrwFront3,
                4 => baseCab.IncDrwFront4,
                _ => false
            };

            if (included)
            {
                return true;
            }
        }

        return false;
    }
}