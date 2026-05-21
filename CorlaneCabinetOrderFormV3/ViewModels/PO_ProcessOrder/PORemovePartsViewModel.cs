using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PORemovePartsViewModel : ObservableObject
{
    private const string TabId = "RemoveParts";

    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));
    private static readonly SolidColorBrush s_allDoneBlue = new(Color.FromRgb(135, 206, 250));

    private readonly ICabinetService? _cabinetService;

    private bool _refreshQueued;
    private bool _isRefreshing;

    public PORemovePartsViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public PORemovePartsViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += Cabinets_CollectionChanged;

            foreach (var cab in _cabinetService.Cabinets)
                HookCabinet(cab);
        }

        Refresh();
    }

    public ObservableCollection<RemovePartsExceptionRow> Exceptions { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingChange { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    private void Cabinets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (var ni in e.NewItems)
                if (ni is CabinetModel cab) HookCabinet(cab);

        if (e.OldItems != null)
            foreach (var oi in e.OldItems)
                if (oi is CabinetModel cab) UnhookCabinet(cab);

        if (e.Action == NotifyCollectionChangedAction.Reset && _cabinetService?.Cabinets != null)
            foreach (var cab in _cabinetService.Cabinets)
                HookCabinet(cab);

        RequestRefresh();
    }

    private void HookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
            PropertyChangedEventManager.AddHandler(inpc, Cabinet_PropertyChanged, string.Empty);
    }

    private void UnhookCabinet(CabinetModel cab)
    {
        if (cab is INotifyPropertyChanged inpc)
            PropertyChangedEventManager.RemoveHandler(inpc, Cabinet_PropertyChanged, string.Empty);
    }

    private void Cabinet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BaseCabinetModel.HasLeftEnd)
            or nameof(BaseCabinetModel.HasRightEnd)
            or nameof(BaseCabinetModel.HasTop)
            or nameof(BaseCabinetModel.HasDeck)
            or nameof(BaseCabinetModel.HasBack)
            or nameof(BaseCabinetModel.HasToeKickBoard)
            or nameof(UpperCabinetModel.HasLeftEnd)
            or nameof(UpperCabinetModel.HasRightEnd)
            or nameof(UpperCabinetModel.HasTop)
            or nameof(UpperCabinetModel.HasDeck)
            or nameof(UpperCabinetModel.HasBack)
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

    private static bool IsFlagged(CabinetModel cab)
    {
        return cab switch
        {
            BaseCabinetModel b => !b.HasLeftEnd || !b.HasRightEnd || !b.HasTop || !b.HasDeck || !b.HasBack || !b.HasToeKickBoard,
            UpperCabinetModel u => !u.HasLeftEnd || !u.HasRightEnd || !u.HasTop || !u.HasDeck || !u.HasBack,
            _ => false
        };
    }

    private static string DescribeRemovedParts(CabinetModel cab)
    {
        var parts = new List<string>();

        bool leftEnd, rightEnd, top, deck, back, toeKick;

        switch (cab)
        {
            case BaseCabinetModel b:
                leftEnd = b.HasLeftEnd;
                rightEnd = b.HasRightEnd;
                top = b.HasTop;
                deck = b.HasDeck;
                back = b.HasBack;
                toeKick = b.HasToeKickBoard;
                if (!leftEnd) parts.Add("Left End");
                if (!rightEnd) parts.Add("Right End");
                if (!top) parts.Add("Top");
                if (!deck) parts.Add("Deck");
                if (!back) parts.Add("Back");
                if (!toeKick) parts.Add("Toekick Board");
                break;

            case UpperCabinetModel u:
                leftEnd = u.HasLeftEnd;
                rightEnd = u.HasRightEnd;
                top = u.HasTop;
                deck = u.HasDeck;
                back = u.HasBack;
                if (!leftEnd) parts.Add("Left End");
                if (!rightEnd) parts.Add("Right End");
                if (!top) parts.Add("Top");
                if (!deck) parts.Add("Deck");
                if (!back) parts.Add("Back");
                break;
        }

        return string.Join(", ", parts);
    }

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        _isRefreshing = true;
        SnapshotDoneKeys();

        Exceptions.Clear();
        TotalCabsNeedingChange = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            _isRefreshing = false;
            return;
        }

        var savedKeys = _cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set) ? set : null;

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            if (cab is not (BaseCabinetModel or UpperCabinetModel))
                continue;

            if (!IsFlagged(cab))
                continue;

            var row = new RemovePartsExceptionRow
            {
                CabinetId = cab.Id,
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                RemovedParts = DescribeRemovedParts(cab),
                IsDone = savedKeys?.Contains(MakeKey(cab.Id)) == true
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RemovePartsExceptionRow.IsDone))
                {
                    if (!_isRefreshing) UpdateDoneKey(row);
                    UpdateTabHeaderBrush();
                }
            };

            Exceptions.Add(row);
            TotalCabsNeedingChange += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
        _isRefreshing = false;
    }

    [RelayCommand]
    private void RefreshList() => Refresh();

    private static string MakeKey(Guid cabinetId) => cabinetId.ToString("N");

    private void SnapshotDoneKeys()
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        foreach (var row in Exceptions)
        {
            var key = MakeKey(row.CabinetId);
            if (row.IsDone) set.Add(key); else set.Remove(key);
        }
    }

    private void UpdateDoneKey(RemovePartsExceptionRow row)
    {
        if (_cabinetService == null) return;

        if (!_cabinetService.ExceptionDoneKeys.TryGetValue(TabId, out var set))
        {
            set = new HashSet<string>();
            _cabinetService.ExceptionDoneKeys[TabId] = set;
        }

        var key = MakeKey(row.CabinetId);
        if (row.IsDone) set.Add(key); else set.Remove(key);

        _cabinetService.RaiseExceptionDoneStateChanged();
    }

    private void UpdateTabHeaderBrush()
    {
        if (Exceptions.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = Exceptions.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_allDoneBlue : s_warnRed;
    }

    public sealed partial class RemovePartsExceptionRow : ObservableObject
    {
        public Guid CabinetId { get; set; }

        [ObservableProperty] public partial bool IsDone { get; set; }
        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string RemovedParts { get; set; } = "";
    }
}