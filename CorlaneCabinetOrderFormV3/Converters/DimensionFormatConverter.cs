using System;
using System.Globalization;
using System.Windows.Data;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Converters;

namespace CorlaneCabinetOrderFormV3.Converters;

public sealed class DimensionFormatConverter : IValueConverter
{
    // value: incoming dimension string (e.g. "34.5" or "1 1/2")
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Parse to double (handles fractions and decimals)
        double parsed = ConvertDimension.FractionToDouble(raw);

        // Try to obtain defaults from DI; fall back to Fraction
        string format = "Fraction";
        try
        {
            var defaults = App.ServiceProvider.GetService(typeof(DefaultSettingsService)) as DefaultSettingsService;
            if (defaults != null && !string.IsNullOrWhiteSpace(defaults.DefaultDimensionFormat))
                format = defaults.DefaultDimensionFormat;
        }
        catch
        {
            // ignore DI errors, fall back to default
        }

        if (format.Equals("Decimal", StringComparison.OrdinalIgnoreCase))
        {
            // Use up to two decimals (adjust formatting if you prefer different precision)
            return parsed.ToString("0.####", CultureInfo.CurrentCulture);
        }

        // Default: fraction display using existing helper
        try
        {
            return ConvertDimension.DoubleToFraction(parsed);
        }
        catch
        {
            // Fallback to decimal if fraction conversion fails
            return parsed.ToString("0.##", CultureInfo.CurrentCulture);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Two-way binding not supported for dimension formatting.");
    }
}