using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    private void PrintCabinetList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        printer.PrintCabinetList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            dimensionFormat: defaults.DefaultDimensionFormat ?? "Fraction",
            cabinets: _cabinet_service.Cabinets.ToList());
    }

    [RelayCommand]
    private void PrintDoorList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        var doorVm = App.ServiceProvider.GetRequiredService<DoorSizesListViewModel>();
        doorVm.Rebuild();

        printer.PrintDoorList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            doors: doorVm.DoorSizes.ToList());
    }

    [RelayCommand]
    private void PrintDrawerBoxList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        var drawerVm = App.ServiceProvider.GetRequiredService<DrawerBoxSizesListViewModel>();
        drawerVm.Rebuild();

        printer.PrintDrawerBoxList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            drawerBoxes: drawerVm.DrawerBoxSizes.ToList());
    }

    // --- CSV export commands added below ---

    [RelayCommand]
    private void ExportCabinetList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"{CurrentJobName} - Cabinet List.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            string dimensionFormat = defaults.DefaultDimensionFormat ?? "Fraction";

            static string FormatDimensionString(string? raw, string dimensionFormat)
            {
                if (string.IsNullOrWhiteSpace(raw)) return "";
                double parsed = ConvertDimension.FractionToDouble(raw);
                return string.Equals(dimensionFormat, "Decimal", System.StringComparison.OrdinalIgnoreCase)
                    ? parsed.ToString("0.####")
                    : ConvertDimension.DoubleToFraction(parsed);
            }

            static string EscapeCsv(string s)
            {
                if (s is null) return "";
                if (s.Contains('"'))
                    s = s.Replace("\"", "\"\"");
                if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                    return $"\"{s}\"";
                return s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Qty,Type,Style,Name,Width,Height,Depth");

            foreach (var cab in _cabinet_service.Cabinets)
            {
                var w = FormatDimensionString(cab.Width, dimensionFormat);
                var h = FormatDimensionString(cab.Height, dimensionFormat);
                var d = FormatDimensionString(cab.Depth, dimensionFormat);

                sb.AppendLine(string.Join(",",
                    EscapeCsv(cab.Qty.ToString()),
                    EscapeCsv(cab.CabinetType ?? ""),
                    EscapeCsv(cab.Style ?? ""),
                    EscapeCsv(cab.Name ?? ""),
                    EscapeCsv(w),
                    EscapeCsv(h),
                    EscapeCsv(d)));
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Cabinet list exported.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error exporting cabinet list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportDoorList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var doorVm = App.ServiceProvider.GetRequiredService<DoorSizesListViewModel>();
        doorVm.Rebuild();

        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"{CurrentJobName} - Door List.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            static string EscapeCsv(string s)
            {
                if (s is null) return "";
                if (s.Contains('"'))
                    s = s.Replace("\"", "\"\"");
                if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                    return $"\"{s}\"";
                return s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Cab #,Cabinet Name,Type,Height,Width,Species,Grain");

            foreach (var d in doorVm.DoorSizes)
            {
                var height = d.DisplayHeight ?? d.Height.ToString("0.####");
                var width = d.DisplayWidth ?? d.Width.ToString("0.####");

                sb.AppendLine(string.Join(",",
                    EscapeCsv(d.CabinetNumber.ToString()),
                    EscapeCsv(d.CabinetName ?? ""),
                    EscapeCsv(d.Type ?? ""),
                    EscapeCsv(height),
                    EscapeCsv(width),
                    EscapeCsv(d.Species ?? ""),
                    EscapeCsv(d.GrainDirection ?? "")
                ));
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Door list exported.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error exporting door list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportDrawerBoxList()
    {
        var drawerVm = App.ServiceProvider.GetRequiredService<DrawerBoxSizesListViewModel>();
        drawerVm.Rebuild();

        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"{CurrentJobName} - Drawer Box List.csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            static string EscapeCsv(string s)
            {
                if (s is null) return "";
                if (s.Contains('"'))
                    s = s.Replace("\"", "\"\"");
                if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                    return $"\"{s}\"";
                return s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Cab #,Cabinet Name,Type,Height,Width,Length");

            foreach (var r in drawerVm.DrawerBoxSizes)
            {
                var height = r.DisplayHeight ?? r.Height.ToString("0.####");
                var width = r.DisplayWidth ?? r.Width.ToString("0.####");
                var length = r.DisplayLength ?? r.Length.ToString("0.####");

                sb.AppendLine(string.Join(",",
                    EscapeCsv(r.CabinetNumber.ToString()),
                    EscapeCsv(r.CabinetName ?? ""),
                    EscapeCsv(r.Type ?? ""),
                    EscapeCsv(height),
                    EscapeCsv(width),
                    EscapeCsv(length)
                ));
            }

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Drawer box list exported.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error exporting drawer box list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}