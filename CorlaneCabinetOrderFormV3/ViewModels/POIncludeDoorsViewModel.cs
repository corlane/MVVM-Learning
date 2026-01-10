using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POIncludeDoorsViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POIncludeDoorsViewModel()
    {
        // design-time support
        DefaultIncDoors = false;
        UpdateTabHeaderBrush();
    }

    public POIncludeDoorsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultIncDoors = false;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial bool DefaultIncDoors { get; set; } = false;

    public ObservableCollection<IncDoorsChangeRow> DoorsToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultIncDoorsChanged(bool value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        DoorsToChange.Clear();
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

            bool incDoors = cab switch
            {
                BaseCabinetModel b => b.IncDoors,
                UpperCabinetModel u => u.IncDoors,
                _ => DefaultIncDoors
            };

            if (incDoors == DefaultIncDoors)
            {
                continue;
            }

            // Check for drawer fronts (BaseCabinetModel only)
            if (cab is BaseCabinetModel baseCab && baseCab.DrwCount > 0)
            {
                // Add cabinet-level row for doors  
                var mainRow = new IncDoorsChangeRow
                {
                    CabinetNumber = cabNumber,
                    CabinetName = cab.Name ?? "",
                    Type = "Doors",
                    IncDoors = incDoors,
                    DefaultIncDoors = DefaultIncDoors,
                    IsDone = false
                };

                mainRow.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(IncDoorsChangeRow.IsDone))
                    {
                        UpdateTabHeaderBrush();
                    }
                };

                DoorsToChange.Add(mainRow);

                // Add individual drawer front rows
                for (int i = 1; i <= baseCab.DrwCount; i++)
                {
                    bool incDrwFront = i switch
                    {
                        1 => baseCab.IncDrwFront1,
                        2 => baseCab.IncDrwFront2,
                        3 => baseCab.IncDrwFront3,
                        4 => baseCab.IncDrwFront4,
                        _ => DefaultIncDoors
                    };

                    if (incDrwFront == DefaultIncDoors)
                    {
                        continue;
                    }

                    var drwRow = new IncDoorsChangeRow
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = cab.Name ?? "",
                        Type = $"Drawer Front {i}",
                        IncDoors = incDrwFront,
                        DefaultIncDoors = DefaultIncDoors,
                        IsDone = false
                    };

                    drwRow.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == nameof(IncDoorsChangeRow.IsDone))
                        {
                            UpdateTabHeaderBrush();
                        }
                    };

                    DoorsToChange.Add(drwRow);
                }

                TotalCabsNeedingChange += Math.Max(1, cab.Qty);
            }
            else
            {
                // No drawer fronts, just add the cabinet
                var row = new IncDoorsChangeRow
                {
                    CabinetNumber = cabNumber,
                    CabinetName = cab.Name ?? "",
                    Type = "Doors",
                    IncDoors = incDoors,
                    DefaultIncDoors = DefaultIncDoors,
                    IsDone = false
                };

                row.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(IncDoorsChangeRow.IsDone))
                    {
                        UpdateTabHeaderBrush();
                    }
                };

                DoorsToChange.Add(row);
                TotalCabsNeedingChange += Math.Max(1, cab.Qty);
            }
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (DoorsToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = DoorsToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class IncDoorsChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string Type { get; set; } = "";

        [ObservableProperty] public partial bool IncDoors { get; set; }
        [ObservableProperty] public partial bool DefaultIncDoors { get; set; }
    }
}