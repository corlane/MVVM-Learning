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

public partial class POCornerCabinetDimsViewModel : ObservableObject
{
    private const string TabId = "CornerDims";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private bool _isRefreshing;

    public POCornerCabinetDimsViewModel()
    {
        DefaultBaseLeftFrontWidth = "12";
        DefaultBaseRightFrontWidth = "12";
        DefaultBaseLeftDepth = "24";
        DefaultBaseRightDepth = "24";
        DefaultBaseLeftBackWidth = "36";
        DefaultBaseRightBackWidth = "36";

        DefaultUpperLeftFrontWidth = "12";
        DefaultUpperRightFrontWidth = "12";
        DefaultUpperLeftDepth = "12";
        DefaultUpperRightDepth = "12";

        UpdateTabHeaderBrush();
    }

    public POCornerCabinetDimsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultBaseLeftFrontWidth = "12";
        DefaultBaseRightFrontWidth = "12";
        DefaultBaseLeftDepth = "24";
        DefaultBaseRightDepth = "24";
        DefaultBaseLeftBackWidth = "36";
        DefaultBaseRightBackWidth = "36";

        DefaultUpperLeftFrontWidth = "12";
        DefaultUpperRightFrontWidth = "12";
        DefaultUpperLeftDepth = "12";
        DefaultUpperRightDepth = "12";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty] public partial string DefaultBaseLeftFrontWidth { get; set; } = "12";
    [ObservableProperty] public partial string DefaultBaseRightFrontWidth { get; set; } = "12";
    [ObservableProperty] public partial string DefaultBaseLeftDepth { get; set; } = "24";
    [ObservableProperty] public partial string DefaultBaseRightDepth { get; set; } = "24";
    [ObservableProperty] public partial string DefaultBaseLeftBackWidth { get; set; } = "36";
    [ObservableProperty] public partial string DefaultBaseRightBackWidth { get; set; } = "36";

    [ObservableProperty] public partial string DefaultUpperLeftFrontWidth { get; set; } = "12";
    [ObservableProperty] public partial string DefaultUpperRightFrontWidth { get; set; } = "12";
    [ObservableProperty] public partial string DefaultUpperLeftDepth { get; set; } = "12";
    [ObservableProperty] public partial string DefaultUpperRightDepth { get; set; } = "12";

    public ObservableCollection<CornerCabinetDimsChangeRow> CornerCabinetDimsToChange { get; } = new();

    [ObservableProperty] public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty] public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultBaseLeftFrontWidthChanged(string value) => Refresh();
    partial void OnDefaultBaseRightFrontWidthChanged(string value) => Refresh();
    partial void OnDefaultBaseLeftDepthChanged(string value) => Refresh();
    partial void OnDefaultBaseRightDepthChanged(string value) => Refresh();
    partial void OnDefaultBaseLeftBackWidthChanged(string value) => Refresh();
    partial void OnDefaultBaseRightBackWidthChanged(string value) => Refresh();

    partial void OnDefaultUpperLeftFrontWidthChanged(string value) => Refresh();
    partial void OnDefaultUpperRightFrontWidthChanged(string value) => Refresh();
    partial void OnDefaultUpperLeftDepthChanged(string value) => Refresh();
    partial void OnDefaultUpperRightDepthChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        CornerCabinetDimsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        // IMPORTANT: use the same canonical style strings used by the cabinet entry screens.
        // This prevents mismatches like "Style3" vs "Style 3" vs "90° Corner".
        string baseCorner90Style = BaseCabinetViewModel.Style3;
        string baseAngleFrontStyle = BaseCabinetViewModel.Style4;
        string upperCornerStyle = UpperCabinetViewModel.Style2;

        double defBaseLfw = ConvertDimension.FractionToDouble(DefaultBaseLeftFrontWidth ?? "");
        double defBaseRfw = ConvertDimension.FractionToDouble(DefaultBaseRightFrontWidth ?? "");
        double defBaseLd = ConvertDimension.FractionToDouble(DefaultBaseLeftDepth ?? "");
        double defBaseRd = ConvertDimension.FractionToDouble(DefaultBaseRightDepth ?? "");
        double defBaseLbw = ConvertDimension.FractionToDouble(DefaultBaseLeftBackWidth ?? "");
        double defBaseRbw = ConvertDimension.FractionToDouble(DefaultBaseRightBackWidth ?? "");

        double defUpperLfw = ConvertDimension.FractionToDouble(DefaultUpperLeftFrontWidth ?? "");
        double defUpperRfw = ConvertDimension.FractionToDouble(DefaultUpperRightFrontWidth ?? "");
        double defUpperLd = ConvertDimension.FractionToDouble(DefaultUpperLeftDepth ?? "");
        double defUpperRd = ConvertDimension.FractionToDouble(DefaultUpperRightDepth ?? "");

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            bool isBaseCorner90 = cab is BaseCabinetModel
                && string.Equals(cab.Style, baseCorner90Style, StringComparison.OrdinalIgnoreCase);

            bool isBaseAngleFront = cab is BaseCabinetModel
                && string.Equals(cab.Style, baseAngleFrontStyle, StringComparison.OrdinalIgnoreCase);

            bool isUpperCorner = cab is UpperCabinetModel
                && string.Equals(cab.Style, upperCornerStyle, StringComparison.OrdinalIgnoreCase);

            if (!isBaseCorner90 && !isBaseAngleFront && !isUpperCorner)
            {
                continue;
            }

            string lfw = cab switch
            {
                BaseCabinetModel baseCab1 => baseCab1.LeftFrontWidth ?? "",
                UpperCabinetModel upperCab1 => upperCab1.LeftFrontWidth ?? "",
                _ => ""
            };

            string rfw = cab switch
            {
                BaseCabinetModel baseCab2 => baseCab2.RightFrontWidth ?? "",
                UpperCabinetModel upperCab2 => upperCab2.RightFrontWidth ?? "",
                _ => ""
            };

            string ld = cab switch
            {
                BaseCabinetModel baseCab3 => baseCab3.LeftDepth ?? "",
                UpperCabinetModel upperCab3 => upperCab3.LeftDepth ?? "",
                _ => ""
            };

            string rd = cab switch
            {
                BaseCabinetModel baseCab4 => baseCab4.RightDepth ?? "",
                UpperCabinetModel upperCab4 => upperCab4.RightDepth ?? "",
                _ => ""
            };

            BaseCabinetModel? angleBaseCab = isBaseAngleFront ? (cab as BaseCabinetModel) : null;
            string lbw = angleBaseCab?.LeftBackWidth ?? "";
            string rbw = angleBaseCab?.RightBackWidth ?? "";

            double cabLfw = ConvertDimension.FractionToDouble(lfw);
            double cabRfw = ConvertDimension.FractionToDouble(rfw);
            double cabLd = ConvertDimension.FractionToDouble(ld);
            double cabRd = ConvertDimension.FractionToDouble(rd);
            double cabLbw = ConvertDimension.FractionToDouble(lbw);
            double cabRbw = ConvertDimension.FractionToDouble(rbw);

            bool differs =
                isBaseCorner90
                    ? (!NearlyEqual(cabLfw, defBaseLfw)
                       || !NearlyEqual(cabRfw, defBaseRfw)
                       || !NearlyEqual(cabLd, defBaseLd)
                       || !NearlyEqual(cabRd, defBaseRd))
                : isBaseAngleFront
                    ? (!NearlyEqual(cabLd, defBaseLd)
                       || !NearlyEqual(cabRd, defBaseRd)
                       || !NearlyEqual(cabLbw, defBaseLbw)
                       || !NearlyEqual(cabRbw, defBaseRbw))
                : (!NearlyEqual(cabLfw, defUpperLfw)
                   || !NearlyEqual(cabRfw, defUpperRfw)
                   || !NearlyEqual(cabLd, defUpperLd)
                   || !NearlyEqual(cabRd, defUpperRd));

            if (!differs)
            {
                continue;
            }

            var row = new CornerCabinetDimsChangeRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetType = cab.CabinetType,
                Style = cab.Style ?? "",

                LeftFrontWidth = isBaseAngleFront ? "" : lfw,
                RightFrontWidth = isBaseAngleFront ? "" : rfw,
                LeftDepth = ld,
                RightDepth = rd,
                LeftBackWidth = lbw,
                RightBackWidth = rbw,

                DefaultLeftFrontWidth = isBaseCorner90 ? (DefaultBaseLeftFrontWidth ?? "") : isUpperCorner ? (DefaultUpperLeftFrontWidth ?? "") : "",
                DefaultRightFrontWidth = isBaseCorner90 ? (DefaultBaseRightFrontWidth ?? "") : isUpperCorner ? (DefaultUpperRightFrontWidth ?? "") : "",
                DefaultLeftDepth = (isBaseCorner90 || isBaseAngleFront) ? (DefaultBaseLeftDepth ?? "") : (DefaultUpperLeftDepth ?? ""),
                DefaultRightDepth = (isBaseCorner90 || isBaseAngleFront) ? (DefaultBaseRightDepth ?? "") : (DefaultUpperRightDepth ?? ""),
                DefaultLeftBackWidth = isBaseAngleFront ? (DefaultBaseLeftBackWidth ?? "") : "",
                DefaultRightBackWidth = isBaseAngleFront ? (DefaultBaseRightBackWidth ?? "") : "",

                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CornerCabinetDimsChangeRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            CornerCabinetDimsToChange.Add(row);
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

        foreach (var row in CornerCabinetDimsToChange)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(CornerCabinetDimsChangeRow row)
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
        if (CornerCabinetDimsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = CornerCabinetDimsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    private static bool NearlyEqual(double a, double b)
        => Math.Abs(a - b) < 0.0001;

    public sealed partial class CornerCabinetDimsChangeRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetType { get; set; } = "";
        [ObservableProperty] public partial string Style { get; set; } = "";

        [ObservableProperty] public partial string LeftFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string RightFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string LeftDepth { get; set; } = "";
        [ObservableProperty] public partial string RightDepth { get; set; } = "";
        [ObservableProperty] public partial string LeftBackWidth { get; set; } = "";
        [ObservableProperty] public partial string RightBackWidth { get; set; } = "";

        [ObservableProperty] public partial string DefaultLeftFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string DefaultLeftDepth { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightDepth { get; set; } = "";
        [ObservableProperty] public partial string DefaultLeftBackWidth { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightBackWidth { get; set; } = "";
    }
}