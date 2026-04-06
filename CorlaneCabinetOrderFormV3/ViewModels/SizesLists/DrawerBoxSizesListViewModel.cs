//using CommunityToolkit.Mvvm.ComponentModel;
//using CorlaneCabinetOrderFormV3.Converters;
//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Services;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.ComponentModel;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public sealed partial class DrawerBoxSizesListViewModel : ObservableObject
//{
//    private readonly ICabinetService _cabinetService;
//    private readonly DefaultSettingsService _defaults;

//    public DrawerBoxSizesListViewModel()
//    {
//        // Parameterless constructor for design-time support
//    }

//    public DrawerBoxSizesListViewModel(ICabinetService cabinetService, DefaultSettingsService defaults)
//    {
//        _cabinetService = cabinetService;
//        _defaults = defaults;

//        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
//        {
//            cc.CollectionChanged += Cabinets_CollectionChanged;
//        }

//        _defaults.PropertyChanged += Defaults_PropertyChanged;

//        HookCabinetItemEvents();
//        Rebuild();
//    }

//    public ObservableCollection<DrawerBoxRow> DrawerBoxSizes { get; } = [];

//    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
//    {
//        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
//        {
//            Rebuild();
//        }
//    }

//    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        UnhookCabinetItemEvents(e.OldItems);
//        HookCabinetItemEvents(e.NewItems);

//        // On Reset (bulk load), NewItems is null — re-hook all current items
//        if (e.Action == NotifyCollectionChangedAction.Reset)
//        {
//            HookCabinetItemEvents();
//        }

//        Rebuild();
//    }


//    private void HookCabinetItemEvents()
//    {
//        foreach (var cab in _cabinetService.Cabinets)
//        {
//            cab.PropertyChanged += Cabinet_PropertyChanged;
//        }
//    }

//    private void HookCabinetItemEvents(System.Collections.IList? newItems)
//    {
//        if (newItems is null) return;

//        foreach (var item in newItems)
//        {
//            if (item is CabinetModel cab)
//            {
//                cab.PropertyChanged += Cabinet_PropertyChanged;
//            }
//        }
//    }

//    private void UnhookCabinetItemEvents(System.Collections.IList? oldItems)
//    {
//        if (oldItems is null) return;

//        foreach (var item in oldItems)
//        {
//            if (item is CabinetModel cab)
//            {
//                cab.PropertyChanged -= Cabinet_PropertyChanged;
//            }
//        }
//    }

//    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
//    {
//        Rebuild();
//    }

//    private string FormatDimension(double value)
//    {
//        var format = _defaults?.DefaultDimensionFormat ?? "Decimal";

//        return string.Equals(format, "Fraction", StringComparison.OrdinalIgnoreCase)
//            ? ConvertDimension.DoubleToFraction(value)
//            : value.ToString("0.####");
//    }

//    public void Rebuild()
//    {
//        DrawerBoxSizes.Clear();

//        _cabinetService.AccumulateAllMaterialAndEdgeTotals();

//        for (int i = 0; i < _cabinetService.Cabinets.Count; i++)
//        {
//            var cab = _cabinetService.Cabinets[i];
//            int cabinetNumber = i + 1;
//            string cabinetName = cab.Name ?? "";

//            foreach (var row in cab.DrawerBoxes)
//            {
//                DrawerBoxSizes.Add(row with
//                {
//                    CabinetNumber = cabinetNumber,
//                    CabinetName = cabinetName,
//                    DisplayHeight = FormatDimension(row.Height),
//                    DisplayWidth = FormatDimension(row.Width),
//                    DisplayLength = FormatDimension(row.Length)
//                });
//            }
//        }
//    }
//}










using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public sealed partial class DrawerBoxSizesListViewModel : ObservableObject
{
    private readonly ICabinetService _cabinetService;
    private readonly DefaultSettingsService _defaults;
    private bool _rebuildQueued;

    public DrawerBoxSizesListViewModel()
    {
        // Parameterless constructor for design-time support
    }

    public DrawerBoxSizesListViewModel(ICabinetService cabinetService, DefaultSettingsService defaults)
    {
        _cabinetService = cabinetService;
        _defaults = defaults;

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += Cabinets_CollectionChanged;
        }

        _defaults.PropertyChanged += Defaults_PropertyChanged;

        HookCabinetItemEvents();
        Rebuild();
    }

    public ObservableCollection<DrawerBoxRow> DrawerBoxSizes { get; } = [];

    private void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefaultSettingsService.DefaultDimensionFormat))
        {
            RequestRebuild();
        }
    }

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UnhookCabinetItemEvents(e.OldItems);
        HookCabinetItemEvents(e.NewItems);

        // On Reset (bulk load), NewItems is null — re-hook all current items
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            HookCabinetItemEvents();
        }

        RequestRebuild();
    }

    private void HookCabinetItemEvents()
    {
        foreach (var cab in _cabinetService.Cabinets)
        {
            cab.PropertyChanged += Cabinet_PropertyChanged;
        }
    }

    private void HookCabinetItemEvents(System.Collections.IList? newItems)
    {
        if (newItems is null) return;

        foreach (var item in newItems)
        {
            if (item is CabinetModel cab)
            {
                cab.PropertyChanged += Cabinet_PropertyChanged;
            }
        }
    }

    private void UnhookCabinetItemEvents(System.Collections.IList? oldItems)
    {
        if (oldItems is null) return;

        foreach (var item in oldItems)
        {
            if (item is CabinetModel cab)
            {
                cab.PropertyChanged -= Cabinet_PropertyChanged;
            }
        }
    }

    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RequestRebuild();
    }

    /// <summary>
    /// Coalesces multiple rapid Rebuild requests into a single deferred pass,
    /// preventing AccumulateAllMaterialAndEdgeTotals from running N times
    /// when the collection changes or multiple cabinet properties fire.
    /// </summary>
    private void RequestRebuild()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null) return;

        if (!dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RequestRebuild));
            return;
        }

        if (_rebuildQueued) return;
        _rebuildQueued = true;

        dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            try { Rebuild(); }
            finally { _rebuildQueued = false; }
        }));
    }

    private string FormatDimension(double value)
    {
        var format = _defaults?.DefaultDimensionFormat ?? "Decimal";

        return string.Equals(format, "Fraction", StringComparison.OrdinalIgnoreCase)
            ? ConvertDimension.DoubleToFraction(value)
            : value.ToString("0.####");
    }

    public void Rebuild()
    {
        DrawerBoxSizes.Clear();

        _cabinetService.AccumulateAllMaterialAndEdgeTotals();

        for (int i = 0; i < _cabinetService.Cabinets.Count; i++)
        {
            var cab = _cabinetService.Cabinets[i];
            int cabinetNumber = i + 1;
            string cabinetName = cab.Name ?? "";

            foreach (var row in cab.DrawerBoxes)
            {
                DrawerBoxSizes.Add(row with
                {
                    CabinetNumber = cabinetNumber,
                    CabinetName = cabinetName,
                    DisplayHeight = FormatDimension(row.Height),
                    DisplayWidth = FormatDimension(row.Width),
                    DisplayLength = FormatDimension(row.Length)
                });
            }
        }
    }
}