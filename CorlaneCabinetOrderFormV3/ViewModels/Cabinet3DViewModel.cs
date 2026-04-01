//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using CorlaneCabinetOrderFormV3.Models;
//using CorlaneCabinetOrderFormV3.Rendering;
//using CorlaneCabinetOrderFormV3.Services;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Windows;
//using System.Windows.Media.Media3D;
//using System.Windows.Threading;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public partial class Cabinet3DViewModel : ObservableObject
//{
//    public Cabinet3DViewModel()
//    {
//        // Blank constructor for design
//    }

//    private readonly IPreviewService? _previewSvc;
//    private readonly ICabinetService? _cabinetSvc;
//    private readonly MainWindowViewModel? _mainVm;

//    // Debounce state (coalesce multiple rebuild requests)
//    private bool _rebuildQueued;

//    private void RequestRebuildPreview()
//    {
//        var dispatcher = Application.Current?.Dispatcher;
//        if (dispatcher == null)
//            return;

//        if (!dispatcher.CheckAccess())
//        {
//            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RequestRebuildPreview));
//            return;
//        }

//        if (_rebuildQueued)
//            return;

//        _rebuildQueued = true;

//        dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
//        {
//            try
//            {
//                RebuildPreview();
//            }
//            finally
//            {
//                _rebuildQueued = false;
//            }
//        }));
//    }




//    public Cabinet3DViewModel(IPreviewService previewSvc, ICabinetService cabinetSvc, MainWindowViewModel mainVm)
//    {
//        _previewSvc = previewSvc;
//        _cabinetSvc = cabinetSvc;
//        _mainVm = mainVm;

//        _previewSvc.PreviewChanged += PreviewSvc_PreviewChanged;

//        // Track actual list selection for the info text
//        PropertyChangedEventManager.AddHandler(
//            _mainVm,
//            MainVm_PropertyChanged,
//            nameof(MainWindowViewModel.SelectedCabinet));

//        if (_previewSvc.CurrentPreviewCabinet != null)
//        {
//            RequestRebuildPreview();
//        }

//        UpdateSelectedCabinetInfoText(_mainVm.SelectedCabinet);
//    }

//    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
//    {
//        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
//        {
//            UpdateSelectedCabinetInfoText(_mainVm?.SelectedCabinet);
//        }
//    }

//    // Visibility Properties
//    [ObservableProperty] public partial bool LeftEndHidden { get; set; } = false; partial void OnLeftEndHiddenChanged(bool value)
//    {
//        HideLeftEndText = value ? "Show Left End" : "Hide Left End";
//        RequestRebuildPreview();
//    }

//    [ObservableProperty] public partial bool RightEndHidden { get; set; } = false; partial void OnRightEndHiddenChanged(bool value)
//    {
//        HideRightEndText = value ? "Show Right End" : "Hide Right End";
//        RequestRebuildPreview();
//    }

//    [ObservableProperty] public partial bool DeckHidden { get; set; } = false; partial void OnDeckHiddenChanged(bool value)
//    {
//        HideDeckText = value ? "Show Deck" : "Hide Deck";
//        RequestRebuildPreview();
//    }

//    [ObservableProperty] public partial bool TopHidden { get; set; } = false; partial void OnTopHiddenChanged(bool value)
//    {
//        HideTopText = value ? "Show Top" : "Hide Top";
//        RequestRebuildPreview();
//    }

//    [ObservableProperty] public partial string HideTopText { get; set; } = "Hide Top";
//    [ObservableProperty] public partial string HideDeckText { get; set; } = "Hide Deck";
//    [ObservableProperty] public partial string HideLeftEndText { get; set; } = "Hide Left End";
//    [ObservableProperty] public partial string HideRightEndText { get; set; } = "Hide Right End";

//    [ObservableProperty]
//    public partial string SelectedCabinetInfoText { get; set; } = "No cabinet selected";

//    private void PreviewSvc_PreviewChanged(object? sender, EventArgs e)
//    {
//        RequestRebuildPreview();
//    }

//    [ObservableProperty]
//    public partial Model3DGroup? PreviewModel { get; set; }

//    partial void OnPreviewModelChanged(Model3DGroup? value)
//    {
//        PreviewBounds = value?.Bounds ?? Rect3D.Empty;
//    }

//    [ObservableProperty]
//    public partial Rect3D PreviewBounds { get; set; }

//    [RelayCommand]
//    private void LoadInitialModel()
//    {
//        RequestRebuildPreview();
//    }

//    public void RebuildPreview()
//    {
//        var dispatcher = Application.Current?.Dispatcher;
//        if (dispatcher != null && !dispatcher.CheckAccess())
//        {
//            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(RebuildPreview));
//            return;
//        }

//        //Debug.WriteLine($"=== RebuildPreview #{DateTime.Now.Ticks} ===");
//        //Debug.WriteLine(Environment.StackTrace);

//        PreviewModel = CabinetPreviewBuilder.BuildPreviewModel(
//            _previewSvc!.CurrentPreviewCabinet,
//            LeftEndHidden,
//            RightEndHidden,
//            DeckHidden,
//            TopHidden);
//    }

//    private void UpdateSelectedCabinetInfoText(CabinetModel? cab)
//    {
//        if (cab == null)
//        {
//            SelectedCabinetInfoText = "No cabinet selected";
//            return;
//        }

//        int cabNumber = 0;
//        if (_cabinetSvc != null)
//        {
//            var index = _cabinetSvc.Cabinets.IndexOf(cab);
//            if (index >= 0)
//                cabNumber = index + 1;
//        }

//        SelectedCabinetInfoText = cabNumber > 0
//            ? $"Selected Item: #{cabNumber}\n{cab.Style} {cab.CabinetType}\n{cab.Name}"
//            : $"Selected Item: {cab.Style} {cab.CabinetType}\n{cab.Name}";
//    }
//}






using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Rendering;
using CorlaneCabinetOrderFormV3.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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
    private readonly ICabinetService? _cabinetSvc;
    private readonly MainWindowViewModel? _mainVm;

    // Debounce state (coalesce multiple rebuild requests)
    private bool _rebuildQueued;

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

        if (_rebuildQueued)
            return;

        _rebuildQueued = true;

        dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            try
            {
                RebuildPreview();
            }
            finally
            {
                _rebuildQueued = false;
            }
        }));
    }




    public Cabinet3DViewModel(IPreviewService previewSvc, ICabinetService cabinetSvc, MainWindowViewModel mainVm)
    {
        _previewSvc = previewSvc;
        _cabinetSvc = cabinetSvc;
        _mainVm = mainVm;

        _previewSvc.PreviewChanged += PreviewSvc_PreviewChanged;

        // Track actual list selection for the info text
        PropertyChangedEventManager.AddHandler(
            _mainVm,
            MainVm_PropertyChanged,
            nameof(MainWindowViewModel.SelectedCabinet));

        if (_previewSvc.CurrentPreviewCabinet != null)
        {
            RequestRebuildPreview();
        }

        UpdateSelectedCabinetInfoText(_mainVm.SelectedCabinet);
    }

    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedCabinet))
        {
            UpdateSelectedCabinetInfoText(_mainVm?.SelectedCabinet);
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

    [ObservableProperty]
    public partial string SelectedCabinetInfoText { get; set; } = "No cabinet selected";

    [ObservableProperty]
    public partial double PreviewZoomMultiplier { get; set; } = 1.0;

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

        var cab = _previewSvc!.CurrentPreviewCabinet;

        PreviewZoomMultiplier = GetStyleZoomMultiplier(cab);

        PreviewModel = CabinetPreviewBuilder.BuildPreviewModel(
            cab,
            LeftEndHidden,
            RightEndHidden,
            DeckHidden,
            TopHidden);
    }

    private static double GetStyleZoomMultiplier(CabinetModel? cab)
    {
        if (cab is null) return 1.0;

        if (string.Equals(cab.Style, CabinetStyles.Base.AngleFront, StringComparison.Ordinal)
            || string.Equals(cab.Style, CabinetStyles.Upper.AngleFront, StringComparison.Ordinal))
        {
            return 2.2;
        }

        if (string.Equals(cab.Style, CabinetStyles.Base.Corner90, StringComparison.Ordinal)
            || string.Equals(cab.Style, CabinetStyles.Upper.Corner90, StringComparison.Ordinal))
        {
            return 1.5; // adjust to taste
        }

        return 1.0;
    }

    private void UpdateSelectedCabinetInfoText(CabinetModel? cab)
    {
        if (cab == null)
        {
            SelectedCabinetInfoText = "No cabinet selected";
            return;
        }

        int cabNumber = 0;
        if (_cabinetSvc != null)
        {
            var index = _cabinetSvc.Cabinets.IndexOf(cab);
            if (index >= 0)
                cabNumber = index + 1;
        }

        SelectedCabinetInfoText = cabNumber > 0
            ? $"Selected Item: #{cabNumber}\n{cab.Style} {cab.CabinetType}\n{cab.Name}"
            : $"Selected Item: {cab.Style} {cab.CabinetType}\n{cab.Name}";
    }
}