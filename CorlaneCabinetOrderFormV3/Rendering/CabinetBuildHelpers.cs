using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class CabinetBuildHelpers
{
    internal static string GetMatchingEdgebandingSpecies(string? fillerSpecies) // Helper to map common species/material names to edgebanding names
    {
        return fillerSpecies switch
        {
            null or "" => "None",

            // Match common species/material names -> edgebanding names
            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Maply Ply", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Melamine", StringComparison.OrdinalIgnoreCase) => "Melamine",
            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "PVC Hardrock Maple",
            string s when s.Contains("PFP 1/4", StringComparison.OrdinalIgnoreCase) => "None",
            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood White Oak",

            _ => "None"
        };
    }

    internal static string GetDoorEdgebandingSpecies(string? doorSpecies)
        => GetMatchingEdgebandingSpecies(doorSpecies);

    internal static string ResolveDoorSpeciesForTotals(string? doorSpecies, string? customDoorSpecies)
    {
        var s = (doorSpecies ?? "").Trim();
        if (!string.Equals(s, "Custom", StringComparison.OrdinalIgnoreCase))
        {
            return s;
        }

        var custom = (customDoorSpecies ?? "").Trim();
        return string.IsNullOrWhiteSpace(custom) ? "Custom" : custom;
    }

    internal static void AddFrontPartRow(
        BaseCabinetModel cab,
        string type,
        double height,
        double width,
        string? species,
        string? grainDirection)
    {
        // CabinetNumber/CabinetName are assigned later by the list view-model.
        cab.FrontParts.Add(new FrontPartRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Species: species ?? "",
            GrainDirection: grainDirection ?? ""));
    }

    internal static void AddFrontPartRow(
        UpperCabinetModel cab,
        string type,
        double height,
        double width,
        string? species,
        string? grainDirection)
    {
        cab.FrontParts.Add(new FrontPartRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Species: species ?? "",
            GrainDirection: grainDirection ?? ""));
    }

    internal static void AddDrawerBoxRow(
        BaseCabinetModel cab,
        string type,
        double height,
        double width,
        double length)
    {
        cab.DrawerBoxes.Add(new DrawerBoxRow(
            CabinetNumber: 0,
            CabinetName: "",
            Type: type,
            Height: height,
            Width: width,
            Length: length));
    }
}