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

public partial class PODrawerBoxesViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public PODrawerBoxesViewModel()
    {
        // design-time support
        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultMinDrawerBoxHeight = "4";
        UpdateTabHeaderBrush();
    }

    public PODrawerBoxesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultMinDrawerBoxHeight = "4";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDrwStyle { get; set; } = "Blum Tandem H/Equivalent Undermount";
    partial void OnDefaultDrwStyleChanged(string value) => Refresh();

    [ObservableProperty]
    public partial string DefaultMinDrawerBoxHeight { get; set; } = "4";
    partial void OnDefaultMinDrawerBoxHeightChanged(string value) => Refresh();

    public ObservableCollection<DrawerBoxExceptionRow> Exceptions { get; } = new();

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

        Exceptions.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService is null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        string defaultStyle = (DefaultDrwStyle ?? "").Trim();
        double minHeight = ConvertDimension.FractionToDouble(DefaultMinDrawerBoxHeight ?? "");

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            cabNumber++;

            // Only drawer boxes coming from base cabinets with IncDrwBoxes == true.
            if (!baseCab.IncDrwBoxes)
            {
                continue;
            }

            bool hasDrawerBoxes = baseCab.DrwCount > 0 || cab.DrawerBoxes.Count > 0;

            // Default #1: DrwStyle differs from a required value (only when cabinet actually has drawer boxes)
            if (hasDrawerBoxes)
            {
                string actualStyle = (baseCab.DrwStyle ?? "").Trim();

                if (!string.Equals(actualStyle, defaultStyle, StringComparison.OrdinalIgnoreCase))
                {
                    AddRow(new DrawerBoxExceptionRow
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = baseCab.Name ?? "",
                        ExceptionType = "Drawer Slide Type",
                        DrawerBoxType = "",
                        Actual = actualStyle,
                        Default = defaultStyle,
                        IsDone = false
                    }, baseCab.Qty);
                }
            }

            // Default #2: ANY drawer box whose height is less than the default flags
            foreach (var box in cab.DrawerBoxes)
            {
                if (box.Height >= minHeight)
                {
                    continue;
                }

                AddRow(new DrawerBoxExceptionRow
                {
                    CabinetNumber = cabNumber,
                    CabinetName = baseCab.Name ?? "",
                    ExceptionType = "Drawer Box Height",
                    DrawerBoxType = box.Type ?? "",
                    Actual = FormatDecimal(box.Height),
                    Default = DefaultMinDrawerBoxHeight ?? "",
                    IsDone = false
                }, baseCab.Qty);
            }
        }

        UpdateTabHeaderBrush();
    }

    private void AddRow(DrawerBoxExceptionRow row, int qty)
    {
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DrawerBoxExceptionRow.IsDone))
            {
                UpdateTabHeaderBrush();
            }
        };

        Exceptions.Add(row);
        TotalCabsNeedingChange += Math.Max(1, qty);
    }

    private static string FormatDecimal(double value)
        => value.ToString("0.####");

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

    public sealed partial class DrawerBoxExceptionRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";

        [ObservableProperty] public partial string ExceptionType { get; set; } = "";
        [ObservableProperty] public partial string DrawerBoxType { get; set; } = "";

        [ObservableProperty] public partial string Actual { get; set; } = "";
        [ObservableProperty] public partial string Default { get; set; } = "";
    }
}