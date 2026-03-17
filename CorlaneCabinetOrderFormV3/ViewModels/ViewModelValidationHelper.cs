using System.Windows;

namespace CorlaneCabinetOrderFormV3.ViewModels;

/// <summary>
/// Shared validation checks used by all cabinet ViewModel Add/Update commands.
/// </summary>
internal static class ViewModelValidationHelper
{
    /// <summary>
    /// Returns true if all required custom species fields are filled in.
    /// Shows a MessageBox and returns false if any "Custom" species is missing its name.
    /// </summary>
    internal static bool ValidateCustomSpecies(
        string species, string? customSpecies,
        string ebSpecies, string? customEBSpecies,
        string? doorSpecies = null, string? customDoorSpecies = null)
    {
        if (species == "Custom" && string.IsNullOrWhiteSpace(customSpecies))
        {
            MessageBox.Show("Please enter a custom species name.", "Custom Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (doorSpecies is not null && doorSpecies == "Custom" && string.IsNullOrWhiteSpace(customDoorSpecies))
        {
            MessageBox.Show("Please enter a custom door species name.", "Custom Door Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (ebSpecies == "Custom" && string.IsNullOrWhiteSpace(customEBSpecies))
        {
            MessageBox.Show("Please enter a custom edgebanding species name.", "Custom Edge Band Species", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the name is unique (or blank). Returns false and notifies if a duplicate exists.
    /// </summary>
    internal static bool ValidateUniqueName(
        string? name,
        Models.CabinetModel selected,
        Services.ICabinetService? cabinetService,
        MainWindowViewModel? mainVm)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;

        var normalized = name.Trim();

        bool dup = cabinetService?.Cabinets.Any(c =>
            !ReferenceEquals(c, selected) &&
            !string.IsNullOrWhiteSpace(c.Name) &&
            string.Equals(c.Name.Trim(), normalized, StringComparison.OrdinalIgnoreCase)) == true;

        if (dup)
        {
            mainVm?.Notify("Duplicate cabinet names are not allowed.", System.Windows.Media.Brushes.Red, 3000);
            return false;
        }

        return true;
    }
}