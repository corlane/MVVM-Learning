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

public partial class POEdgebandingViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = new(Color.FromRgb(146, 250, 153));
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POEdgebandingViewModel()
    {
        // design-time support
        DefaultEbSpecies = "Wood Maple";
        UpdateTabHeaderBrush();
    }

    public POEdgebandingViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultEbSpecies = "Wood Maple";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultEbSpecies { get; set; } = "Wood Maple";

    public ObservableCollection<EdgebandingChangeRow> EdgebandingToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultEbSpeciesChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        EdgebandingToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        var defaultSpecies = (DefaultEbSpecies ?? "").Trim();

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            var cabSpecies = (cab.EBSpecies ?? "").Trim();

            // Treat blank as "None" to match other parts of the app.
            if (string.IsNullOrWhiteSpace(cabSpecies))
            {
                cabSpecies = "None";
            }

            if (string.Equals(cabSpecies, defaultSpecies, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new EdgebandingChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                EbSpecies = cabSpecies,
                DefaultEbSpecies = defaultSpecies,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EdgebandingChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            EdgebandingToChange.Add(row);

            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (EdgebandingToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = EdgebandingToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class EdgebandingChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string EbSpecies { get; set; } = "";
        [ObservableProperty] public partial string DefaultEbSpecies { get; set; } = "";
    }
}