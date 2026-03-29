using CorlaneCabinetOrderFormV3.Models;
using System.Text;
using System.Windows;

namespace CorlaneCabinetOrderFormV3.Views;

public partial class PartsListWindow : Window
{
    public PartsListWindow()
    {
        InitializeComponent();
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not List<PartListEntry> entries) return;

        var sb = new StringBuilder();
        sb.AppendLine("Cabinet\tPart\tQty\tSpecies\tLength\tWidth\tThickness\tNotes");

        foreach (var entry in entries)
        {
            sb.AppendLine(string.Join('\t',
                entry.CabinetLabel,
                entry.PartName,
                entry.Qty,
                entry.Species,
                entry.Length,
                entry.Width,
                entry.Thickness,
                entry.Notes));
        }

        Clipboard.SetText(sb.ToString());
        MessageBox.Show("Parts list copied to clipboard (tab-delimited).",
            "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}