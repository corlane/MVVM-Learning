using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Services;

// PriceBreakdownResult.cs
// Immutable result record returned by the pricing service after calculating
// the full cost breakdown for a cabinet order. Carries the grand total price
// (Total) alongside a read-only line-item list (Lines) of per-material totals
// (MaterialTotal), allowing the UI and report layers to display both a summary
// figure and a detailed material-by-material cost breakdown without mutating
// the source data.

public sealed record PriceBreakdownResult(
    decimal Total,
    IReadOnlyList<MaterialTotal> Lines);