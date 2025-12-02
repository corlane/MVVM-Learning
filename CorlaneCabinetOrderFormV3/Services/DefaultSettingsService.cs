using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CorlaneCabinetOrderFormV3.Services;

public partial class DefaultSettingsService : ObservableObject
{
    private const string SettingsFile = "default-settings.json";

    //Dimension Format
    [ObservableProperty] public partial string DefaultDimensionFormat { get; set; } = "Fraction";

    // Species
    [ObservableProperty] public partial string DefaultSpecies { get; set; } = "Prefinished Ply";
    [ObservableProperty] public partial string DefaultEBSpecies { get; set; } = "Wood Maple";

    //Top
    [ObservableProperty] public partial string DefaultTopType { get; set; } = "Stretcher";

    // Back
    [ObservableProperty] public partial string DefaultBaseBackThickness { get; set; } = "0.75";
    [ObservableProperty] public partial string DefaultUpperBackThickness { get; set; } = "0.75";

    //Toekick
    [ObservableProperty] public partial bool DefaultHasTK { get; set; } = true;
    [ObservableProperty] public partial string DefaultTKHeight { get; set; } = "4";
    [ObservableProperty] public partial string DefaultTKDepth { get; set; } = "3";

    // Shelves
    [ObservableProperty] public partial int DefaultShelfCount { get; set; } = 1;
    [ObservableProperty] public partial string DefaultShelfDepth { get; set; } = "Half Depth";
    [ObservableProperty] public partial bool DefaultDrillShelfHoles { get; set; } = true;

    // Openings
    [ObservableProperty] public partial string DefaultOpeningHeight1 { get; set; } = "";
    [ObservableProperty] public partial string DefaultOpeningHeight2 { get; set; } = "";
    [ObservableProperty] public partial string DefaultOpeningHeight3 { get; set; } = "";


    // Doors
    [ObservableProperty] public partial string DefaultDoorDrwSpecies { get; set; } = "Maple Ply";
    [ObservableProperty] public partial int DefaultDoorCount { get; set; } = 1;
    [ObservableProperty] public partial bool DefaultDrillHingeHoles { get; set; } = true;
    [ObservableProperty] public partial string DefaultDoorGrainDir { get; set; } = "Vertical";
    [ObservableProperty] public partial bool DefaultIncDoorsInList { get; set; } = true;
    [ObservableProperty] public partial bool DefaultIncDoors { get; set; } = true;

    // Drawers
    [ObservableProperty] public partial int DefaultStdDrawerCount { get; set; } = 1;
    [ObservableProperty] public partial int DefaultDrawerStackDrawerCount { get; set; } = 3;
    [ObservableProperty] public partial string DefaultDrwStyle { get; set; } = "Blum Tandem H/Equivalent Undermount";
    [ObservableProperty] public partial string DefaultDrwGrainDir { get; set; } = "Horizontal";
    [ObservableProperty] public partial bool DefaultIncDrwFrontsInList { get; set; } = true;
    [ObservableProperty] public partial bool DefaultIncDrwFronts { get; set; } = true;
    [ObservableProperty] public partial bool DefaultIncDrwBoxesInList { get; set; } = true;
    [ObservableProperty] public partial bool DefaultIncDrwBoxes { get; set; } = true;
    [ObservableProperty] public partial bool DefaultDrillSlideHoles { get; set; } = true;
    [ObservableProperty] public partial string DefaultDrwFrontHeight1 { get; set; } = "7";
    [ObservableProperty] public partial string DefaultDrwFrontHeight2 { get; set; } = "7";
    [ObservableProperty] public partial string DefaultDrwFrontHeight3 { get; set; } = "7";

    // Reveals and Gaps
    [ObservableProperty] public partial string DefaultBaseLeftReveal { get; set; } = ".0625";
    [ObservableProperty] public partial string DefaultBaseRightReveal { get; set; } = ".0625";
    [ObservableProperty] public partial string DefaultBaseTopReveal { get; set; } = ".4375";
    [ObservableProperty] public partial string DefaultBaseBottomReveal { get; set; } = ".0625";

    [ObservableProperty] public partial string DefaultUpperLeftReveal { get; set; } = ".0625";
    [ObservableProperty] public partial string DefaultUpperRightReveal { get; set; } = ".0625";
    [ObservableProperty] public partial string DefaultUpperTopReveal { get; set; } = ".125";
    [ObservableProperty] public partial string DefaultUpperBottomReveal { get; set; } = ".125";

    [ObservableProperty] public partial string DefaultGapWidth { get; set; } = ".125";



    // Add more defaults here as you create new properties

    public async Task LoadAsync()
    {
        if (!File.Exists(SettingsFile)) return;

        var json = await File.ReadAllTextAsync(SettingsFile);
        var loaded = JsonSerializer.Deserialize<DefaultSettingsService>(json);

        if (loaded == null) return;

        foreach (var prop in typeof(DefaultSettingsService).GetProperties(
                     System.Reflection.BindingFlags.Public |
                     System.Reflection.BindingFlags.Instance))
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(loaded);
                prop.SetValue(this, value);
            }
        }
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(SettingsFile, json);
    }


}