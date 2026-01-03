using System.Text.Json.Serialization;

namespace CorlaneCabinetOrderFormV3.Models;

public sealed record FrontPartRow(
    int CabinetNumber,
    string CabinetName,
    string Type,
    double Height,
    double Width,
    string Species,
    string GrainDirection)
{
    [JsonIgnore]
    public string DisplayWidth { get; init; } = "";

    [JsonIgnore]
    public string DisplayHeight { get; init; } = "";

    [JsonIgnore]
    public string DisplaySize { get; init; } = "";
}