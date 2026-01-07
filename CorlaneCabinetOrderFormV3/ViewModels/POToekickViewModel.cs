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

public partial class POToekickViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POToekickViewModel()
    {
        // design-time support
        DefaultTkHeight = "4";
        DefaultTkDepth = "3.75";
        UpdateTabHeaderBrush();
    }

    public POToekickViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultTkHeight = "4";
        DefaultTkDepth = "3.75";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultTkHeight { get; set; } = "4";

    [ObservableProperty]
    public partial string DefaultTkDepth { get; set; } = "3.75";

    public ObservableCollection<ToekickChangeRow> ToekickDimensionsToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultTkHeightChanged(string value) => Refresh();
    partial void OnDefaultTkDepthChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        ToekickDimensionsToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        double defaultHeight = ConvertDimension.FractionToDouble(DefaultTkHeight ?? "");
        double defaultDepth = ConvertDimension.FractionToDouble(DefaultTkDepth ?? "");

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            cabNumber++;

            if (!baseCab.HasTK)
            {
                continue;
            }

            double cabHeight = ConvertDimension.FractionToDouble(baseCab.TKHeight ?? "");
            double cabDepth = ConvertDimension.FractionToDouble(baseCab.TKDepth ?? "");

            bool heightDiffers = !NearlyEqual(cabHeight, defaultHeight);
            bool depthDiffers = !NearlyEqual(cabDepth, defaultDepth);

            if (!heightDiffers && !depthDiffers)
            {
                continue;
            }

            var row = new ToekickChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                TkHeight = baseCab.TKHeight ?? "",
                TkDepth = baseCab.TKDepth ?? "",
                DefaultTkHeight = DefaultTkHeight ?? "",
                DefaultTkDepth = DefaultTkDepth ?? "",
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ToekickChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            ToekickDimensionsToChange.Add(row);

            TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        // Two-state:
        // - green when there are no exceptions OR everything is marked Done
        // - red when there are exceptions and at least one is not Done
        if (ToekickDimensionsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = ToekickDimensionsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    private static bool NearlyEqual(double a, double b)
        => Math.Abs(a - b) < 0.0001;

    public sealed partial class ToekickChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string TkHeight { get; set; } = "";
        [ObservableProperty] public partial string TkDepth { get; set; } = "";
        [ObservableProperty] public partial string DefaultTkHeight { get; set; } = "";
        [ObservableProperty] public partial string DefaultTkDepth { get; set; } = "";
    }
}