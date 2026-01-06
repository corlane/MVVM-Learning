using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

public sealed class MaterialPricesService : IMaterialPricesService
{
    private static readonly HttpClient s_httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private const string MaterialPricesBaseUrl = "https://corlanecabinetry.com/matprices/";
    private const string MaterialPricesFileName = "material-prices.json";
    private static readonly Uri s_pricesUri = new(new Uri(MaterialPricesBaseUrl), MaterialPricesFileName);

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly object _gate = new();

    private List<MaterialPriceRow> _sheetMaterials = [];
    private List<EdgeBandPriceRow> _edgeBanding = [];
    private Dictionary<string, double> _yieldBySpecies = new(StringComparer.OrdinalIgnoreCase);

    private decimal _cncPricePerSheet = 60m;
    private double _defaultSheetYield = 0.82;
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
        var json = await s_httpClient.GetStringAsync(s_pricesUri, cancellationToken).ConfigureAwait(false);
        var dto = JsonSerializer.Deserialize<MaterialPricesDto>(json, s_jsonOptions);
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

            _cncPricePerSheet = dto.CncCutting?.PricePerSheet ?? 60m;
            _defaultSheetYield = dto.Yields?.DefaultSheetYield ?? 0.82;

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

    private sealed class MaterialPricesDto
    {
        [JsonPropertyName("sheetMaterials")]
        public List<SheetMaterialPriceDto>? SheetMaterials { get; set; }

        [JsonPropertyName("edgeBanding")]
        public List<EdgeBandingPriceDto>? EdgeBanding { get; set; }

        [JsonPropertyName("cncCutting")]
        public CncCuttingDto? CncCutting { get; set; }

        [JsonPropertyName("yields")]
        public YieldsDto? Yields { get; set; }
    }

    private sealed class SheetMaterialPriceDto
    {
        [JsonPropertyName("species")]
        public string? Species { get; set; }

        [JsonPropertyName("pricePerSqFt")]
        public decimal PricePerSqFt { get; set; }

        [JsonPropertyName("sheetWidthIn")]
        public double SheetWidthIn { get; set; }

        [JsonPropertyName("sheetLengthIn")]
        public double SheetLengthIn { get; set; }
    }

    private sealed class EdgeBandingPriceDto
    {
        [JsonPropertyName("species")]
        public string? Species { get; set; }

        [JsonPropertyName("pricePerFt")]
        public decimal PricePerFt { get; set; }
    }

    private sealed class CncCuttingDto
    {
        [JsonPropertyName("pricePerSheet")]
        public decimal PricePerSheet { get; set; }
    }

    private sealed class YieldsDto
    {
        [JsonPropertyName("defaultSheetYield")]
        public double DefaultSheetYield { get; set; }

        [JsonPropertyName("yieldBySpecies")]
        public Dictionary<string, double>? YieldBySpecies { get; set; }
    }
}