using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

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