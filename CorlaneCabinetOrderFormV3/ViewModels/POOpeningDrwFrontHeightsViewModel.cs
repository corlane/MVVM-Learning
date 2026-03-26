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

public partial class POOpeningDrwFrontHeightsViewModel : ObservableObject
{
    private const string TabId = "DrwFrontHeights";

    // Expected opening heights (inches)
    private const double Expected_OH1_Default = 6.375;    // 6 3/8  – Type1 any, Type2 3-drw OH1, Type2 4-drw OH1
    private const double Expected_OH1_2Drw = 14.3125;  // 14 5/16 – Type2 2-drawer OH1
    private const double Expected_OH2_3Drw = 10.75;    // 10 3/4  – Type2 3-drawer OH2
    private const double Expected_OH23_4Drw = 6.9167;   // ~6 11/12 – Type2 4-drawer OH2 & OH3

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public POOpeningDrwFrontHeightsViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public POOpeningDrwFrontHeightsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    public ObservableCollection<OpeningDrwFrontHeightExceptionRow> Exceptions { get; } = new();

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

            bool isType1 = string.Equals(baseCab.Style, BaseCabinetViewModel.Style1, StringComparison.Ordinal);
            bool isType2 = string.Equals(baseCab.Style, BaseCabinetViewModel.Style2, StringComparison.Ordinal);

            if (!isType1 && !isType2)
            {
                continue;
            }

            int drwCount = baseCab.DrwCount;
            if (drwCount <= 0)
            {
                continue;
            }

            // Opening heights (used for comparisons)
            double oh1 = ConvertDimension.FractionToDouble(baseCab.OpeningHeight1 ?? "");
            double oh2 = ConvertDimension.FractionToDouble(baseCab.OpeningHeight2 ?? "");
            double oh3 = ConvertDimension.FractionToDouble(baseCab.OpeningHeight3 ?? "");

            var reasons = new List<string>();

            if (isType1)
            {
                // Type1 (Standard): OH1 must be 6 3/8
                if (!NearlyEqual(oh1, Expected_OH1_Default))
                {
                    reasons.Add("OH1 expected 6 3/8");
                }
            }
            else if (isType2)
            {
                if (drwCount == 2)
                {
                    // Type2 (Drawer) 2-drw: OH1 must be 14 5/16
                    if (!NearlyEqual(oh1, Expected_OH1_2Drw))
                    {
                        reasons.Add("OH1 expected 14 5/16");
                    }
                }
                else if (drwCount == 3)
                {
                    // Type2 (Drawer) 3-drw: OH1 must be 6 3/8, OH2 must be 10 3/4
                    if (!NearlyEqual(oh1, Expected_OH1_Default))
                    {
                        reasons.Add("OH1 expected 6 3/8");
                    }

                    if (!NearlyEqual(oh2, Expected_OH2_3Drw))
                    {
                        reasons.Add("OH2 expected 10 3/4");
                    }
                }
                else if (drwCount == 4)
                {
                    // Type2 (Drawer) 4-drw: OH1 must be 6 3/8, OH2 & OH3 must be ~6 11/12
                    if (!NearlyEqual(oh1, Expected_OH1_Default))
                    {
                        reasons.Add("OH1 expected 6 3/8");
                    }

                    if (!NearlyEqual(oh2, Expected_OH23_4Drw))
                    {
                        reasons.Add("OH2 expected ~6 11/12");
                    }

                    if (!NearlyEqual(oh3, Expected_OH23_4Drw))
                    {
                        reasons.Add("OH3 expected ~6 11/12");
                    }
                }
            }

            if (reasons.Count == 0)
            {
                continue;
            }

            string styleLabel = isType1 ? "Standard" : $"Drawer ({drwCount} drw)";
            string reason = $"{styleLabel}: {string.Join("; ", reasons)}";

            var row = new OpeningDrwFrontHeightExceptionRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                Style = baseCab.Style ?? "",
                DrwCount = drwCount,

                OpeningHeight1 = baseCab.OpeningHeight1 ?? "",
                OpeningHeight2 = baseCab.OpeningHeight2 ?? "",
                OpeningHeight3 = baseCab.OpeningHeight3 ?? "",
                OpeningHeight4 = baseCab.OpeningHeight4 ?? "",

                Reason = reason,
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(OpeningDrwFrontHeightExceptionRow.IsDone))
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

    private void UpdateDoneKey(OpeningDrwFrontHeightExceptionRow row)
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

    private static bool NearlyEqual(double a, double b)
        => Math.Abs(a - b) < 0.001;

    public sealed partial class OpeningDrwFrontHeightExceptionRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string Style { get; set; } = "";
        [ObservableProperty] public partial int DrwCount { get; set; }

        [ObservableProperty] public partial string OpeningHeight1 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight2 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight3 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight4 { get; set; } = "";

        [ObservableProperty] public partial string Reason { get; set; } = "";
    }
}