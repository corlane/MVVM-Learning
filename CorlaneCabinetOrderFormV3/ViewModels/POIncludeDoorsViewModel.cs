using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POIncludeDoorsViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    private bool _refreshQueued;

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
            cc.CollectionChanged += Cabinets_CollectionChanged;

            // Also track existing cabinet property changes so the exception list stays live.
            foreach (var cab in _cabinetService.Cabinets)
            {
                HookCabinet(cab);
            }
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

    partial void OnDefaultIncDoorsChanged(bool value) => RequestRefresh();

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var ni in e.NewItems)
            {
                if (ni is CabinetModel cab)
                {
                    HookCabinet(cab);
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (var oi in e.OldItems)
            {
                if (oi is CabinetModel cab)
                {
                    UnhookCabinet(cab);
                }
            }
        }

        RequestRefresh();
    }

    private void HookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
        {
            // Listen for all properties; we filter in the handler.
            PropertyChangedEventManager.AddHandler(inpc, Cabinet_PropertyChanged, string.Empty);
        }
    }

    private void UnhookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
        {
            PropertyChangedEventManager.RemoveHandler(inpc, Cabinet_PropertyChanged, string.Empty);
        }
    }

    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Keep this focused to the properties that affect this list.
        if (e.PropertyName is nameof(BaseCabinetModel.IncDoors)
            or nameof(UpperCabinetModel.IncDoors)
            or nameof(BaseCabinetModel.DrwCount)
            or nameof(BaseCabinetModel.IncDrwFront1)
            or nameof(BaseCabinetModel.IncDrwFront2)
            or nameof(BaseCabinetModel.IncDrwFront3)
            or nameof(BaseCabinetModel.IncDrwFront4)
            or nameof(CabinetModel.Name)
            or nameof(CabinetModel.Qty))
        {
            RequestRefresh();
        }
    }

    private void RequestRefresh()
    {
        if (Application.Current?.Dispatcher == null)
        {
            Refresh();
            return;
        }

        if (_refreshQueued) return;

        _refreshQueued = true;
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _refreshQueued = false;
            Refresh();
        }, DispatcherPriority.Background);
    }

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

        void TrackRow(IncDoorsChangeRow row)
        {
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IncDoorsChangeRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            DoorsToChange.Add(row);
        }

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;
            bool anyRowsAddedForCab = false;

            // Doors exception (Base/Upper only)
            bool incDoors = cab switch
            {
                BaseCabinetModel b => b.IncDoors,
                UpperCabinetModel u => u.IncDoors,
                _ => DefaultIncDoors
            };

            bool isDoorException = cab is BaseCabinetModel or UpperCabinetModel
                && incDoors != DefaultIncDoors;

            if (isDoorException)
            {
                TrackRow(new IncDoorsChangeRow
                {
                    CabinetNumber = cabNumber,
                    CabinetName = cab.Name ?? "",
                    Type = "Doors",
                    IncDoors = incDoors,
                    DefaultIncDoors = DefaultIncDoors,
                    IsDone = false
                });

                anyRowsAddedForCab = true;
            }

            // Drawer front exceptions (BaseCabinetModel only) - independent of doors exception
            if (cab is BaseCabinetModel baseCab && baseCab.DrwCount > 0)
            {
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

                    TrackRow(new IncDoorsChangeRow
                    {
                        CabinetNumber = cabNumber,
                        CabinetName = cab.Name ?? "",
                        Type = $"Drawer Front {i}",
                        IncDoors = incDrwFront,
                        DefaultIncDoors = DefaultIncDoors,
                        IsDone = false
                    });

                    anyRowsAddedForCab = true;
                }
            }

            if (anyRowsAddedForCab)
            {
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