using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class CabinetModel
{
    /// <summary>Cached thumbnail rendered by ThumbnailService. UI-only, not persisted.</summary>
    [ObservableProperty]
    [JsonIgnore]
    public partial BitmapSource? Thumbnail { get; set; }
}