using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.Services;

public sealed class PrintService : IPrintService
{
    private const double MarginInch = 0.75;
    private const double Dpi = 96.0;

    public void PrintCabinetList(string companyName, string jobName, string dimensionFormat, IReadOnlyList<CabinetModel> cabinets)
    {
        var doc = CreateDocument(companyName, jobName, "Cabinet List");
        AddCabinetTable(doc, cabinets, dimensionFormat);
        Print(doc, "Cabinet List");
    }

    public void PrintDoorList(string companyName, string jobName, IReadOnlyList<FrontPartRow> doors)
    {
        var doc = CreateDocument(companyName, jobName, "Door List");
        AddDoorTable(doc, doors);
        Print(doc, "Door List");
    }

    public void PrintDrawerBoxList(string companyName, string jobName, IReadOnlyList<DrawerBoxRow> drawerBoxes)
    {
        var doc = CreateDocument(companyName, jobName, "Drawer Box List");
        AddDrawerBoxTable(doc, drawerBoxes);
        Print(doc, "Drawer Box List");
    }

    private static void Print(FlowDocument doc, string description)
    {
        var dlg = new System.Windows.Controls.PrintDialog();
        if (dlg.ShowDialog() != true) return;

        doc.ColumnWidth = double.PositiveInfinity;
        doc.PageHeight = dlg.PrintableAreaHeight;
        doc.PageWidth = dlg.PrintableAreaWidth;

        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
        dlg.PrintDocument(paginator, description);
    }

    private static FlowDocument CreateDocument(string companyName, string jobName, string sectionTitle)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 10.5,
            PagePadding = new Thickness(MarginInch * Dpi),
            TextAlignment = TextAlignment.Left
        };

        companyName = string.IsNullOrWhiteSpace(companyName) ? "(Company)" : companyName.Trim();
        jobName = string.IsNullOrWhiteSpace(jobName) ? "(Job)" : jobName.Trim();

        doc.Blocks.Add(new Paragraph(new Run(companyName))
        {
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 6)
        });

        doc.Blocks.Add(new Paragraph(new Run($"Job: {jobName}"))
        {
            FontSize = 12.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        });

        doc.Blocks.Add(new Paragraph(new Run(sectionTitle))
        {
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        return doc;
    }

    private static string FormatDimensionString(string? raw, string dimensionFormat)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        double parsed = ConvertDimension.FractionToDouble(raw);

        if (string.Equals(dimensionFormat, "Decimal", StringComparison.OrdinalIgnoreCase))
        {
            return parsed.ToString("0.####");
        }

        return ConvertDimension.DoubleToFraction(parsed);
    }

    private static void AddCabinetTable(FlowDocument doc, IReadOnlyList<CabinetModel> cabinets, string dimensionFormat)
    {
        int qtyTotal = cabinets.Sum(c => c.Qty);
        var table = CreateTable(
            headers:
            [
                "Qty",
                "Type",
                "Style",
                "Name",
                "Width",
                "Height",
                "Depth",
            ],
            columnWidths:
            [
                45,
                95,
                95,
                170,
                80,
                80,
                80,
            ]);

        foreach (var cab in cabinets)
        {
            AddRow(table,
            [
                cab.Qty.ToString(),
                cab.CabinetType,
                cab.Style ?? "",
                cab.Name ?? "",
                FormatDimensionString(cab.Width, dimensionFormat),
                FormatDimensionString(cab.Height, dimensionFormat),
                FormatDimensionString(cab.Depth, dimensionFormat)
            ]);
        }

        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Run($"Total cabinets: {qtyTotal}"))
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontStyle = FontStyles.Italic
        });
    }

    private static void AddDoorTable(FlowDocument doc, IReadOnlyList<FrontPartRow> doors)
    {
        var table = CreateTable(
            headers:
            [
                "Cab #",
                "Cabinet Name",
                "Type",
                "Height",
                "Width",
                "Species",
                "Grain"
            ],
            columnWidths:
            [
                55,
                180,
                100,
                80,
                80,
                120,
                90
            ]);

        foreach (var d in doors)
        {
            AddRow(table,
            [
                d.CabinetNumber.ToString(),
                d.CabinetName ?? "",
                d.Type ?? "",
                d.DisplayHeight ?? d.Height.ToString("0.####"),
                d.DisplayWidth ?? d.Width.ToString("0.####"),
                d.Species ?? "",
                d.GrainDirection ?? ""
            ]);
        }

        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Run($"Total doors: {doors.Count}"))
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontStyle = FontStyles.Italic
        });
    }

    private static void AddDrawerBoxTable(FlowDocument doc, IReadOnlyList<DrawerBoxRow> drawerBoxes)
    {
        var table = CreateTable(
            headers:
            [
                "Cab #",
                "Cabinet Name",
                "Type",
                "Height",
                "Width",
                "Length"
            ],
            columnWidths:
            [
                55,
                190,
                110,
                85,
                85,
                90
            ]);

        foreach (var r in drawerBoxes)
        {
            AddRow(table,
            [
                r.CabinetNumber.ToString(),
                r.CabinetName ?? "",
                r.Type ?? "",
                r.DisplayHeight ?? r.Height.ToString("0.####"),
                r.DisplayWidth ?? r.Width.ToString("0.####"),
                r.DisplayLength ?? r.Length.ToString("0.####")
            ]);
        }

        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Run($"Total drawer boxes: {drawerBoxes.Count}"))
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontStyle = FontStyles.Italic
        });
    }

    private static Table CreateTable(string[] headers, double[] columnWidths)
    {
        var table = new Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0)
        };

        var group = new TableRowGroup();
        table.RowGroups.Add(group);

        for (int i = 0; i < headers.Length; i++)
        {
            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(columnWidths[i], GridUnitType.Pixel)
            });
        }

        var headerRow = new TableRow();
        group.Rows.Add(headerRow);

        for (int i = 0; i < headers.Length; i++)
        {
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(headers[i])))
            {
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(4, 3, 4, 3),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.Gainsboro
            });
        }

        return table;
    }

    private static void AddRow(Table table, string[] values)
    {
        var group = table.RowGroups.First();
        var row = new TableRow();
        group.Rows.Add(row);

        for (int i = 0; i < values.Length; i++)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(values[i] ?? string.Empty)))
            {
                Padding = new Thickness(4, 2, 4, 2),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5)
            });
        }
    }
}