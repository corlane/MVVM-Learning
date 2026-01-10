using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PODoorDrwGrainDirViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;
    private readonly DefaultSettingsService? _defaults;

    public PODoorDrwGrainDirViewModel()
    {
        // design-time
        DefaultDoorGrainDir = "Vertical";
        DefaultDrwFrontGrainDir = "Horizontal";
        UpdateTabHeaderBrush();
    }

    public PODoorDrwGrainDirViewModel(ICabinetService cabinetService, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _defaults = defaults;

        DefaultDoorGrainDir = _defaults.DefaultDoorGrainDir;
        DefaultDrwFrontGrainDir = _defaults.DefaultDrwGrainDir;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        _defaults.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DefaultSettingsService.DefaultDoorGrainDir))
            {
                DefaultDoorGrainDir = _defaults.DefaultDoorGrainDir;
            }
            else if (e.PropertyName == nameof(DefaultSettingsService.DefaultDrwGrainDir))
            {
                DefaultDrwFrontGrainDir = _defaults.DefaultDrwGrainDir;
            }
        };

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDoorGrainDir { get; set; } = "Vertical";
    partial void OnDefaultDoorGrainDirChanged(string value) => Refresh();

    [ObservableProperty]
    public partial string DefaultDrwFrontGrainDir { get; set; } = "Horizontal";
    partial void OnDefaultDrwFrontGrainDirChanged(string value) => Refresh();

    public ObservableCollection<GrainDirExceptionRow> Exceptions { get; } = new();

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

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;
            string cabName = cab.Name ?? "";

            if (cab is BaseCabinetModel baseCab)
            {
                AddDoorExceptionIfNeeded(cabNumber, cabName, baseCab.IncDoors, baseCab.DoorCount, baseCab.DoorGrainDir, baseCab.Qty);

                bool anyFrontIncluded = baseCab.IncDrwFront1 || baseCab.IncDrwFront2 || baseCab.IncDrwFront3 || baseCab.IncDrwFront4;
                if (anyFrontIncluded)
                {
                    AddDrawerFrontExceptionIfNeeded(cabNumber, cabName, baseCab.DrwFrontGrainDir, baseCab.Qty);
                }
            }
            else if (cab is UpperCabinetModel upperCab)
            {
                AddDoorExceptionIfNeeded(cabNumber, cabName, upperCab.IncDoors, upperCab.DoorCount, upperCab.DoorGrainDir, upperCab.Qty);
            }
        }

        UpdateTabHeaderBrush();
    }

    private void AddDoorExceptionIfNeeded(int cabNumber, string cabName, bool incDoors, int doorCount, string? actualDoorGrainDir, int qty)
    {
        if (!incDoors || doorCount <= 0)
        {
            return;
        }

        string expected = NormalizeToDefault(DefaultDoorGrainDir, DefaultDoorGrainDir);
        string actual = NormalizeToDefault(actualDoorGrainDir, expected);

        if (string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AddRow(cabNumber, cabName, "Door(s)", actual, expected, qty);
    }

    private void AddDrawerFrontExceptionIfNeeded(int cabNumber, string cabName, string? actualDrwFrontGrainDir, int qty)
    {
        string expected = NormalizeToDefault(DefaultDrwFrontGrainDir, DefaultDrwFrontGrainDir);
        string actual = NormalizeToDefault(actualDrwFrontGrainDir, expected);

        if (string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AddRow(cabNumber, cabName, "Drawer Front(s)", actual, expected, qty);
    }

    private void AddRow(int cabinetNumber, string cabinetName, string partType, string actual, string expected, int qty)
    {
        var row = new GrainDirExceptionRow
        {
            CabinetNumber = cabinetNumber,
            CabinetName = cabinetName,
            PartType = partType,
            ActualGrainDir = actual,
            DefaultGrainDir = expected,
            IsDone = false
        };

        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(GrainDirExceptionRow.IsDone))
            {
                UpdateTabHeaderBrush();
            }
        };

        Exceptions.Add(row);
        TotalCabsNeedingChange += System.Math.Max(1, qty);
    }

    private static string NormalizeToDefault(string? actualMaybeBlank, string defaultValue)
        => string.IsNullOrWhiteSpace(actualMaybeBlank) ? defaultValue : actualMaybeBlank.Trim();

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

    public sealed partial class GrainDirExceptionRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }
        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string PartType { get; set; } = "";
        [ObservableProperty] public partial string ActualGrainDir { get; set; } = "";
        [ObservableProperty] public partial string DefaultGrainDir { get; set; } = "";
    }
}