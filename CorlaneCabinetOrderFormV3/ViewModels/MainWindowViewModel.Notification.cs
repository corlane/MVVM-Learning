using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    private CancellationTokenSource? _notificationCts;

    [ObservableProperty]
    private string notificationMessage = "";

    [ObservableProperty]
    private bool isNotificationVisible;

    // Fire-and-forget helper
    public void Notify(string message, int durationMs = 2000) => _ = ShowNotificationAsync(message, durationMs);

    // Async notification that cancels previous one and hides after duration
    public async Task ShowNotificationAsync(string message, int durationMs = 2000)
    {
        _notificationCts?.Cancel();
        _notificationCts = new CancellationTokenSource();
        var ct = _notificationCts.Token;

        // Ensure initial set runs on UI thread
        if (Application.Current?.Dispatcher == null)
        {
            NotificationMessage = message;
            IsNotificationVisible = true;
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotificationMessage = message;
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
            // Marshal final hide to UI thread
            if (Application.Current?.Dispatcher == null)
            {
                IsNotificationVisible = false;
                NotificationMessage = "";
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsNotificationVisible = false;
                    NotificationMessage = "";
                });
            }
        }
    }
}