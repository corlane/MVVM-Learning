using CorlaneCabinetOrderFormV3.ViewModels;
using CorlaneCabinetOrderFormV3.Views;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3;

public partial class MainWindow : Window
{
    private bool _allowClose;
    private readonly Cabinet3DView _viewport;

    public MainWindow()
    {
        InitializeComponent();
        _viewport = App.ServiceProvider.GetRequiredService<Cabinet3DView>();
        this.Closed += MainWindow_Closed;
        DataContextChanged += (_, _) => HookTabIndexChanged();
        Loaded += (_, _) =>
        {
            HookTabIndexChanged();
            MoveViewportToActiveTab();
        };
    }

    private void HookTabIndexChanged()
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        vm.PropertyChanged -= Vm_PropertyChanged;
        vm.PropertyChanged += Vm_PropertyChanged;
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.SelectedTabIndex))
        {
            // Defer so the visual tree has updated after tab switch
            Dispatcher.InvokeAsync(MoveViewportToActiveTab,
                System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void MoveViewportToActiveTab()
    {
        // Detach from current parent
        if (_viewport.Parent is Border oldBorder)
            oldBorder.Child = null;
        else if (_viewport.Parent is ContentPresenter cp)
            cp.Content = null;

        if (DataContext is not MainWindowViewModel vm)
            return;

        // Only show viewport on cabinet tabs (0–3) with experimental view
        // Only show viewport on cabinet tabs (0–3)
        if (!vm.ViewportVisible)
            return;

        // Find the active tab's content
        var tabItem = MainTabControl.ItemContainerGenerator
            .ContainerFromIndex(vm.SelectedTabIndex) as TabItem;

        if (tabItem?.Content is not DependencyObject root)
            return;

        // Walk the visual tree to find a Border named "ViewportHost"
        var host = FindViewportHost(root);
        if (host != null)
            host.Child = _viewport;
    }

    private static Border? FindViewportHost(DependencyObject? root)
    {
        if (root is null) return null;

        // The tab content might not be in the visual tree yet, walk logical tree first
        if (root is Border b && b.Name == "ViewportHost")
            return b;

        // Check logical children (works for UserControls not yet in visual tree)
        foreach (var child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is not DependencyObject dChild) continue;
            var found = FindViewportHost(dChild);
            if (found != null) return found;
        }

        return null;
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.ContextMenu is null) return;
        btn.ContextMenu.PlacementTarget = btn;
        btn.ContextMenu.IsOpen = true;
    }

    private async void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowClose) return;
        if (DataContext is not MainWindowViewModel vm) return;
        if (!vm.IsModified) return;

        e.Cancel = true;

        var result = MessageBox.Show(
            "You have unsaved changes. Save before closing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel) return;

        if (result == MessageBoxResult.No)
        {
            _allowClose = true;
            _ = Dispatcher.BeginInvoke(Close);
            return;
        }

        try
        {
            await vm.SaveJobCommand.ExecuteAsync(null);
            if (!vm.IsModified)
            {
                _allowClose = true;
                _ = Dispatcher.BeginInvoke(Close);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Catch] Save on close failed: {ex.Message}");
        }
    }

    private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        try
        {
            var defaults = App.ServiceProvider.GetRequiredService<CorlaneCabinetOrderFormV3.Services.DefaultSettingsService>();
            var bounds = this.RestoreBounds;

            double left = bounds.Left;
            double top = bounds.Top;
            double width = bounds.Width;
            double height = bounds.Height;

            if (double.IsNaN(left) || double.IsInfinity(left)) left = this.Left;
            if (double.IsNaN(top) || double.IsInfinity(top)) top = this.Top;
            if (double.IsNaN(width) || width <= 0) width = this.Width;
            if (double.IsNaN(height) || height <= 0) height = this.Height;

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
            Debug.WriteLine($"[Catch] Save on close failed: {ex.Message}");
        }
    }
}