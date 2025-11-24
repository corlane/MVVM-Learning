using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;  // For polymorphism in save/load

namespace MVVM_Learning.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]  // JSON setup for subtypes
[JsonDerivedType(typeof(BaseCabinetModel), "BaseCabinet")]
[JsonDerivedType(typeof(UpperCabinetModel), "UpperCabinet")]
//[JsonDerivedType(typeof(PanelModel), "Panel")]
//[JsonDerivedType(typeof(FillerModel), "Filler")]
// Add entries for future subtypes as you create them
public abstract partial class CabinetModel : ObservableObject
{
    // These properties are common to all cabinet types
    [ObservableProperty] public partial string Width { get; set; } = "";
    [ObservableProperty] public partial string Height { get; set; } = "";
    [ObservableProperty] public partial string Depth { get; set; } = "";
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string Name { get; set; } = "";
    [ObservableProperty] public partial int Qty { get; set; }

    // Optional: Add shared methods, e.g., CalculateVolume() if needed
}