using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

// IMaterialPricesService.cs
// Defines the contract for the material pricing service that fetches and exposes
// current sheet material and edge banding prices used to calculate job cost estimates.
//
// Prices are fetched from the Corlane API server (RefreshFromServerAsync) and cached
// in memory for the session. Until a successful fetch, HasPrices is false and the
// quoting UI disables or hides price-dependent fields accordingly.
//
// The concrete implementation (MaterialPricesService) is thread-safe — all data is
// guarded by a lock so that a background refresh never races against the UI thread
// reading prices for a quote calculation.
//
// Contract covers:
//   - SheetMaterials: per-species sheet material prices (price/sq ft, sheet dimensions)
//     used to calculate material cost from the accumulated square footage per cabinet
//   - EdgeBanding: per-species edge banding prices (price/ft) used to calculate EB cost
//     from the accumulated linear footage per cabinet
//   - CncPricePerSheet: flat CNC cutting charge applied per sheet consumed
//   - DefaultSheetYield: default waste factor (e.g. 0.85 = 85% yield) applied when no
//     species-specific yield is available
//   - YieldBySpecies: per-species yield overrides for more accurate sheet count estimates
//   - HasPrices: false until at least one successful server fetch; gates price display in UI
//   - RefreshFromServerAsync: fetches the latest prices from the Corlane API and replaces
//     the cached data atomically under lock
//   - TryGetSheetMaterial / TryGetEdgeBand / TryGetYield: safe lookup helpers used by
//     the quoting/estimating logic to retrieve a single species' pricing data

public interface IMaterialPricesService
{
    IReadOnlyList<MaterialPriceRow> SheetMaterials { get; }
    IReadOnlyList<EdgeBandPriceRow> EdgeBanding { get; }

    decimal CncPricePerSheet { get; }
    double DefaultSheetYield { get; }
    IReadOnlyDictionary<string, double> YieldBySpecies { get; }

    bool HasPrices { get; }

    Task RefreshFromServerAsync(CancellationToken cancellationToken = default);

    bool TryGetSheetMaterial(string species, out MaterialPriceRow row);
    bool TryGetEdgeBand(string species, out EdgeBandPriceRow row);
    bool TryGetYield(string species, out double yield);
}