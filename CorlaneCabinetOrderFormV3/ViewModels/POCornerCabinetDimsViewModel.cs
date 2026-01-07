using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POCornerCabinetDimsViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POCornerCabinetDimsViewModel()
    {
        DefaultBaseLeftFrontWidth = "12";
        DefaultBaseRightFrontWidth = "12";
        DefaultBaseLeftDepth = "24";
        DefaultBaseRightDepth = "24";

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

        CornerCabinetDimsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        // IMPORTANT: use the same canonical style strings used by the cabinet entry screens.
        // This prevents mismatches like "Style3" vs "Style 3" vs "90° Corner".
        string baseCornerStyle = BaseCabinetViewModel.Style3;
        string upperCornerStyle = UpperCabinetViewModel.Style2;

        double defBaseLfw = ConvertDimension.FractionToDouble(DefaultBaseLeftFrontWidth ?? "");
        double defBaseRfw = ConvertDimension.FractionToDouble(DefaultBaseRightFrontWidth ?? "");
        double defBaseLd = ConvertDimension.FractionToDouble(DefaultBaseLeftDepth ?? "");
        double defBaseRd = ConvertDimension.FractionToDouble(DefaultBaseRightDepth ?? "");

        double defUpperLfw = ConvertDimension.FractionToDouble(DefaultUpperLeftFrontWidth ?? "");
        double defUpperRfw = ConvertDimension.FractionToDouble(DefaultUpperRightFrontWidth ?? "");
        double defUpperLd = ConvertDimension.FractionToDouble(DefaultUpperLeftDepth ?? "");
        double defUpperRd = ConvertDimension.FractionToDouble(DefaultUpperRightDepth ?? "");

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            bool isBaseCorner = cab is BaseCabinetModel
                && string.Equals(cab.Style, baseCornerStyle, StringComparison.OrdinalIgnoreCase);

            bool isUpperCorner = cab is UpperCabinetModel
                && string.Equals(cab.Style, upperCornerStyle, StringComparison.OrdinalIgnoreCase);

            if (!isBaseCorner && !isUpperCorner)
            {
                continue;
            }

            string lfw = cab switch
            {
                BaseCabinetModel b => b.LeftFrontWidth ?? "",
                UpperCabinetModel u => u.LeftFrontWidth ?? "",
                _ => ""
            };

            string rfw = cab switch
            {
                BaseCabinetModel b => b.RightFrontWidth ?? "",
                UpperCabinetModel u => u.RightFrontWidth ?? "",
                _ => ""
            };

            string ld = cab switch
            {
                BaseCabinetModel b => b.LeftDepth ?? "",
                UpperCabinetModel u => u.LeftDepth ?? "",
                _ => ""
            };

            string rd = cab switch
            {
                BaseCabinetModel b => b.RightDepth ?? "",
                UpperCabinetModel u => u.RightDepth ?? "",
                _ => ""
            };

            double cabLfw = ConvertDimension.FractionToDouble(lfw);
            double cabRfw = ConvertDimension.FractionToDouble(rfw);
            double cabLd = ConvertDimension.FractionToDouble(ld);
            double cabRd = ConvertDimension.FractionToDouble(rd);

            bool differs = isBaseCorner
                ? (!NearlyEqual(cabLfw, defBaseLfw)
                   || !NearlyEqual(cabRfw, defBaseRfw)
                   || !NearlyEqual(cabLd, defBaseLd)
                   || !NearlyEqual(cabRd, defBaseRd))
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
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetType = cab.CabinetType,
                Style = cab.Style ?? "",

                LeftFrontWidth = lfw,
                RightFrontWidth = rfw,
                LeftDepth = ld,
                RightDepth = rd,

                DefaultLeftFrontWidth = isBaseCorner ? (DefaultBaseLeftFrontWidth ?? "") : (DefaultUpperLeftFrontWidth ?? ""),
                DefaultRightFrontWidth = isBaseCorner ? (DefaultBaseRightFrontWidth ?? "") : (DefaultUpperRightFrontWidth ?? ""),
                DefaultLeftDepth = isBaseCorner ? (DefaultBaseLeftDepth ?? "") : (DefaultUpperLeftDepth ?? ""),
                DefaultRightDepth = isBaseCorner ? (DefaultBaseRightDepth ?? "") : (DefaultUpperRightDepth ?? ""),

                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CornerCabinetDimsChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            CornerCabinetDimsToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
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
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetType { get; set; } = "";
        [ObservableProperty] public partial string Style { get; set; } = "";

        [ObservableProperty] public partial string LeftFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string RightFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string LeftDepth { get; set; } = "";
        [ObservableProperty] public partial string RightDepth { get; set; } = "";

        [ObservableProperty] public partial string DefaultLeftFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightFrontWidth { get; set; } = "";
        [ObservableProperty] public partial string DefaultLeftDepth { get; set; } = "";
        [ObservableProperty] public partial string DefaultRightDepth { get; set; } = "";
    }
}