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

public partial class POOpeningDrwFrontHeightsViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POOpeningDrwFrontHeightsViewModel()
    {
        // design-time support
        DefaultDrwFront1Height = "7";
        UpdateTabHeaderBrush();
    }

    public POOpeningDrwFrontHeightsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrwFront1Height = "7";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDrwFront1Height { get; set; } = "7";

    public ObservableCollection<OpeningDrwFrontHeightExceptionRow> Exceptions { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultDrwFront1HeightChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        Exceptions.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        double defaultFront1 = ConvertDimension.FractionToDouble(DefaultDrwFront1Height ?? "");

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            cabNumber++;

            bool isStyle1 = string.Equals(baseCab.Style, BaseCabinetViewModel.Style1, StringComparison.Ordinal);
            bool isStyle2 = string.Equals(baseCab.Style, BaseCabinetViewModel.Style2, StringComparison.Ordinal);

            if (!isStyle1 && !isStyle2)
            {
                continue;
            }

            // Exclusion: Style2 with DrwCount = 1 is NOT applicable to any of these flags.
            if (isStyle2 && baseCab.DrwCount == 1)
            {
                continue;
            }

            if (baseCab.DrwCount == 0)
            {
                continue; // never flag drawers/openings when there are zero drawers
            }

            string h1 = baseCab.DrwFrontHeight1 ?? "";
            string h2 = baseCab.DrwFrontHeight2 ?? "";
            string h3 = baseCab.DrwFrontHeight3 ?? "";
            string h4 = baseCab.DrwFrontHeight4 ?? "";

            double dh1 = ConvertDimension.FractionToDouble(h1);
            double dh2 = ConvertDimension.FractionToDouble(h2);
            double dh3 = ConvertDimension.FractionToDouble(h3);
            double dh4 = ConvertDimension.FractionToDouble(h4);

            // Baseline deviation applies to Style1 + Style2 (no DrwCount gating; you requested it should still flag)
            bool baselineDiffers = !NearlyEqual(dh1, defaultFront1);

            // Additional Style2 rules:
            bool style2Mismatch =
                isStyle2
                && ((baseCab.DrwCount == 3 && !NearlyEqual(dh2, dh3))
                    || (baseCab.DrwCount == 4 && (!NearlyEqual(dh2, dh3) || !NearlyEqual(dh3, dh4))));

            if (!baselineDiffers && !style2Mismatch)
            {
                continue;
            }

            string reason = BuildReason(isStyle2, baseCab.DrwCount, baselineDiffers, style2Mismatch);

            var row = new OpeningDrwFrontHeightExceptionRow
            {
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                Style = baseCab.Style ?? "",
                DrwCount = baseCab.DrwCount,

                DrwFrontHeight1 = h1,
                DrwFrontHeight2 = h2,
                DrwFrontHeight3 = h3,
                DrwFrontHeight4 = h4,

                DefaultDrwFront1Height = DefaultDrwFront1Height ?? "",
                Reason = reason,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(OpeningDrwFrontHeightExceptionRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            Exceptions.Add(row);
            TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private static string BuildReason(bool isStyle2, int drwCount, bool baselineDiffers, bool style2Mismatch)
    {
        if (!isStyle2)
        {
            return baselineDiffers ? "Drw Front 1 differs from default" : "";
        }

        if (baselineDiffers && style2Mismatch)
        {
            return "Drw Front 1 differs from default; bottom drawer fronts not equal";
        }

        if (baselineDiffers)
        {
            return "Drw Front 1 differs from default";
        }

        if (style2Mismatch)
        {
            return drwCount == 3
                ? "Drw Front 2 and 3 must be equal"
                : "Drw Front 2, 3, and 4 must be equal";
        }

        return "";
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
        => Math.Abs(a - b) < 0.0001;

    public sealed partial class OpeningDrwFrontHeightExceptionRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string Style { get; set; } = "";
        [ObservableProperty] public partial int DrwCount { get; set; }

        [ObservableProperty] public partial string DrwFrontHeight1 { get; set; } = "";
        [ObservableProperty] public partial string DrwFrontHeight2 { get; set; } = "";
        [ObservableProperty] public partial string DrwFrontHeight3 { get; set; } = "";
        [ObservableProperty] public partial string DrwFrontHeight4 { get; set; } = "";

        [ObservableProperty] public partial string DefaultDrwFront1Height { get; set; } = "";
        [ObservableProperty] public partial string Reason { get; set; } = "";
    }
}