using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CorlaneCabinetOrderFormV3.Services;

public partial class DefaultSettingsService : ObservableObject
{
    private const string SettingsFile = "default-settings.json";

    [ObservableProperty] public partial string DefaultSpecies { get; set; } = "Prefinished Ply";
    [ObservableProperty] public partial string DefaultEBSpecies { get; set; } = "Wood Maple";

    // Add more defaults here as you create new properties

    public async Task LoadAsync()
    {
        if (File.Exists(SettingsFile))
        {
            var json = await File.ReadAllTextAsync(SettingsFile);
            var loaded = JsonSerializer.Deserialize<DefaultSettingsService>(json);
            if (loaded != null)
            {

                DefaultSpecies = loaded.DefaultSpecies;
                DefaultEBSpecies = loaded.DefaultEBSpecies;
                // copy all
            }
        }
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(SettingsFile, json);
    }


}