using System.Net.Http;
using System.Text.Json;

namespace CorlaneCabinetOrderFormV3.Services;

/// <summary>
/// Single source of truth for Corlane web API infrastructure:
/// shared HttpClient, URL constants, and JSON options.
/// </summary>
internal static class CorlaneApi
{
    /// <summary>
    /// Shared HttpClient for all Corlane API calls.
    /// 15 s default timeout; callers needing a shorter deadline should use a per-request CancellationTokenSource.
    /// </summary>
    internal static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    // ── Material Prices endpoints ────────────────────────────────────────

    internal const string MaterialPricesBaseUrl = "https://corlanecabinetry.com/matprices/";
    internal const string MaterialPricesFileName = "material-prices.json";

    internal static readonly Uri PricesUri =
        new(new Uri(MaterialPricesBaseUrl), MaterialPricesFileName);

    internal static readonly Uri UploadPricesUri =
        new(new Uri(MaterialPricesBaseUrl), "upload-material-prices.php");

    // ── Job Orders endpoints ─────────────────────────────────────────────

    internal const string JobOrdersBaseUrl = "https://corlanecabinetry.com/joborders/";

    internal static readonly Uri UploadJobUri =
        new(new Uri(JobOrdersBaseUrl), "upload-job.php");

    // ── Connectivity probe ───────────────────────────────────────────────

    internal const string ProbeUrl = "https://www.corlanecabinetry.com";

    // ── JSON options ─────────────────────────────────────────────────────

    internal static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}