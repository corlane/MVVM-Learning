namespace CorlaneCabinetOrderFormV3.Services;

// IPriceBreakdownService.cs
// Defines the contract for the service that converts accumulated material and edge banding
// usage totals into a priced line-item breakdown for the job quote.
//
// Called by the quoting ViewModel after AccumulateAllMaterialAndEdgeTotals() has run,
// passing in two dictionaries: total square footage consumed per sheet species, and total
// linear feet consumed per edge banding species.
//
// The concrete implementation (PriceBreakdownService) applies the following logic:
//   - Sheet materials: converts sq ft to whole sheet count (ceiling) using per-species
//     yield factors, looks up price per sq ft from IMaterialPricesService, and produces
//     a "Sheets" line item per species
//   - Edge banding: looks up price per linear foot and produces a "ft" line item per species
//   - CNC cutting: charges a flat rate per sheet of billable area cut (rounded up to whole
//     sheets per species, then summed), expressed as a per-sq-ft rate against total
//     billable area
//
// Returns a PriceBreakdownResult containing the rounded grand total and the full list of
// MaterialTotal line items ready for display in the quote breakdown UI.

public interface IPriceBreakdownService
{
    PriceBreakdownResult Build(
        Dictionary<string, double> materialsSqFtBySpecies,
        Dictionary<string, double> edgebandingFeetBySpecies);
}