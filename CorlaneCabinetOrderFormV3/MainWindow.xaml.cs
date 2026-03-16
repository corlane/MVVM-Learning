using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CorlaneCabinetOrderFormV3;

public partial class MainWindow : Window
{
    private bool _allowClose;

    public MainWindow()
    {
        InitializeComponent();
        this.Closed += MainWindow_Closed;
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.ContextMenu is null) return;

        btn.ContextMenu.PlacementTarget = btn;
        btn.ContextMenu.IsOpen = true;
    }

    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (!vm.IsModified)
        {
            return;
        }

        // Pause closing so we can ask the question.
        e.Cancel = true;

        var result = MessageBox.Show(
            "You have unsaved changes. Save before closing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel)
        {
            return;
        }

        if (result == MessageBoxResult.No)
        {
            _allowClose = true;
            e.Cancel = false; // allow THIS close to proceed (do not call Close())
            return;
        }

        // Yes => attempt save
        try
        {
            await vm.SaveJobCommand.ExecuteAsync(null);

            // If the user completed a save (or you set IsModified=false on save), then allow close.
            if (!vm.IsModified)
            {
                _allowClose = true;
                e.Cancel = false; // allow THIS close to proceed (do not call Close())
            }
        }
        catch
        {
            // Save failed: keep window open.
        }
    }

private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        try
        {
            var defaults = App.ServiceProvider.GetRequiredService<CorlaneCabinetOrderFormV3.Services.DefaultSettingsService>();

            // Prefer RestoreBounds so a maximized window stores the restored (normal) bounds
            var bounds = this.RestoreBounds;

            double left = bounds.Left;
            double top = bounds.Top;
            double width = bounds.Width;
            double height = bounds.Height;

            // Fallbacks if RestoreBounds yielded invalid values
            if (double.IsNaN(left) || double.IsInfinity(left)) left = this.Left;
            if (double.IsNaN(top) || double.IsInfinity(top)) top = this.Top;
            if (double.IsNaN(width) || width <= 0) width = this.Width;
            if (double.IsNaN(height) || height <= 0) height = this.Height;

            // Sanity clamp so JSON doesn't get absurd values
            width = Math.Max(300, width);
            height = Math.Max(200, height);

            defaults.WindowLeft = left;
            defaults.WindowTop = top;
            defaults.WindowWidth = width;
            defaults.WindowHeight = height;
            defaults.WindowState = this.WindowState.ToString();

            await defaults.SaveAsync().ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            // Log the exception so we can see why save might have failed
            System.Diagnostics.Debug.WriteLine("Failed saving window bounds: " + ex);
        }
    }
}