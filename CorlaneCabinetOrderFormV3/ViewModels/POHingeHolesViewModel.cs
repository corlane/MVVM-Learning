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

public partial class POHingeHolesViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POHingeHolesViewModel()
    {
        // design-time support
        DefaultDrillHingeHoles = true;
        UpdateTabHeaderBrush();
    }

    public POHingeHolesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultDrillHingeHoles = true;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial bool DefaultDrillHingeHoles { get; set; } = true;

    public ObservableCollection<HingeHolesChangeRow> HingeHolesToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultDrillHingeHolesChanged(bool value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        HingeHolesToChange.Clear();
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

            bool drillHingeHoles = cab switch
            {
                BaseCabinetModel b => b.DrillHingeHoles,
                UpperCabinetModel u => u.DrillHingeHoles,
                _ => DefaultDrillHingeHoles
            };

            if (drillHingeHoles == DefaultDrillHingeHoles)
            {
                continue;
            }

            var row = new HingeHolesChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                DrillHingeHoles = drillHingeHoles,
                DefaultDrillHingeHoles = DefaultDrillHingeHoles,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HingeHolesChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            HingeHolesToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (HingeHolesToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = HingeHolesToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class HingeHolesChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";

        [ObservableProperty] public partial bool DrillHingeHoles { get; set; }
        [ObservableProperty] public partial bool DefaultDrillHingeHoles { get; set; }
    }
}