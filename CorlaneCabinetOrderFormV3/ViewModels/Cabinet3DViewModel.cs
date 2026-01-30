using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class Cabinet3DViewModel : ObservableObject
{
    public Cabinet3DViewModel()
    {
        // Blank constructor for design
    }

    private readonly IPreviewService? _previewSvc;

    // Debounce state (coalesce multiple rebuild requests)
    private DispatcherOperation? _pendingRebuild;

    private void RequestRebuildPreview()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        if (!dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RequestRebuildPreview));
            return;
        }

        // If a rebuild is already queued, don't queue another.
        if (_pendingRebuild is { Status: DispatcherOperationStatus.Pending })
            return;

        _pendingRebuild = dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RebuildPreview));
    }

    public Cabinet3DViewModel(IPreviewService previewSvc)
    {
        _previewSvc = previewSvc;

        _previewSvc.PreviewChanged += PreviewSvc_PreviewChanged;

        if (_previewSvc.CurrentPreviewCabinet != null)
        {
            RequestRebuildPreview();
        }
    }

    // Visibility Properties
    [ObservableProperty] public partial bool LeftEndHidden { get; set; } = false; partial void OnLeftEndHiddenChanged(bool value)
    {
        HideLeftEndText = value ? "Show Left End" : "Hide Left End";
        RequestRebuildPreview();
    }

    [ObservableProperty] public partial bool RightEndHidden { get; set; } = false; partial void OnRightEndHiddenChanged(bool value)
    {
        HideRightEndText = value ? "Show Right End" : "Hide Right End";
        RequestRebuildPreview();
    }

    [ObservableProperty] public partial bool DeckHidden { get; set; } = false; partial void OnDeckHiddenChanged(bool value)
    {
        HideDeckText = value ? "Show Deck" : "Hide Deck";
        RequestRebuildPreview();
    }

    [ObservableProperty] public partial bool TopHidden { get; set; } = false; partial void OnTopHiddenChanged(bool value)
    {
        HideTopText = value ? "Show Top" : "Hide Top";
        RequestRebuildPreview();
    }

    [ObservableProperty] public partial string HideTopText { get; set; } = "Hide Top";
    [ObservableProperty] public partial string HideDeckText { get; set; } = "Hide Deck";
    [ObservableProperty] public partial string HideLeftEndText { get; set; } = "Hide Left End";
    [ObservableProperty] public partial string HideRightEndText { get; set; } = "Hide Right End";

    private void PreviewSvc_PreviewChanged(object? sender, EventArgs e)
    {
        RequestRebuildPreview();
    }

    [ObservableProperty]
    public partial Model3DGroup? PreviewModel { get; set; }

    partial void OnPreviewModelChanged(Model3DGroup? value)
    {
        PreviewBounds = value?.Bounds ?? Rect3D.Empty;
    }

    [ObservableProperty]
    public partial Rect3D PreviewBounds { get; set; }

    [RelayCommand]
    private void LoadInitialModel()
    {
        RequestRebuildPreview();
    }

    public void RebuildPreview()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RebuildPreview));
            return;
        }

        PreviewModel = CabinetPreviewBuilder.BuildPreviewModel(
            _previewSvc!.CurrentPreviewCabinet,
            LeftEndHidden,
            RightEndHidden,
            DeckHidden,
            TopHidden);
    }

    public void AccumulateMaterialAndEdgeTotals(CabinetModel? cab)
    {
        if (cab == null) return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        dispatcher.Invoke(() =>
        {
            try
            {
                cab.ResetAllMaterialAndEdgeTotals();
                _ = CabinetPreviewBuilder.BuildCabinetForTotals(cab);
            }
            catch
            {
                // best-effort
            }
        }, DispatcherPriority.Background);
    }
}
