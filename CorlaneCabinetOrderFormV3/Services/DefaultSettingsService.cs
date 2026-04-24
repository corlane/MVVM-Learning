using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json;

namespace CorlaneCabinetOrderFormV3.Services;

// DefaultSettingsService.cs
// Persists and exposes all user-configurable default settings for the cabinet order form.
// Settings are saved to a JSON file in the user's LocalApplicationData folder
// (%LOCALAPPDATA%\CorlaneCabinetOrderFormV3\default-settings.json) and survive across
// application sessions.
//
// Implements ObservableObject so that ViewModels (e.g., DefaultSettingsViewModel,
// UpperCabinetViewModel) can bind directly to its properties and react to changes via
// PropertyChanged — for example, refreshing dimension format displays when
// DefaultDimensionFormat changes.
//
// Covers the following categories of defaults:
//   - Dimension format (fraction vs decimal)
//   - Species and edge banding species for base, upper, filler, and panel cabinets
//   - Toe kick dimensions and visibility
//   - Shelf counts, shelf depth style, and shelf hole drilling
//   - Door and drawer counts, species, grain direction, reveal/gap sizes, and include flags
//   - Drawer front heights and equalization behavior
//   - Door and drawer slide hole drilling
//   - Base and upper cabinet reveal values and gap width
//   - UI theme, UI scale, and main window size/position (persisted across sessions)
//   - Company/contact info pre-filled on new job customer info forms
//   - One-time popup dismissal tracking (HasSeenPopup stores last-seen version string)
//   - Last file dialog directory (restored on next open/save dialog)
//
// LoadAsync(): deserializes the JSON file and copies all public properties onto this
//   instance using reflection, so new properties added to the class automatically
//   participate without any manual mapping code.
//
// SaveAsync(): serializes this instance to JSON using a temp-file + atomic Move pattern
//   to prevent corruption if the app is closed mid-write. Protected by a SemaphoreSlim
//   to prevent concurrent writes.
//
// GetFileDialogDirectory() / RememberFileDialogDirectory(): convenience helpers used by
//   open/save/export dialogs to default to the last-used folder.

public partial class DefaultSettingsService : ObservableObject
{
    private const string SettingsFileName = "default-settings.json";

    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CorlaneCabinetOrderFormV3");

    private static readonly string SettingsFilePath =
        Path.Combine(SettingsDirectory, SettingsFileName);

    //Dimension Format
    [ObservableProperty] public partial string DefaultDimensionFormat { get; set; } = "Fraction";

    // Species
    [ObservableProperty] public partial string DefaultSpecies { get; set; } = "Prefinished Ply";
    [ObservableProperty] public partial string DefaultEBSpecies { get; set; } = "Wood Maple";
    [ObservableProperty] public partial string DefaultFillerSpecies { get; set; } = "Maple Ply";
    [ObservableProperty] public partial string DefaultPanelSpecies { get; set; } = "Maple Ply";
    [ObservableProperty] public partial string DefaultPanelEBSpecies { get; set; } = "Wood Maple";

    //Top
    [ObservableProperty] public partial string DefaultTopType { get; set; } = "Stretcher";

    // Back
    [ObservableProperty] public partial string DefaultBaseBackThickness { get; set; } = "0.75";
    [ObservableProperty] public partial string DefaultUpperBackThickness { get; set; } = "0.75";

    // Panel Thickness
    [ObservableProperty] public partial string DefaultPanelThickness { get; set; } = "0.75";

    //Toekick
    [ObservableProperty] public partial bool DefaultHasTK { get; set; } = true;
    [ObservableProperty] public partial string DefaultTKHeight { get; set; } = "4";
    [ObservableProperty] public partial string DefaultTKDepth { get; set; } = "3.75";

    // Shelves
    [ObservableProperty] public partial int DefaultShelfCount { get; set; } = 1;
    [ObservableProperty] public partial int DefaultUpperShelfCount { get; set; } = 2;

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
    [ObservableProperty] public partial bool DefaultEdgebandDoorsAndDrawers { get; set; } = true;

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
    [ObservableProperty] public partial bool DefaultEqualizeBottomDrwFronts { get; set; } = true;
    [ObservableProperty] public partial bool DefaultEqualizeAllDrwFronts { get; set; } = false;


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

    // Default Theme
    [ObservableProperty] public partial string? DefaultTheme { get; set; }

    // Customer Info
    [ObservableProperty] public partial string? CompanyName { get; set; }
    [ObservableProperty] public partial string? ContactName { get; set; }
    [ObservableProperty] public partial string? PhoneNumber { get; set; }
    [ObservableProperty] public partial string? EMail { get; set; }
    [ObservableProperty] public partial string? Street { get; set; }
    [ObservableProperty] public partial string? City { get; set; }
    [ObservableProperty] public partial string? ZipCode { get; set; }


    // Window size and position
    [ObservableProperty] public partial double? WindowLeft { get; set; }
    [ObservableProperty] public partial double? WindowTop { get; set; }
    [ObservableProperty] public partial double? WindowWidth { get; set; }
    [ObservableProperty] public partial double? WindowHeight { get; set; }
    [ObservableProperty] public partial string? WindowState { get; set; } = "Normal";

    // UI Scale (1.0 = 100%)
    [ObservableProperty] public partial double UIScale { get; set; } = 1.0;


    // One-time popup notices — stores the last version whose popup was dismissed
    [ObservableProperty] public partial string? HasSeenPopup { get; set; }

    // Last directory used in a file dialog (open/save/export)
    [ObservableProperty] public partial string? LastFileDialogDirectory { get; set; }

    /// <summary>
    /// Returns the best initial directory for a file dialog:
    /// <paramref name="preferredPath"/> directory first, then last-used directory, then My Documents.
    /// </summary>
    public string GetFileDialogDirectory(string? preferredPath = null)
    {
        if (!string.IsNullOrEmpty(preferredPath))
        {
            var dir = Path.GetDirectoryName(preferredPath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                return dir;
        }

        if (!string.IsNullOrEmpty(LastFileDialogDirectory) && Directory.Exists(LastFileDialogDirectory))
            return LastFileDialogDirectory;

        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    /// <summary>
    /// Saves the directory of the given file path as the last-used dialog directory.
    /// </summary>
    public void RememberFileDialogDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            LastFileDialogDirectory = dir;
            _ = SaveAsync(); // fire-and-forget persist
        }
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(SettingsFilePath)) return;

        var json = await File.ReadAllTextAsync(SettingsFilePath).ConfigureAwait(false);
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


    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public async Task SaveAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(this);
            var tempPath = SettingsFilePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, SettingsFilePath, overwrite: true);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}


