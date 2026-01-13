using CorlaneCabinetOrderFormV3.ViewModels;
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
}