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


}
