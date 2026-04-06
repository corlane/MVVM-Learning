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

public partial class POBackGrainDirViewModel : ObservableObject
{
    private const string TabId = "BackGrainDir";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    // Default thresholds matching the rendering code in BaseCabinetBuilder.Standard.cs / UpperCabinetBuilder.Standard.cs.
    // 3/4" back: width > 49.25  (47.75 + 2 * 0.75)
    // 1/4" back: width > 47.75
    private const double DefaultThreeQuarterBackWidth = 49.25;
    private const double DefaultQuarterBackWidth = 47.75;

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public POBackGrainDirViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public POBackGrainDirViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    /// <summary>Width threshold for 3/4" back cabinets. Cabinets wider than this need a grain direction change.</summary>
    [ObservableProperty]
    public partial string ThreeQuarterBackWidthThreshold { get; set; } = DefaultThreeQuarterBackWidth.ToString("0.##");
    partial void OnThreeQuarterBackWidthThresholdChanged(string value) => Refresh();

    /// <summary>Width threshold for 1/4" back cabinets. Cabinets wider than this need a grain direction change.</summary>
    [ObservableProperty]
    public partial string QuarterBackWidthThreshold { get; set; } = DefaultQuarterBackWidth.ToString("0.##");
    partial void OnQuarterBackWidthThresholdChanged(string value) => Refresh();

    public ObservableCollection<BackGrainDirExceptionRow> Exceptions { get; } = new();

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

        double threshold34 = ConvertDimension.FractionToDouble(ThreeQuarterBackWidthThreshold ?? "");
        double threshold14 = ConvertDimension.FractionToDouble(QuarterBackWidthThreshold ?? "");

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            string? backThickness = null;
            bool isApplicable = false;

            if (cab is BaseCabinetModel baseCab)
            {
                // Standard (Type1) and Drawer (Type2) base cabinets
                if (baseCab.Style == CabinetStyles.Base.Standard || baseCab.Style == CabinetStyles.Base.Drawer)
                {
                    backThickness = baseCab.BackThickness;
                    isApplicable = true;
                }
            }
            else if (cab is UpperCabinetModel upperCab)
            {
                // Standard (Type1) upper cabinets only
                if (upperCab.Style == CabinetStyles.Upper.Standard)
                {
                    backThickness = upperCab.BackThickness;
                    isApplicable = true;
                }
            }

            if (!isApplicable || string.IsNullOrWhiteSpace(backThickness))
                continue;

            double widthIn = ConvertDimension.FractionToDouble(cab.Width ?? "");
            if (widthIn <= 0)
                continue;

            bool isThreeQuarterBack = IsThreeQuarterBack(backThickness);
            double threshold = isThreeQuarterBack ? threshold34 : threshold14;

            if (widthIn <= threshold)
                continue;

            // Width exceeds threshold — flag for grain direction change
            var row = new BackGrainDirExceptionRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetType = cab.CabinetType,
                Width = cab.Width ?? "",
                BackThickness = isThreeQuarterBack ? "3/4\"" : "1/4\"",
                Threshold = threshold.ToString("0.##"),
                Instruction = $"Width ({cab.Width}) exceeds {threshold:0.##}\" — change back grain direction to Horizontal.",
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BackGrainDirExceptionRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            Exceptions.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
        _isRefreshing = false;
    }

    [RelayCommand]
    private void RefreshList() => Refresh();

    private static bool IsThreeQuarterBack(string backThickness)
    {
        // Match both decimal and fractional representations
        return backThickness == CabinetOptions.BackThickness.ThreeQuarterDecimal
            || backThickness == CabinetOptions.BackThickness.ThreeQuarterFraction;
    }

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

    private void UpdateDoneKey(BackGrainDirExceptionRow row)
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

    public sealed partial class BackGrainDirExceptionRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }
        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetType { get; set; } = "";
        [ObservableProperty] public partial string Width { get; set; } = "";
        [ObservableProperty] public partial string BackThickness { get; set; } = "";
        [ObservableProperty] public partial string Threshold { get; set; } = "";
        [ObservableProperty] public partial string Instruction { get; set; } = "";
    }
}