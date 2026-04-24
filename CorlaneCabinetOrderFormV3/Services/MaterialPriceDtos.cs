using System.Text.Json.Serialization;

namespace CorlaneCabinetOrderFormV3.Services;

// MaterialPriceDtos.cs
// Internal JSON deserialization DTOs for the material-prices.json file hosted on the
// Corlane API server.
//
// DTO (Data Transfer Object): a plain class with no logic whose sole purpose is to
// carry data across a boundary — in this case, mapping the raw JSON fields from the
// server response into typed C# properties so System.Text.Json can deserialize them
// directly. DTOs are intentionally dumb; they are immediately mapped into proper domain
// models (MaterialPriceRow, EdgeBandPriceRow) before the rest of the app touches the data.
//
// Used in two places:
//   - MaterialPricesService.RefreshFromServerAsync(): downloads and deserializes the
//     prices file, then maps these DTOs into the domain model types that the rest of
//     the app consumes for quoting and price breakdown calculations
//   - ProcessOrderViewModel (admin): reads and writes the raw DTO structure for the
//     admin upload/download UI that lets Corlane staff update the hosted prices file
//
// Structure mirrors the JSON layout:
//   sheetMaterials  → List<SheetMaterialPriceDto>  (species, price/sq ft, sheet dimensions)
//   edgeBanding     → List<EdgeBandingPriceDto>     (species, price/ft)
//   cncCutting      → CncCuttingDto                 (flat CNC charge per sheet)
//   yields          → YieldsDto                     (default yield + per-species overrides)
//
// All classes are internal and sealed — they are implementation details of the pricing
// pipeline and are not exposed outside the Services layer.

/// <summary>
/// JSON DTOs for material-prices.json.
/// Shared by <see cref="MaterialPricesService"/> (download/parse)
/// and ProcessOrderViewModel (admin upload/download UI).
/// </summary>
internal sealed class MaterialPricesDto
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

internal sealed class SheetMaterialPriceDto
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

internal sealed class EdgeBandingPriceDto
{
    [JsonPropertyName("species")]
    public string? Species { get; set; }

    [JsonPropertyName("pricePerFt")]
    public decimal PricePerFt { get; set; }
}

internal sealed class CncCuttingDto
{
    [JsonPropertyName("pricePerSheet")]
    public decimal PricePerSheet { get; set; }
}

internal sealed class YieldsDto
{
    [JsonPropertyName("defaultSheetYield")]
    public double DefaultSheetYield { get; set; }

    [JsonPropertyName("yieldBySpecies")]
    public Dictionary<string, double>? YieldBySpecies { get; set; }
}