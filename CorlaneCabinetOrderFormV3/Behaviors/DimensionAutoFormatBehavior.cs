using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Behaviors;

/// <summary>
/// Attached behavior that reformats a dimension TextBox's value
/// to the user's default format (Fraction or Decimal) on LostFocus
/// and automatically when the default dimension format changes.
/// Usage: &lt;TextBox appbehaviors:DimensionAutoFormat.IsEnabled="True" /&gt;
/// </summary>
public static class DimensionAutoFormat
{
    // All TextBoxes with the behavior attached
    private static readonly List<WeakReference<TextBox>> s_registeredBoxes = [];
    private static bool s_listeningToDefaults;

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DimensionAutoFormat),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
        {
            tb.LostFocus += TextBox_LostFocus;
            s_registeredBoxes.Add(new WeakReference<TextBox>(tb));
            EnsureListeningToDefaults();

            // Format on first load so the initial value matches the default format
            if (tb.IsLoaded)
                ReformatTextBox(tb, GetCurrentFormat());
            else
                tb.Loaded += FormatOnInitialLoad;
        }
        else
        {
            tb.LostFocus -= TextBox_LostFocus;
            RemoveBox(tb);
        }
    }

    private static void FormatOnInitialLoad(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        tb.Loaded -= FormatOnInitialLoad; // one-shot
        ReformatTextBox(tb, GetCurrentFormat());
    }

    private static void EnsureListeningToDefaults()
    {
        if (s_listeningToDefaults) return;

        try
        {
            if (App.ServiceProvider.GetService(typeof(DefaultSettingsService)) is DefaultSettingsService defaults)
            {
                PropertyChangedEventManager.AddHandler(
                    defaults,
                    Defaults_PropertyChanged,
                    nameof(DefaultSettingsService.DefaultDimensionFormat));
                s_listeningToDefaults = true;
            }
        }
        catch { /* design-time */ }
    }

    private static void Defaults_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DefaultSettingsService.DefaultDimensionFormat)) return;

        string format = (sender as DefaultSettingsService)?.DefaultDimensionFormat ?? "Fraction";

        for (int i = s_registeredBoxes.Count - 1; i >= 0; i--)
        {
            if (s_registeredBoxes[i].TryGetTarget(out var tb))
            {
                if (tb.IsLoaded)
                    ReformatTextBox(tb, format);
                else
                    tb.Loaded += ReformatOnLoaded; // reformat when tab becomes visible
            }
            else
            {
                s_registeredBoxes.RemoveAt(i); // clean up GC'd refs
            }
        }
    }

    private static void ReformatOnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        tb.Loaded -= ReformatOnLoaded; // one-shot
        ReformatTextBox(tb, GetCurrentFormat());
    }

    private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        ReformatTextBox(tb, GetCurrentFormat());
    }

    private static void ReformatTextBox(TextBox tb, string format)
    {
        var raw = tb.Text;
        if (string.IsNullOrWhiteSpace(raw)) return;

        double parsed = ConvertDimension.FractionToDouble(raw);

        // Don't reformat if parsing failed (0 result but input wasn't literally "0")
        if (parsed == 0 && raw.Trim() != "0") return;

        string formatted = format.Equals("Decimal", StringComparison.OrdinalIgnoreCase)
            ? parsed.ToString("0.####")
            : ConvertDimension.DoubleToFraction(parsed);

        // Only touch the TextBox if the display actually changed
        if (formatted != raw)
        {
            tb.Text = formatted;
            // Push the reformatted string back to the ViewModel
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }

    private static string GetCurrentFormat()
    {
        try
        {
            if (App.ServiceProvider.GetService(typeof(DefaultSettingsService)) is DefaultSettingsService defaults
                && !string.IsNullOrWhiteSpace(defaults.DefaultDimensionFormat))
            {
                return defaults.DefaultDimensionFormat;
            }
        }
        catch { /* design-time */ }
        return "Fraction";
    }

    private static void RemoveBox(TextBox tb)
    {
        for (int i = s_registeredBoxes.Count - 1; i >= 0; i--)
        {
            if (!s_registeredBoxes[i].TryGetTarget(out var target) || ReferenceEquals(target, tb))
                s_registeredBoxes.RemoveAt(i);
        }
    }
}