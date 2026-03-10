//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Services;
//using System;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public partial class PODrawerBoxesViewModel : ObservableObject
//{
//    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
//    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

//    private readonly ICabinetService? _cabinetService;

//    public PODrawerBoxesViewModel()
//    {
//        // design-time support
//        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
//        UpdateTabHeaderBrush();
//    }

//    public PODrawerBoxesViewModel(ICabinetService cabinetService)
//    {
//        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

//        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";

//        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
//        {
//            cc.CollectionChanged += (_, __) => Refresh();
//        }

//        Refresh();
//    }

//    [ObservableProperty]
//    public partial string DefaultDrwStyle { get; set; } = "Blum Tandem H/Equivalent Undermount";
//    partial void OnDefaultDrwStyleChanged(string value) => Refresh();

//    public ObservableCollection<DrawerBoxExceptionRow> Exceptions { get; } = new();

//    [ObservableProperty]
//    public partial int TotalCabsNeedingChange { get; set; }

//    [ObservableProperty]
//    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

//    public void Refresh()
//    {
//        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
//        {
//            Application.Current.Dispatcher.Invoke(Refresh);
//            return;
//        }

//        Exceptions.Clear();
//        TotalCabsNeedingChange = 0;

//        if (_cabinetService is null)
//        {
//            UpdateTabHeaderBrush();
//            return;
//        }

//        string defaultStyle = (DefaultDrwStyle ?? "").Trim();

//        int cabNumber = 0;

//        foreach (var cab in _cabinetService.Cabinets)
//        {
//            cabNumber++;

//            if (cab is not BaseCabinetModel baseCab)
//            {
//                continue;
//            }

//            // Only drawer boxes coming from base cabinets with IncDrwBoxes == true.
//            if (!baseCab.IncDrwBoxes)
//            {
//                continue;
//            }

//            bool hasDrawerBoxes = baseCab.DrwCount > 0 || cab.DrawerBoxes.Count > 0;

//            // Flag when cabinet actually has drawer boxes and slide type differs from required value.
//            if (!hasDrawerBoxes)
//            {
//                continue;
//            }

//            string actualStyle = (baseCab.DrwStyle ?? "").Trim();

//            if (!string.Equals(actualStyle, defaultStyle, StringComparison.OrdinalIgnoreCase))
//            {
//                AddRow(new DrawerBoxExceptionRow
//                {
//                    CabinetNumber = cabNumber,
//                    CabinetName = baseCab.Name ?? "",
//                    ExceptionType = "Drawer Slide Type",
//                    DrawerBoxType = "",
//                    Actual = actualStyle,
//                    Default = defaultStyle,
//                    IsDone = false
//                }, baseCab.Qty);
//            }
//        }

//        UpdateTabHeaderBrush();
//    }

//    [RelayCommand]
//    private void RefreshList() => Refresh();

//    private void AddRow(DrawerBoxExceptionRow row, int qty)
//    {
//        row.PropertyChanged += (_, e) =>
//        {
//            if (e.PropertyName == nameof(DrawerBoxExceptionRow.IsDone))
//            {
//                UpdateTabHeaderBrush();
//            }
//        };

//        Exceptions.Add(row);
//        TotalCabsNeedingChange += Math.Max(1, qty);
//    }

//    private void UpdateTabHeaderBrush()
//    {
//        if (Exceptions.Count == 0)
//        {
//            TabHeaderBrush = s_okGreen;
//            return;
//        }

//        bool allDone = Exceptions.All(r => r.IsDone);
//        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
//    }

//    public sealed partial class DrawerBoxExceptionRow : ObservableObject
//    {
//        [ObservableProperty] public partial bool IsDone { get; set; }

//        [ObservableProperty] public partial int CabinetNumber { get; set; }
//        [ObservableProperty] public partial string CabinetName { get; set; } = "";

//        [ObservableProperty] public partial string ExceptionType { get; set; } = "";
//        [ObservableProperty] public partial string DrawerBoxType { get; set; } = "";

//        [ObservableProperty] public partial string Actual { get; set; } = "";
//        [ObservableProperty] public partial string Default { get; set; } = "";
//    }
//}



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public PODrawerBoxesViewModel()
    {
        // design-time support
        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultIncDrwBoxes = true;
        UpdateTabHeaderBrush();
    }

    public PODrawerBoxesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrwStyle = "Blum Tandem H/Equivalent Undermount";
        DefaultIncDrwBoxes = true;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDrwStyle { get; set; } = "Blum Tandem H/Equivalent Undermount";
    partial void OnDefaultDrwStyleChanged(string value) => Refresh();

    // Invert comparison by toggling this (default = true).
    [ObservableProperty]
    public partial bool DefaultIncDrwBoxes { get; set; } = true;
    partial void OnDefaultIncDrwBoxesChanged(bool value) => Refresh();

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

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            bool anyRowsAddedForCab = false;

            int drwCount = Math.Clamp(baseCab.DrwCount, 0, 4);

            // IncDrwBoxes (per-opening): when an opening doesn't match the default, flag it.
            // Default = true => exceptions when an opening is false (remove drawer box).
            // Default = false => exceptions when an opening is true (add drawer box).
            if (drwCount > 0)
            {
                for (int opening = 1; opening <= drwCount; opening++)
                {
                    bool inc = GetIncDrwBoxForOpening(baseCab, opening);

                    if (inc == DefaultIncDrwBoxes)
                    {
                        continue;
                    }

                    TrackRow(new DrawerBoxExceptionRow
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = baseCab.Name ?? "",
                        ExceptionType = "Drawer Box Include",
                        DrawerBoxType = $"Opening {opening}",
                        Actual = IncludeLabel(inc),
                        Default = IncludeLabel(DefaultIncDrwBoxes),
                        IsDone = false
                    });

                    anyRowsAddedForCab = true;
                }
            }

            // Drawer slide type: only relevant if at least one drawer box is included.
            bool anyDrawerBoxIncluded = drwCount > 0
                && Enumerable.Range(1, drwCount).Any(o => GetIncDrwBoxForOpening(baseCab, o));

            if (anyDrawerBoxIncluded)
            {
                string actualStyle = (baseCab.DrwStyle ?? "").Trim();

                if (!string.Equals(actualStyle, defaultStyle, StringComparison.OrdinalIgnoreCase))
                {
                    TrackRow(new DrawerBoxExceptionRow
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = baseCab.Name ?? "",
                        ExceptionType = "Drawer Slide Type",
                        DrawerBoxType = "",
                        Actual = actualStyle,
                        Default = defaultStyle,
                        IsDone = false
                    });

                    anyRowsAddedForCab = true;
                }
            }

            if (anyRowsAddedForCab)
            {
                TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
            }
        }

        UpdateTabHeaderBrush();
    }

    [RelayCommand]
    private void RefreshList() => Refresh();

    private void TrackRow(DrawerBoxExceptionRow row)
    {
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DrawerBoxExceptionRow.IsDone))
            {
                UpdateTabHeaderBrush();
            }
        };

        Exceptions.Add(row);
    }

    private static bool GetIncDrwBoxForOpening(BaseCabinetModel baseCab, int opening) => opening switch
    {
        1 => baseCab.IncDrwBoxOpening1,
        2 => baseCab.IncDrwBoxOpening2,
        3 => baseCab.IncDrwBoxOpening3,
        4 => baseCab.IncDrwBoxOpening4,
        _ => false
    };

    private static string IncludeLabel(bool include) => include ? "Include" : "Remove";

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