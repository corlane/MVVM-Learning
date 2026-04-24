using CorlaneCabinetOrderFormV3.Models;
using System.Text.Json;

namespace CorlaneCabinetOrderFormV3.Services;

// MaterialPricesService.cs
// Concrete implementation of IMaterialPricesService. Fetches and caches the current
// material and edge banding price data from the Corlane API server for use in job
// quoting and price breakdown calculations.
//
// All cached data starts empty (HasPrices = false) and is populated by calling
// RefreshFromServerAsync(), which is triggered at app startup and optionally by the
// admin UI in ProcessOrderViewModel. Until a successful fetch, quoting features that
// depend on prices are disabled or hidden in the UI.
//
// RefreshFromServerAsync() downloads material-prices.json, deserializes it via
// MaterialPriceDtos, maps the DTOs into domain model types (MaterialPriceRow,
// EdgeBandPriceRow), and replaces all cached data atomically under lock.
//
// Thread safety: all private fields are guarded by a single lock (_gate). Read
// properties return defensive copies (ToList, new Dictionary) so callers can't
// mutate the cached data. TryGet methods also run under the same lock. This allows
// RefreshFromServerAsync to run on a background thread without racing against the
// UI thread reading prices for a quote calculation.
//
// Species lookups are case-insensitive throughout (StringComparer.OrdinalIgnoreCase)
// to match loosely against species strings that may come from user input or saved
// job files with inconsistent casing.
//
// Falls back to MaterialDefaults constants (DefaultCncPricePerSheet, DefaultYield,
// DefaultSheetAreaSqFt) when the server data is missing or a species is not found,
// so the quoting logic always has a usable value even without a successful fetch.

public sealed class MaterialPricesService : IMaterialPricesService
{
    private readonly object _gate = new();

    private List<MaterialPriceRow> _sheetMaterials = [];
    private List<EdgeBandPriceRow> _edgeBanding = [];
    private Dictionary<string, double> _yieldBySpecies = new(StringComparer.OrdinalIgnoreCase);

    private decimal _cncPricePerSheet = MaterialDefaults.DefaultCncPricePerSheet;
    private double _defaultSheetYield = MaterialDefaults.DefaultYield;
    private bool _hasPrices;

    public IReadOnlyList<MaterialPriceRow> SheetMaterials
    {
        get { lock (_gate) return _sheetMaterials.ToList(); }
    }

    public IReadOnlyList<EdgeBandPriceRow> EdgeBanding
    {
        get { lock (_gate) return _edgeBanding.ToList(); }
    }

    public decimal CncPricePerSheet
    {
        get { lock (_gate) return _cncPricePerSheet; }
    }

    public double DefaultSheetYield
    {
        get { lock (_gate) return _defaultSheetYield; }
    }

    public IReadOnlyDictionary<string, double> YieldBySpecies
    {
        get { lock (_gate) return new Dictionary<string, double>(_yieldBySpecies, StringComparer.OrdinalIgnoreCase); }
    }

    public bool HasPrices
    {
        get { lock (_gate) return _hasPrices; }
    }

    public async Task RefreshFromServerAsync(CancellationToken cancellationToken = default)
    {
        var json = await CorlaneApi.HttpClient.GetStringAsync(CorlaneApi.PricesUri, cancellationToken).ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<MaterialPricesDto>(json, CorlaneApi.JsonReadOptions);
        if (dto == null)
        {
            return;
        }

        var sheet = dto.SheetMaterials?.Select(s => new MaterialPriceRow
        {
            Species = s.Species ?? "",
            PricePerSqFt = s.PricePerSqFt,
            SheetWidthIn = s.SheetWidthIn,
            SheetLengthIn = s.SheetLengthIn
        }).ToList() ?? [];

        var edge = dto.EdgeBanding?.Select(e => new EdgeBandPriceRow
        {
            Species = e.Species ?? "",
            PricePerFt = e.PricePerFt
        }).ToList() ?? [];

        var yields = dto.Yields?.YieldBySpecies ?? new Dictionary<string, double>();

        lock (_gate)
        {
            _sheetMaterials = sheet;
            _edgeBanding = edge;

            _cncPricePerSheet = dto.CncCutting?.PricePerSheet ?? MaterialDefaults.DefaultCncPricePerSheet;
            _defaultSheetYield = dto.Yields?.DefaultSheetYield ?? MaterialDefaults.DefaultYield;

            _yieldBySpecies = new Dictionary<string, double>(yields, StringComparer.OrdinalIgnoreCase);

            _hasPrices = (_sheetMaterials.Count > 0) || (_edgeBanding.Count > 0);
        }
    }

    public bool TryGetSheetMaterial(string species, out MaterialPriceRow row)
    {
        lock (_gate)
        {
            var found = _sheetMaterials.FirstOrDefault(s => string.Equals(s.Species, species, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                row = null!;
                return false;
            }

            row = new MaterialPriceRow
            {
                Species = found.Species,
                PricePerSqFt = found.PricePerSqFt,
                SheetWidthIn = found.SheetWidthIn,
                SheetLengthIn = found.SheetLengthIn
            };
            return true;
        }
    }

    public bool TryGetEdgeBand(string species, out EdgeBandPriceRow row)
    {
        lock (_gate)
        {
            var found = _edgeBanding.FirstOrDefault(s => string.Equals(s.Species, species, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                row = null!;
                return false;
            }

            row = new EdgeBandPriceRow
            {
                Species = found.Species,
                PricePerFt = found.PricePerFt
            };
            return true;
        }
    }

    public bool TryGetYield(string species, out double yield)
    {
        lock (_gate)
        {
            return _yieldBySpecies.TryGetValue(species, out yield);
        }
    }
}