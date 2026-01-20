using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;  // For polymorphism in save/load

namespace CorlaneCabinetOrderFormV3.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]  // JSON setup for subtypes
[JsonDerivedType(typeof(BaseCabinetModel), "BaseCabinet")]
[JsonDerivedType(typeof(UpperCabinetModel), "UpperCabinet")]
[JsonDerivedType(typeof(FillerModel), "Filler")]
[JsonDerivedType(typeof(PanelModel), "Panel")]
// Add entries for future subtypes as you create them

public abstract partial class CabinetModel : ObservableObject
{
    // These properties are common to all cabinet types
    [ObservableProperty] public partial double MaterialThickness34 { get; set; } = 0.75;
    [ObservableProperty] public partial double MaterialThickness14 { get; set; } = 0.25;
    [ObservableProperty] public partial string Width { get; set; }
    [ObservableProperty] public partial string Height { get; set; }
    [ObservableProperty] public partial string Depth { get; set; }
    [ObservableProperty] public partial string Species { get; set; }
    [ObservableProperty] public partial string CustomSpecies { get; set; }
    [ObservableProperty] public partial string EBSpecies { get; set; }
    [ObservableProperty] public partial string CustomEBSpecies { get; set; }
    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial int Qty { get; set; }
    [ObservableProperty] public partial string Notes { get; set; }
    [ObservableProperty] public partial string Style { get; set; }

    public virtual string CabinetType =>
        this switch
        {
            BaseCabinetModel => "Base Cabinet",
            UpperCabinetModel => "Upper Cabinet",
            PanelModel => "Panel",
            FillerModel => "Filler",
            _ => "Unknown"
        };

    // --- Material accumulators (populated by the 3D builder) ---

    [JsonIgnore]
    public Dictionary<string, double> MaterialAreaBySpecies { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public Dictionary<string, double> EdgeBandingLengthBySpecies { get; } = new(StringComparer.OrdinalIgnoreCase);

    // Door Sizes accumulator (populated by the 3D builder)
    [JsonIgnore]
    public List<FrontPartRow> FrontParts { get; } = new();

    [JsonIgnore]
    public List<DrawerBoxRow> DrawerBoxes { get; } = new();

    public void ResetFrontParts() => FrontParts.Clear();
    public void ResetDrawerBoxes() => DrawerBoxes.Clear();

    public void ResetAllMaterialAndEdgeTotals()
    {
        MaterialAreaBySpecies.Clear();
        EdgeBandingLengthBySpecies.Clear();
        FrontParts.Clear();
        DrawerBoxes.Clear();
    }
    // Convenience computed totals
    [JsonIgnore]
    public double TotalMaterialAreaFt2 => MaterialAreaBySpecies.Values.Sum();

    [JsonIgnore]
    public double TotalEdgeBandingFeet => EdgeBandingLengthBySpecies.Values.Sum();
}