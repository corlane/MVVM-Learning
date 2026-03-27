using System.Text.Json.Serialization;

namespace CorlaneCabinetOrderFormV3.Services;

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