namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Pure math for sheet goods and edgebanding yield calculations.
/// Extracted from POJobMaterialListViewModel so it can be unit-tested.
/// </summary>
public static class MaterialYieldCalculator
{
    /// <summary>
    /// Compute number of sheets needed, rounding up.
    /// </summary>
    public static int ComputeSheetCount(double totalSqFt, double sheetAreaSqFt, double yield)
    {
        if (sheetAreaSqFt <= 0 || yield <= 0)
            return 0;

        return (int)Math.Ceiling((totalSqFt / yield) / sheetAreaSqFt);
    }

    /// <summary>
    /// Aggregate material areas across cabinets, keyed by species.
    /// Resolves "Custom" keys to the user-entered custom name.
    /// </summary>
    public static Dictionary<string, double> AggregateMaterialAreas(
        IEnumerable<CabinetMaterialSnapshot> cabinets)
    {
        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var cab in cabinets)
        {
            foreach (var kv in cab.MaterialAreaBySpecies)
            {
                var species = ResolveCustomSpeciesKey(kv.Key, cab.CustomSpecies);
                var area = kv.Value * cab.Qty;

                if (result.TryGetValue(species, out var existing))
                    result[species] = existing + area;
                else
                    result[species] = area;
            }
        }

        return result;
    }

    /// <summary>
    /// Aggregate edgebanding lengths across cabinets, keyed by species.
    /// </summary>
    public static Dictionary<string, double> AggregateEdgeBanding(
        IEnumerable<CabinetMaterialSnapshot> cabinets)
    {
        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var cab in cabinets)
        {
            foreach (var kv in cab.EdgeBandingBySpecies)
            {
                var species = ResolveCustomEBKey(kv.Key, cab.CustomEBSpecies);
                var feet = kv.Value * cab.Qty;

                if (result.TryGetValue(species, out var existing))
                    result[species] = existing + feet;
                else
                    result[species] = feet;
            }
        }

        return result;
    }

    /// <summary>
    /// Collapse UP/DOWN suffixes for combined sheet totals.
    /// </summary>
    public static string CollapseFaceKey(string species)
    {
        if (string.IsNullOrWhiteSpace(species))
            return "None";

        var s = species.Trim();
        if (s.EndsWith(" UP", StringComparison.OrdinalIgnoreCase))
            return s[..^3].TrimEnd();
        if (s.EndsWith(" DOWN", StringComparison.OrdinalIgnoreCase))
            return s[..^5].TrimEnd();
        return s;
    }

    private static string ResolveCustomSpeciesKey(string key, string? customSpecies)
    {
        var raw = NormalizeBlank(key);
        string suffix = "", baseKey = raw;

        if (baseKey.EndsWith(" UP", StringComparison.OrdinalIgnoreCase))
        { suffix = " UP"; baseKey = baseKey[..^3].TrimEnd(); }
        else if (baseKey.EndsWith(" DOWN", StringComparison.OrdinalIgnoreCase))
        { suffix = " DOWN"; baseKey = baseKey[..^5].TrimEnd(); }

        if (!IsCustom(baseKey)) return raw;

        var custom = NormalizeBlank(customSpecies);
        if (string.Equals(custom, "None", StringComparison.OrdinalIgnoreCase))
            custom = "Custom";

        return $"{custom}{suffix}";
    }

    private static string ResolveCustomEBKey(string key, string? customEB)
    {
        var k = NormalizeBlank(key);
        if (!IsCustom(k)) return k;

        var custom = NormalizeBlank(customEB);
        return string.Equals(custom, "None", StringComparison.OrdinalIgnoreCase) ? "Custom" : custom;
    }

    private static string NormalizeBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? "None" : s.Trim();

    private static bool IsCustom(string s) =>
        string.Equals(s?.Trim(), "Custom", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Lightweight snapshot of a cabinet's material data — no WPF dependencies.
/// </summary>
public record CabinetMaterialSnapshot(
    int Qty,
    string? CustomSpecies,
    string? CustomEBSpecies,
    Dictionary<string, double> MaterialAreaBySpecies,
    Dictionary<string, double> EdgeBandingBySpecies);