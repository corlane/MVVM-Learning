using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class POBatchListViewModel : ObservableObject
{
    private const string CsvDirectoryPlaceholder = "TODO";
    private const string CsvTypeConstant = "Cabinet";

    private readonly ICabinetService? _cabinetService;

    public POBatchListViewModel()
    {
        // design-time support
        Refresh();
    }

    public POBatchListViewModel(ICabinetService cabinetService)
    {
        _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

        if (_cabinetService.Cabinets is INotifyCollectionChanged cc)
        {
            cc.CollectionChanged += (_, __) => Refresh();
        }

        Refresh();
    }

    public ObservableCollection<BatchListRow> Rows { get; } = new();

    [ObservableProperty]
    public partial int TotalCabinetEntries { get; set; }

    public void Refresh()
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(Refresh);
            return;
        }

        Rows.Clear();
        TotalCabinetEntries = 0;

        if (_cabinetService is null)
        {
            return;
        }

        for (int i = 0; i < _cabinetService.Cabinets.Count; i++)
        {
            var cab = _cabinetService.Cabinets[i];

            var modelName = BuildModelName(cab);
            var qty = Math.Max(1, cab.Qty);

            var isCornerOrAngleFront =
                (cab.Style?.Contains("Corner", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (cab.Style?.Contains("Angle", StringComparison.OrdinalIgnoreCase) ?? false);

            var height = ToDouble(cab.Height);
            var width = isCornerOrAngleFront ? 0d : ToDouble(cab.Width);
            var depth = isCornerOrAngleFront ? 0d : ToDouble(cab.Depth);

            Rows.Add(new BatchListRow
            {
                Name = modelName,
                Directory = CsvDirectoryPlaceholder,
                Type = CsvTypeConstant,
                Quantity = qty,
                Height = height,
                Width = width,
                Depth = depth,
                NewName = cab.Name ?? ""
            });

            TotalCabinetEntries += qty;
        }
    }

    private static double ToDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0d;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0d;
    }

    [RelayCommand]
    private void Generate()
    {
        Refresh();
    }

    [RelayCommand]
    private void SaveCsv()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Save Batch List",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = "BatchList.csv"
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var csv = BuildCsv();
            File.WriteAllText(dlg.FileName, csv, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving CSV: {ex.Message}", "Error");
        }
    }

    private string BuildCsv()
    {
        const string Sep = ", ";

        static string Esc(string s)
        {
            // Minimal CSV escaping: quote if contains comma, quote, or newline
            if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }

            return s;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Name, Directory, Type, Quantity, Height, Width, Depth, New Name");

        foreach (var r in Rows)
        {
            sb.Append(Esc(r.Name ?? ""));
            sb.Append(Sep);
            sb.Append(Esc(r.Directory ?? ""));
            sb.Append(Sep);
            sb.Append(Esc(r.Type ?? ""));
            sb.Append(Sep);
            sb.Append(Esc(r.Quantity.ToString(CultureInfo.InvariantCulture)));
            sb.Append(Sep);
            sb.Append(Esc(r.Height.ToString(CultureInfo.InvariantCulture)));
            sb.Append(Sep);
            sb.Append(Esc(r.Width.ToString(CultureInfo.InvariantCulture)));
            sb.Append(Sep);
            sb.Append(Esc(r.Depth.ToString(CultureInfo.InvariantCulture)));
            sb.Append(Sep);
            sb.AppendLine(Esc(r.NewName ?? ""));
        }

        return sb.ToString();
    }

    private static string BuildModelName(CabinetModel cab)
    {
        // Format (13 columns, no delimiters):
        // Type + Style + Sink(Y/N) + Toekick(Y/N) + DoorCount + DrwCount + AdjShelfCount + RolloutCount + TopType(S/F) + BackThickness(1/3)
        // + ShelfDepth(H/F) + DrillShelfHoles(Y/N) + TrashDrawer(Y/N)

        static string BoolYN(bool value) => value ? "Y" : "N";

        static int ClampNonNegative(int value) => Math.Max(0, value);

        static string TypeCode(CabinetModel c) =>
            c switch
            {
                BaseCabinetModel => "B",
                UpperCabinetModel => "U",
                FillerModel => "F",
                PanelModel => "P",
                _ => "X"
            };

        static string StyleCode(CabinetModel c)
        {
            // Style only applies to Base/Upper; fillers/panels use X.
            if (c is not BaseCabinetModel && c is not UpperCabinetModel)
            {
                return "X";
            }

            var style = c.Style ?? "";

            // Explicit per your rule:
            // Upper: S=Standard, C=90° Corner, A=Angle Front
            if (c is UpperCabinetModel)
            {
                if (style.Contains("Standard", StringComparison.OrdinalIgnoreCase))
                {
                    return "S";
                }

                if (style.Contains("90", StringComparison.OrdinalIgnoreCase) ||
                    style.Contains("Corner", StringComparison.OrdinalIgnoreCase))
                {
                    return "C";
                }

                if (style.Contains("Angle", StringComparison.OrdinalIgnoreCase))
                {
                    return "A";
                }

                return "X";
            }

            // Base: S=Standard, D=Drawer, C=90° Corner, A=Angle Front
            if (style.Contains("Standard", StringComparison.OrdinalIgnoreCase))
            {
                return "S";
            }

            if (style.Contains("Drawer", StringComparison.OrdinalIgnoreCase))
            {
                return "D";
            }

            if (style.Contains("90", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("Corner", StringComparison.OrdinalIgnoreCase))
            {
                return "C";
            }

            if (style.Contains("Angle", StringComparison.OrdinalIgnoreCase))
            {
                return "A";
            }

            return "X";
        }

        static string TopTypeCode(CabinetModel c)
        {
            // Rule: any cabinet OTHER than Base => TopType always F
            if (c is not BaseCabinetModel)
            {
                return "F";
            }

            if (c is BaseCabinetModel b)
            {
                var topType = b.TopType ?? "";

                if (topType.Contains("Stretcher", StringComparison.OrdinalIgnoreCase))
                {
                    return "S";
                }

                if (topType.Contains("Full", StringComparison.OrdinalIgnoreCase))
                {
                    return "F";
                }

                if (string.Equals(topType.Trim(), "S", StringComparison.OrdinalIgnoreCase))
                {
                    return "S";
                }

                if (string.Equals(topType.Trim(), "F", StringComparison.OrdinalIgnoreCase))
                {
                    return "F";
                }
            }

            return "X";
        }

        static string BackThicknessCode(string? backThickness)
        {
            if (string.IsNullOrWhiteSpace(backThickness))
            {
                return "X";
            }

            var t = backThickness.Trim();

            if (string.Equals(t, "1", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("1/4", StringComparison.OrdinalIgnoreCase) ||
                t.Contains(".25", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("0.25", StringComparison.OrdinalIgnoreCase))
            {
                return "1";
            }

            if (string.Equals(t, "3", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("3/4", StringComparison.OrdinalIgnoreCase) ||
                t.Contains(".75", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("0.75", StringComparison.OrdinalIgnoreCase))
            {
                return "3";
            }

            return "X";
        }

        static string ShelfDepthCode(string? style, string? shelfDepth)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return "F";
            }

            bool supportsHalfDepth =
                style.Contains("Standard", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("90", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("Corner", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("Angle", StringComparison.OrdinalIgnoreCase);

            if (!supportsHalfDepth)
            {
                return "F";
            }

            return string.Equals(shelfDepth?.Trim(), "Half Depth", StringComparison.OrdinalIgnoreCase)
                ? "H"
                : "F";
        }

        var type = TypeCode(cab);
        var styleCode = StyleCode(cab);
        var topType = TopTypeCode(cab);

        // Defaults for cabinet types/properties that don't apply
        var sink = "N";
        var toekick = "N";
        var doorCount = 0;
        var drwCount = 0;
        var shelfCount = 0;
        var rolloutCount = 0;
        var backThickness = "X";
        var shelfDepthCode = type is "B" or "U" ? "F" : "X";
        var drillShelfHolesCode = "N";
        var trashDrawerCode = "N";

        switch (cab)
        {
            case BaseCabinetModel b:
                sink = BoolYN(b.SinkCabinet);
                toekick = BoolYN(b.HasTK);
                doorCount = ClampNonNegative(b.DoorCount);
                drwCount = ClampNonNegative(b.DrwCount);
                shelfCount = ClampNonNegative(b.ShelfCount);
                rolloutCount = ClampNonNegative(b.RolloutCount);
                backThickness = BackThicknessCode(b.BackThickness);

                shelfDepthCode = ShelfDepthCode(b.Style, b.ShelfDepth);
                drillShelfHolesCode = BoolYN(b.DrillShelfHoles);
                trashDrawerCode = BoolYN(b.TrashDrawer);
                break;

            case UpperCabinetModel u:
                doorCount = ClampNonNegative(u.DoorCount);
                shelfCount = ClampNonNegative(u.ShelfCount);
                backThickness = BackThicknessCode(u.BackThickness);

                shelfDepthCode = "F";
                drillShelfHolesCode = BoolYN(u.DrillShelfHoles);
                break;

            case FillerModel:
                break;

            case PanelModel p:
                if (p.PanelEBTop) doorCount = 1;
                if (p.PanelEBBottom) drwCount = 1;
                if (p.PanelEBLeft) shelfCount = 1;
                if (p.PanelEBRight) rolloutCount = 1;
                break;
        }

        return string.Concat(
            type,
            styleCode,
            sink,
            toekick,
            doorCount.ToString(CultureInfo.InvariantCulture),
            drwCount.ToString(CultureInfo.InvariantCulture),
            shelfCount.ToString(CultureInfo.InvariantCulture),
            rolloutCount.ToString(CultureInfo.InvariantCulture),
            topType,
            backThickness,
            shelfDepthCode,
            drillShelfHolesCode,
            trashDrawerCode
        );
    }

    public sealed partial class BatchListRow : ObservableObject
    {
        [ObservableProperty] public partial string Name { get; set; } = "";
        [ObservableProperty] public partial string Directory { get; set; } = "";
        [ObservableProperty] public partial string Type { get; set; } = "";
        [ObservableProperty] public partial int Quantity { get; set; }
        [ObservableProperty] public partial double Height { get; set; }
        [ObservableProperty] public partial double Width { get; set; }
        [ObservableProperty] public partial double Depth { get; set; }
        [ObservableProperty] public partial string NewName { get; set; } = "";
    }
}
