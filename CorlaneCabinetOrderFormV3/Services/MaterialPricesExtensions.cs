namespace CorlaneCabinetOrderFormV3.Services;

// MaterialPricesExtensions.cs
// Extension methods on IMaterialPricesService that provide the single source of truth
// for resolving yield and sheet area for a given species. Centralizing this logic here
// prevents each call site (PriceBreakdownService, quoting VMs, etc.) from duplicating
// the same fallback chain every time it needs these values.
//
// GetYieldForSpecies(species):
//   Returns the effective material yield (waste factor) for the given species.
//   Resolution order: None/blank → 0.0, species-specific override from server data,
//   then server-supplied DefaultSheetYield as a fallback.
//   Yield is used to gross up the net sq ft consumed into whole sheet counts
//   (e.g., 0.85 yield means divide net sq ft by 0.85 before rounding up to sheets).
//
// GetSheetAreaForSpecies(species):
//   Returns the physical sheet area in sq ft for the given species.
//   Resolution order: None/blank → 0.0, catalog sheet dimensions (width × length in
//   inches ÷ 144) from server data, then MaterialDefaults.DefaultSheetAreaSqFt as a
//   fallback (assumes a standard 4×8 sheet).
//   Sheet area is used together with yield to convert sq ft totals into a sheet count
//   for both pricing (PriceBreakdownService) and CNC cutting charge calculations.

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