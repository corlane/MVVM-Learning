using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.Rendering;

// CabinetBuildHelpers.cs
// ─────────────────────────────────────────────────────────────────────────────
// Shared static helpers used by both base and upper cabinet builders during
// material/edge-total accumulation and BOM construction.
//
// Responsibilities:
//   - GetMatchingEdgebandingSpecies: Maps cabinet species/material names (e.g.,
//     "Maple Ply", "White Melamine", "Prefinished Ply") to their corresponding
//     edgebanding product names (e.g., "Wood Maple", "PVC White"). Used when
//     building material totals so the correct EB species is recorded per part.
//
//   - GetDoorEdgebandingSpecies: Thin wrapper over GetMatchingEdgebandingSpecies
//     specifically for door species resolution.
//
//   - ResolveDoorSpeciesForTotals: Resolves the effective door species string,
//     substituting the user-entered custom species name when the selected species
//     is "Custom" — preventing the literal word "Custom" from appearing in totals.
//
//   - AddFrontPartRow (overloads for Base/Upper): Appends a FrontPartRow entry
//     (door or drawer front) to a cabinet model's FrontParts list. Cabinet
//     number and name are left as defaults here and assigned later by the
//     list view-model.
//
//   - AddDrawerBoxRow: Appends a DrawerBoxRow entry to a base cabinet model's
//     DrawerBoxes list for BOM/cut-list output.
//
// Note: BOM and material total calculations must always include all cabinet
// parts regardless of preview hide flags — hide toggles are visualization-only.
// ─────────────────────────────────────────────────────────────────────────────

internal static class CabinetBuildHelpers
{
    internal static string GetMatchingEdgebandingSpecies(string? matchingEBSpecies) // Helper to map common species/material names to edgebanding names
    {
        return matchingEBSpecies switch
        {
            null or "" => "None",

            // Pass "Custom" through so the caller's CustomEBSpecies is used
            string s when s.Equals("Custom", StringComparison.OrdinalIgnoreCase) => "Custom",

            // Match common species/material names -> edgebanding names
            string s when s.Contains("Alder", StringComparison.OrdinalIgnoreCase) => "Wood Alder",
            string s when s.Contains("Cherry", StringComparison.OrdinalIgnoreCase) => "Wood Cherry",
            string s when s.Contains("Hickory", StringComparison.OrdinalIgnoreCase) => "Wood Hickory",
            string s when s.Contains("Mahogany", StringComparison.OrdinalIgnoreCase) => "Wood Mahogany",
            string s when s.Contains("Maple", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Maply Ply", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("MDF", StringComparison.OrdinalIgnoreCase) => "Wood Maple",
            string s when s.Contains("Prefinished Ply", StringComparison.OrdinalIgnoreCase) => "PVC Hardrock Maple",
            string s when s.Contains("PFP 1/4", StringComparison.OrdinalIgnoreCase) => "None",
            string s when s.Contains("Red Oak", StringComparison.OrdinalIgnoreCase) => "Wood Red Oak",
            string s when s.Contains("Walnut", StringComparison.OrdinalIgnoreCase) => "Wood Walnut",
            string s when s.Contains("White Oak", StringComparison.OrdinalIgnoreCase) => "Wood White Oak",
            string s when s.Contains("White Melamine", StringComparison.OrdinalIgnoreCase) => "PVC White",
            string s when s.Contains("Black Melamine", StringComparison.OrdinalIgnoreCase) => "PVC Black",

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
            Qty: cab.Qty,
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
            Qty: cab.Qty,
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
            Qty: cab.Qty,
            Type: type,
            Height: height,
            Width: width,
            Length: length));
    }
}