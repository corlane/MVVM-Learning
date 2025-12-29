//using CommunityToolkit.Mvvm.ComponentModel;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;

//namespace CorlaneCabinetOrderFormV3.ViewModels;

//public partial class MainWindowViewModel
//{
//    private CancellationTokenSource? _notificationCts;

//    [ObservableProperty]
//    public partial string NotificationMessage { get; set; } = "";

//    [ObservableProperty]
//    public partial bool IsNotificationVisible { get; set; }

//    // Fire-and-forget helper
//    public void Notify(string message, int durationMs = 2000) => _ = ShowNotificationAsync(message, durationMs);

//    // Async notification that cancels previous one and hides after duration
//    public async Task ShowNotificationAsync(string message, int durationMs = 2000)
//    {
//        _notificationCts?.Cancel();
//        _notificationCts = new CancellationTokenSource();
//        var ct = _notificationCts.Token;

//        // Ensure initial set runs on UI thread
//        if (Application.Current?.Dispatcher == null)
//        {
//            NotificationMessage = message;
//            IsNotificationVisible = true;
//        }
//        else
//        {
//            Application.Current.Dispatcher.Invoke(() =>
//            {
//                NotificationMessage = message;
//                IsNotificationVisible = true;
//            });
//        }

//        try
//        {
//            await Task.Delay(durationMs, ct).ConfigureAwait(false);
//        }
//        catch (TaskCanceledException)
//        {
//            // ignored - new notification replaced this one
//        }

//        if (!ct.IsCancellationRequested)
//        {
//            // Marshal final hide to UI thread
//            if (Application.Current?.Dispatcher == null)
//            {
//                IsNotificationVisible = false;
//                NotificationMessage = "";
//            }
//            else
//            {
//                Application.Current.Dispatcher.Invoke(() =>
//                {
//                    IsNotificationVisible = false;
//                    NotificationMessage = "";
//                });
//            }
//        }
//    }
//}




using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    private CancellationTokenSource? _notificationCts;

    [ObservableProperty]
    public partial string NotificationMessage { get; set; } = "";

    [ObservableProperty]
    public partial bool IsNotificationVisible { get; set; }

    // New: background brush for the notification. Null => XAML fallback resource used.
    [ObservableProperty]
    public partial Brush? NotificationBackground { get; set; }

    // Fire-and-forget helper (overload accepts optional background)
    public void Notify(string message, Brush? background = null, int durationMs = 2000) => _ = ShowNotificationAsync(message, background, durationMs);

    // Async notification that cancels previous one and hides after duration
    public async Task ShowNotificationAsync(string message, Brush? background = null, int durationMs = 2000)
    {
        _notificationCts?.Cancel();
        _notificationCts = new CancellationTokenSource();
        var ct = _notificationCts.Token;

        // Ensure initial set runs on UI thread and also set background
        if (Application.Current?.Dispatcher == null)
        {
            NotificationMessage = message;
            NotificationBackground = background;
            IsNotificationVisible = true;
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotificationMessage = message;
                NotificationBackground = background;
                IsNotificationVisible = true;
            });
        }

        try
        {
            await Task.Delay(durationMs, ct).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // ignored - new notification replaced this one
        }

        if (!ct.IsCancellationRequested)
        {
            // Marshal final hide to UI thread and clear background
            if (Application.Current?.Dispatcher == null)
            {
                IsNotificationVisible = false;
                NotificationMessage = "";
                NotificationBackground = null;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsNotificationVisible = false;
                    NotificationMessage = "";
                    NotificationBackground = null;
                });
            }
        }
    }
}