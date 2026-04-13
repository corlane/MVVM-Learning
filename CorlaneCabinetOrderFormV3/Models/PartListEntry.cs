namespace CorlaneCabinetOrderFormV3.Models;

public sealed class PartListEntry
{
    /// <summary>Used for DataGrid grouping.</summary>
    public string CabinetLabel { get; init; } = "";
    public string PartName { get; init; } = "";
    public int Qty { get; init; } = 1;
    public string Species { get; init; } = "";
    public string Length { get; init; } = "";      // along grain
    public string Width { get; init; } = "";       // across grain
    public string Thickness { get; init; } = "";
    public string Notes { get; init; } = "";
    public string EdgeBandSpecies { get; init; } = "";
    public string EdgeBandLength { get; init; } = "";
}