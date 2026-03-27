namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Convenience look-ups on <see cref="IMaterialPricesService"/>.
/// Single source of truth for "resolve yield / sheet area for a species".
/// </summary>
public static class MaterialPricesExtensions
{
    /// <summary>
    /// Resolve the effective yield for <paramref name="species"/>.
    /// Returns 0 for None/whitespace, the species-specific override if one exists,
    /// or the server-supplied default yield as a last resort.
    /// </summary>
    public static double GetYieldForSpecies(this IMaterialPricesService prices, string species)
    {
        if (string.IsNullOrWhiteSpace(species) ||
            string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
            return 0.0;

        if (prices.TryGetYield(species, out var y))
            return y;

        return prices.DefaultSheetYield;
    }

    /// <summary>
    /// Resolve the sheet area (sq ft) for <paramref name="species"/>.
    /// Returns 0 for None/whitespace, the catalog value if known,
    /// or <see cref="MaterialDefaults.DefaultSheetAreaSqFt"/> as a last resort.
    /// </summary>
    public static double GetSheetAreaForSpecies(this IMaterialPricesService prices, string species)
    {
        if (string.IsNullOrWhiteSpace(species) ||
            string.Equals(species, "None", StringComparison.OrdinalIgnoreCase))
            return 0.0;

        if (prices.TryGetSheetMaterial(species, out var row))
        {
            var areaSqIn = row.SheetWidthIn * row.SheetLengthIn;
            if (areaSqIn > 0)
                return areaSqIn / 144.0;
        }

        return MaterialDefaults.DefaultSheetAreaSqFt;
    }
}