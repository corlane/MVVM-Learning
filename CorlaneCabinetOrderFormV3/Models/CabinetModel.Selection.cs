using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class CabinetModel
{
    /// <summary>UI-only flag for multi-select batch operations. Not persisted.</summary>
    [ObservableProperty]
    [JsonIgnore]
    public partial bool IsSelected { get; set; }
}