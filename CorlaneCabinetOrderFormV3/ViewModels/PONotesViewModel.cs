using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class PONotesViewModel : ObservableObject
{
    private static readonly SolidColorBrush s_okGreen = Brushes.ForestGreen;
    private static readonly SolidColorBrush s_warnRed = new(Color.FromRgb(255, 88, 113));

    private readonly ICabinetService? _cabinetService;

    public PONotesViewModel()
    {
        // design-time support
        UpdateTabHeaderBrush();
    }

    public PONotesViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    // Default is intentionally blank and not user-editable for this tab.
    public string DefaultNotes => "";

    public ObservableCollection<NotesExceptionRow> NotesToReview { get; } = new();

    [ObservableProperty]
    public partial int TotalCabsNeedingReview { get; set; }

    [ObservableProperty]
    public partial Brush TabHeaderBrush { get; set; } = s_okGreen;

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        NotesToReview.Clear();
        TotalCabsNeedingReview = 0;

        if (_cabinetService == null)
        {
            UpdateTabHeaderBrush();
            return;
        }

        int cabNumber = 0;

        foreach (var cab in _cabinetService.Cabinets)
        {
            cabNumber++;

            var notes = cab.Notes ?? "";
            if (string.IsNullOrWhiteSpace(notes))
            {
                continue;
            }

            var row = new NotesExceptionRow
            {
                CabinetNumber = cabNumber,
                CabinetName = cab.Name ?? "",
                CabinetType = cab.CabinetType,
                Notes = notes.Trim(),
                IsDone = false
            };

            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(NotesExceptionRow.IsDone))
                {
                    UpdateTabHeaderBrush();
                }
            };

            NotesToReview.Add(row);
            TotalCabsNeedingReview += Math.Max(1, cab.Qty);
        }

        UpdateTabHeaderBrush();
    }

    private void UpdateTabHeaderBrush()
    {
        if (NotesToReview.Count == 0)
        {
            TabHeaderBrush = s_okGreen;
            return;
        }

        bool allDone = NotesToReview.All(r => r.IsDone);
        TabHeaderBrush = allDone ? s_okGreen : s_warnRed;
    }

    public sealed partial class NotesExceptionRow : ObservableObject
    {
        [ObservableProperty] public partial bool IsDone { get; set; }

        [ObservableProperty] public partial int CabinetNumber { get; set; }
        [ObservableProperty] public partial string CabinetName { get; set; } = "";
        [ObservableProperty] public partial string CabinetType { get; set; } = "";
        [ObservableProperty] public partial string Notes { get; set; } = "";
    }
}