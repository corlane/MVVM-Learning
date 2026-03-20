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
        DataContextChanged += (_, _) => HookTabIndexChanged();
        Loaded += (_, _) => HookTabIndexChanged();
    }

    private void HookTabIndexChanged()
    {
        if (DataContext is not CorlaneCabinetOrderFormV3.ViewModels.MainWindowViewModel vm)
            return;

        vm.PropertyChanged -= Vm_PropertyChanged;
        vm.PropertyChanged += Vm_PropertyChanged;

        // apply immediately (handles startup / restored state)
        ApplySplitterPreset(vm.SelectedTabIndex);
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CorlaneCabinetOrderFormV3.ViewModels.MainWindowViewModel.SelectedTabIndex))
            return;

        if (sender is CorlaneCabinetOrderFormV3.ViewModels.MainWindowViewModel vm)
        {
            ApplySplitterPreset(vm.SelectedTabIndex);
        }
    }

    private double? _savedViewportRow0Px;
    private double? _savedViewportRow2Px;

    private void ApplySplitterPreset(int selectedTabIndex)
    {
        // Hide splitter on: 6 Material Prices, 7 Process Order
        RightPreviewSplitter.Visibility = selectedTabIndex is 6 or 7
            ? Visibility.Collapsed
            : Visibility.Visible;

        bool shouldCollapseViewport = selectedTabIndex is 4 or 5;

        var row0 = RightPreviewGrid.RowDefinitions[0];
        var row2 = RightPreviewGrid.RowDefinitions[2];

        if (shouldCollapseViewport)
        {
            // Save current rendered heights once (what the user dragged to)
            if (_savedViewportRow0Px is null && row0.ActualHeight > 0)
                _savedViewportRow0Px = row0.ActualHeight;

            if (_savedViewportRow2Px is null && row2.ActualHeight > 0)
                _savedViewportRow2Px = row2.ActualHeight;

            row0.Height = new GridLength(0, GridUnitType.Pixel);

            // Let the bottom take the rest while collapsed
            row2.Height = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            if (_savedViewportRow0Px is double topPx && topPx > 0)
                row0.Height = new GridLength(topPx, GridUnitType.Pixel);

            if (_savedViewportRow2Px is double bottomPx && bottomPx > 0)
                row2.Height = new GridLength(bottomPx, GridUnitType.Pixel);
        }
    }

    private void RightPreviewSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        // Only record sizes when we're in the "normal" mode (i.e., not collapsed for Place Order / Defaults)
        if (DataContext is not MainWindowViewModel vm) return;
        if (vm.SelectedTabIndex is 4 or 5) return;

        var row0 = RightPreviewGrid.RowDefinitions[0];
        var row2 = RightPreviewGrid.RowDefinitions[2];

        if (row0.ActualHeight > 0) _savedViewportRow0Px = row0.ActualHeight;
        if (row2.ActualHeight > 0) _savedViewportRow2Px = row2.ActualHeight;
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