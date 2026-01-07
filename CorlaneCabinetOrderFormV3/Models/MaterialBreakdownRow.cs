namespace CorlaneCabinetOrderFormV3.Models;

public sealed class MaterialBreakdownRow
{
    public string Species { get; init; } = "";

    // Sheet goods
    public double SqFt { get; init; }
    public int Sheets { get; init; }

    // Edgebanding
    public double LinearFeet { get; init; }
}