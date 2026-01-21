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
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
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

            int drwCount = baseCab.DrwCount;
            if (drwCount <= 0)
            {
                continue;
            }

            // Drawer front heights (used for comparisons)
            double dh1 = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight1 ?? "");
            double dh2 = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight2 ?? "");
            double dh3 = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight3 ?? "");
            double dh4 = ConvertDimension.FractionToDouble(baseCab.DrwFrontHeight4 ?? "");

            bool oh1NeedsChange = false;
            bool oh2NeedsChange = false;
            bool oh3NeedsChange = false;
            bool oh4NeedsChange = false;

            string reason = "";

            if (isStyle1)
            {
                // Style1 + DrwCount > 0 + DF1 != default => flag OH1
                if (!NearlyEqual(dh1, defaultFront1))
                {
                    oh1NeedsChange = true;
                    reason = "Style1: Drw Front 1 differs from default";
                }
            }
            else if (isStyle2)
            {
                if (drwCount == 2)
                {
                    // DF1 and DF2 must be equal => if not, flag OH1, OH2
                    if (!NearlyEqual(dh1, dh2))
                    {
                        oh1NeedsChange = true;
                        oh2NeedsChange = true;
                        reason = "Style2 (2 drawers): Drw Front 1 and 2 must be equal";
                    }
                }
                else if (drwCount == 3)
                {
                    // DF1 != default OR DF2 and DF3 not equal
                    bool df1Differs = !NearlyEqual(dh1, defaultFront1);
                    bool df23Differs = !NearlyEqual(dh2, dh3);

                    if (df1Differs)
                    {
                        oh1NeedsChange = true;
                    }

                    if (df23Differs)
                    {
                        oh2NeedsChange = true;
                        oh3NeedsChange = true;
                    }

                    if (df1Differs && df23Differs)
                    {
                        reason = "Style2 (3 drawers): Drw Front 1 differs from default; Drw Front 2 and 3 must be equal";
                    }
                    else if (df1Differs)
                    {
                        reason = "Style2 (3 drawers): Drw Front 1 differs from default";
                    }
                    else if (df23Differs)
                    {
                        reason = "Style2 (3 drawers): Drw Front 2 and 3 must be equal";
                    }
                }
                else if (drwCount == 4)
                {
                    // DF1 != default OR DF2/DF3/DF4 not all equal
                    bool df1Differs = !NearlyEqual(dh1, defaultFront1);
                    bool df234NotAllEqual = !NearlyEqual(dh2, dh3) || !NearlyEqual(dh3, dh4);

                    if (df1Differs)
                    {
                        oh1NeedsChange = true;
                    }

                    if (df234NotAllEqual)
                    {
                        oh2NeedsChange = true;
                        oh3NeedsChange = true;
                        oh4NeedsChange = true;
                    }

                    if (df1Differs && df234NotAllEqual)
                    {
                        reason = "Style2 (4 drawers): Drw Front 1 differs from default; Drw Front 2, 3, and 4 must be equal";
                    }
                    else if (df1Differs)
                    {
                        reason = "Style2 (4 drawers): Drw Front 1 differs from default";
                    }
                    else if (df234NotAllEqual)
                    {
                        reason = "Style2 (4 drawers): Drw Front 2, 3, and 4 must be equal";
                    }
                }
            }

            if (!oh1NeedsChange && !oh2NeedsChange && !oh3NeedsChange && !oh4NeedsChange)
            {
                continue;
            }

            // Flagged cabinet: show ALL opening heights (not just the ones related to failing drawer fronts).
            var row = new OpeningDrwFrontHeightExceptionRow
            {
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                Style = baseCab.Style ?? "",
                DrwCount = drwCount,

                OpeningHeight1 = baseCab.OpeningHeight1 ?? "",
                OpeningHeight2 = baseCab.OpeningHeight2 ?? "",
                OpeningHeight3 = baseCab.OpeningHeight3 ?? "",
                OpeningHeight4 = baseCab.OpeningHeight4 ?? "",

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

        // Replaces DF1..DF4 in the grid
        [ObservableProperty] public partial string OpeningHeight1 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight2 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight3 { get; set; } = "";
        [ObservableProperty] public partial string OpeningHeight4 { get; set; } = "";

        [ObservableProperty] public partial string DefaultDrwFront1Height { get; set; } = "";
        [ObservableProperty] public partial string Reason { get; set; } = "";
    }
}