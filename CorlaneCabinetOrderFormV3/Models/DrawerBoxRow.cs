using System.Text.Json.Serialization;

namespace CorlaneCabinetOrderFormV3.Models;

public sealed record DrawerBoxRow(
    int CabinetNumber,
    string CabinetName,
    string Type,
    double Height,
    double Width,
    double Length)
{
    [JsonIgnore]
    public string DisplayHeight { get; init; } = "";

    [JsonIgnore]
    public string DisplayWidth { get; init; } = "";

    [JsonIgnore]
    public string DisplayLength { get; init; } = "";
}