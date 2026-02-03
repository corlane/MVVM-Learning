using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POCabinetSpeciesViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public POCabinetSpeciesViewModel()
    {
        // design-time support
        DefaultCabinetSpecies = "Prefinished Ply";
        UpdateTabHeaderBrush();
    }

    public POCabinetSpeciesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        DefaultCabinetSpecies = "Prefinished Ply";

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    [ObservableProperty]
    public partial string DefaultCabinetSpecies { get; set; } = "Prefinished Ply";

    public ObservableCollection<CabinetSpeciesChangeRow> CabinetSpeciesToChange { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    partial void OnDefaultCabinetSpeciesChanged(string value) => Refresh();

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        CabinetSpeciesToChange.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        var defaultSpecies = (DefaultCabinetSpecies ?? "").Trim();

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            var cabSpecies = (cab.Species ?? "").Trim();

            // If species is the literal "Custom", prefer the user-entered custom species if present.
            if (string.Equals(cabSpecies, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                var custom = (cab.CustomSpecies ?? "").Trim();
                if (!string.IsNullOrEmpty(custom))
                {
                    cabSpecies = custom;
                }
            }

            if (string.Equals(cabSpecies, defaultSpecies, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new CabinetSpeciesChangeRow
            {
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetSpecies = cabSpecies,
                DefaultCabinetSpecies = defaultSpecies,
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CabinetSpeciesChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            CabinetSpeciesToChange.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (CabinetSpeciesToChange.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = CabinetSpeciesToChange.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class CabinetSpeciesChangeRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetSpecies { get; set; } = "";
        [ObservableProperty] public partial string DefaultCabinetSpecies { get; set; } = "";
    }
}