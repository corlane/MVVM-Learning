using System.Collections.Generic;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

public sealed record PriceBreakdownResult(
    decimal Total,
    IReadOnlyList<MaterialTotal> Lines);