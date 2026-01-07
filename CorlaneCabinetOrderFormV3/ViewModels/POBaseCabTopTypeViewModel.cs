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

public partial class POBaseCabTopTypeViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POBaseCabTopTypeViewModel()
    {
        // design-time support
        DefaultTopType = "Stretcher";
        UpdateTabHeaderBrush();
    }

    public POBaseCabTopTypeViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultTopType = "Stretcher";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultTopType { get; set; } = "Stretcher";

    public ObservableCollection<BaseCabTopTypeChangeRow> BaseCabTopTypesToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultTopTypeChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        BaseCabTopTypesToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        var defaultTopType = (DefaultTopType ?? "").Trim();

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            if (cab is not BaseCabinetModel baseCab)
            {
                continue;
            }

            cabNumber++;

            var cabTopType = (baseCab.TopType ?? "").Trim();

            if (string.Equals(cabTopType, defaultTopType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new BaseCabTopTypeChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = baseCab.Name ?? "",
                TopType = cabTopType,
                DefaultTopType = defaultTopType,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BaseCabTopTypeChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            BaseCabTopTypesToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, baseCab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (BaseCabTopTypesToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = BaseCabTopTypesToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class BaseCabTopTypeChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string TopType { get; set; } = "";
        [ObservableProperty] public partial string DefaultTopType { get; set; } = "";
    }
}