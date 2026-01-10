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

public partial class PODoorSpeciesViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public PODoorSpeciesViewModel()
    {
        // design-time support
        DefaultDoorSpecies = "Maple Ply";
        UpdateTabHeaderBrush();
    }

    public PODoorSpeciesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDoorSpecies = "Maple Ply";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultDoorSpecies { get; set; } = "Maple Ply";

    public ObservableCollection<DoorSpeciesChangeRow> DoorSpeciesToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultDoorSpeciesChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        DoorSpeciesToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        var defaultSpecies = (DefaultDoorSpecies ?? "").Trim();

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            // Check if this cabinet has doors or drawer fronts included
            bool hasDoorOrDrawerFront = false;
            string doorSpecies = "";

            if (cab is BaseCabinetModel baseCab)
            {
                hasDoorOrDrawerFront = baseCab.IncDoors ||
                                       baseCab.IncDrwFront1 ||
                                       baseCab.IncDrwFront2 ||
                                       baseCab.IncDrwFront3 ||
                                       baseCab.IncDrwFront4;
                doorSpecies = (baseCab.DoorSpecies ?? "").Trim();
            }
            else if (cab is UpperCabinetModel upperCab)
            {
                hasDoorOrDrawerFront = upperCab.IncDoors;
                doorSpecies = (upperCab.DoorSpecies ?? "").Trim();
            }

            // Skip if no doors/drawer fronts are included
            if (!hasDoorOrDrawerFront)
            {
                continue;
            }

            // Skip if species matches default
            if (string.Equals(doorSpecies, defaultSpecies, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new DoorSpeciesChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                DoorSpecies = doorSpecies,
                DefaultDoorSpecies = defaultSpecies,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DoorSpeciesChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            DoorSpeciesToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (DoorSpeciesToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = DoorSpeciesToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class DoorSpeciesChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
        [ObservableProperty] public partial string DefaultDoorSpecies { get; set; } = "";
    }
}