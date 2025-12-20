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
    [ObservableProperty] public partial string EBSpecies { get; set; } 
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
    // Optional: Add shared methods, e.g., CalculateVolume() if needed



    // --- Material accumulators (populated by the 3D builder) ---

    // Accumulated face area per species, in square feet (ft^2).
    // Populated by Cabinet3DViewModel.CreatePanel(...) while building the visual model.
    [JsonIgnore]
    public Dictionary<string, double> MaterialAreaBySpecies { get; } = new(StringComparer.OrdinalIgnoreCase);

    // Accumulated edgebanding length per edgebanding species, in linear feet (ft).
    // Populated by Cabinet3DViewModel.CreatePanel(...) when edgebanding is applied to edges.
    [JsonIgnore]
    public Dictionary<string, double> EdgeBandingLengthBySpecies { get; } = new(StringComparer.OrdinalIgnoreCase);



    // Convenience helpers to clear accumulators before rebuilds
    public void ResetMaterialTotals() => MaterialAreaBySpecies.Clear();
    public void ResetEdgeBandingTotals() => EdgeBandingLengthBySpecies.Clear();
    public void ResetAllMaterialAndEdgeTotals()
    {
        MaterialAreaBySpecies.Clear();
        EdgeBandingLengthBySpecies.Clear();
    }

    // Convenience computed totals
    [JsonIgnore]
    public double TotalMaterialAreaFt2 => MaterialAreaBySpecies.Values.Sum();

    [JsonIgnore]
    public double TotalEdgeBandingFeet => EdgeBandingLengthBySpecies.Values.Sum();

}
