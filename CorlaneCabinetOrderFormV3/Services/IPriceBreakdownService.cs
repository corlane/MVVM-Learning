namespace CorlaneCabinetOrderFormV3.Services;

public interface IPriceBreakdownService
{
    PriceBreakdownResult Build(
        Dictionary<string, double> materialsSqFtBySpecies,
        Dictionary<string, double> edgebandingFeetBySpecies);
}