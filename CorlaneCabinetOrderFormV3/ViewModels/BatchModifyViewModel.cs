using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BatchModifyViewModel : ObservableObject
{
    private readonly List<CabinetModel> _selectedCabinets;

    // ── Enable flags ─────────────────────────────────────────────
    [ObservableProperty] public partial bool ApplySpecies { get; set; }
    [ObservableProperty] public partial bool ApplyEBSpecies { get; set; }
    [ObservableProperty] public partial bool ApplyDoorSpecies { get; set; }
    [ObservableProperty] public partial bool ApplyReveals { get; set; }
    [ObservableProperty] public partial bool ApplyGapWidth { get; set; }
    [ObservableProperty] public partial bool ApplySupplyDoors { get; set; }
    [ObservableProperty] public partial bool ApplySupplyDrawerFronts { get; set; }

    // ── Values ───────────────────────────────────────────────────
    [ObservableProperty] public partial string Species { get; set; } = "";
    [ObservableProperty] public partial string CustomSpecies { get; set; } = "";
    [ObservableProperty] public partial string EBSpecies { get; set; } = "";
    [ObservableProperty] public partial string CustomEBSpecies { get; set; } = "";
    [ObservableProperty] public partial string DoorSpecies { get; set; } = "";
    [ObservableProperty] public partial string CustomDoorSpecies { get; set; } = "";
    [ObservableProperty] public partial string LeftReveal { get; set; } = "";
    [ObservableProperty] public partial string RightReveal { get; set; } = "";
    [ObservableProperty] public partial string TopReveal { get; set; } = "";
    [ObservableProperty] public partial string BottomReveal { get; set; } = "";
    [ObservableProperty] public partial string GapWidth { get; set; } = "";
    [ObservableProperty] public partial bool IncDoors { get; set; } = true;
    [ObservableProperty] public partial bool IncDoorsInList { get; set; } = true;
    [ObservableProperty] public partial bool IncDrwFronts { get; set; } = true;
    [ObservableProperty] public partial bool IncDrwFrontsInList { get; set; } = true;

    // ── Computed visibility helpers ──────────────────────────────
    public bool IsSpeciesCustom => Species == "Custom";
    public bool IsEBSpeciesCustom => EBSpecies == "Custom";
    public bool IsDoorSpeciesCustom => DoorSpecies == "Custom";

    partial void OnSpeciesChanged(string value) => OnPropertyChanged(nameof(IsSpeciesCustom));
    partial void OnEBSpeciesChanged(string value) => OnPropertyChanged(nameof(IsEBSpeciesCustom));
    partial void OnDoorSpeciesChanged(string value) => OnPropertyChanged(nameof(IsDoorSpeciesCustom));

    // ── Combo item sources ───────────────────────────────────────
    public ObservableCollection<string> CabinetSpeciesList { get; }
    public ObservableCollection<string> EBSpeciesList { get; }

    // ── Info ─────────────────────────────────────────────────────
    public int SelectedCount { get; }
    public bool HasBaseOrUpper { get; }
    public bool HasBase { get; }
    /// <summary>True when at least one selected cabinet supports user-specified edgebanding (i.e. is not a filler).</summary>
    public bool HasUserEB { get; }

    public BatchModifyViewModel(List<CabinetModel> selectedCabinets)
    {
        _selectedCabinets = selectedCabinets;
        SelectedCount = selectedCabinets.Count;
        HasBaseOrUpper = selectedCabinets.Any(c => c is BaseCabinetModel or UpperCabinetModel);
        HasBase = selectedCabinets.Any(c => c is BaseCabinetModel);
        HasUserEB = selectedCabinets.Any(c => c is not FillerModel);

        var materialSvc = App.ServiceProvider.GetRequiredService<IMaterialLookupService>();
        CabinetSpeciesList = materialSvc.CabinetSpecies;
        EBSpeciesList = materialSvc.EBSpecies;

        // Pre-fill values from the first selected cabinet for convenience
        var first = selectedCabinets[0];
        Species = first.Species ?? "";
        CustomSpecies = first.CustomSpecies ?? "";
        EBSpecies = first.EBSpecies ?? "";
        CustomEBSpecies = first.CustomEBSpecies ?? "";

        if (first is BaseCabinetModel baseCab)
        {
            DoorSpecies = baseCab.DoorSpecies ?? "";
            CustomDoorSpecies = baseCab.CustomDoorSpecies ?? "";
            LeftReveal = baseCab.LeftReveal ?? "";
            RightReveal = baseCab.RightReveal ?? "";
            TopReveal = baseCab.TopReveal ?? "";
            BottomReveal = baseCab.BottomReveal ?? "";
            GapWidth = baseCab.GapWidth ?? "";
            IncDoors = baseCab.IncDoors;
            IncDoorsInList = baseCab.IncDoorsInList;
            IncDrwFronts = baseCab.IncDrwFronts;
            IncDrwFrontsInList = baseCab.IncDrwFrontsInList;
        }
        else if (first is UpperCabinetModel upperCab)
        {
            DoorSpecies = upperCab.DoorSpecies ?? "";
            CustomDoorSpecies = upperCab.CustomDoorSpecies ?? "";
            LeftReveal = upperCab.LeftReveal ?? "";
            RightReveal = upperCab.RightReveal ?? "";
            TopReveal = upperCab.TopReveal ?? "";
            BottomReveal = upperCab.BottomReveal ?? "";
            GapWidth = upperCab.GapWidth ?? "";
            IncDoors = upperCab.IncDoors;
            IncDoorsInList = upperCab.IncDoorsInList;
        }
    }

    /// <summary>Applies the enabled fields to all selected cabinets.</summary>
    public void ApplyToSelected()
    {
        foreach (var cab in _selectedCabinets)
        {
            if (ApplySpecies)
            {
                cab.Species = Species;
                cab.CustomSpecies = Species == "Custom" ? CustomSpecies : "";
            }

            if (ApplyEBSpecies && cab is not FillerModel)
            {
                cab.EBSpecies = EBSpecies;
                cab.CustomEBSpecies = EBSpecies == "Custom" ? CustomEBSpecies : "";
            }

            if (cab is BaseCabinetModel baseCab)
            {
                if (ApplyDoorSpecies)
                {
                    baseCab.DoorSpecies = DoorSpecies;
                    baseCab.CustomDoorSpecies = DoorSpecies == "Custom" ? CustomDoorSpecies : "";
                }

                if (ApplyReveals)
                {
                    baseCab.LeftReveal = LeftReveal;
                    baseCab.RightReveal = RightReveal;
                    baseCab.TopReveal = TopReveal;
                    baseCab.BottomReveal = BottomReveal;
                }

                if (ApplyGapWidth)
                    baseCab.GapWidth = GapWidth;

                if (ApplySupplyDoors)
                {
                    baseCab.IncDoors = IncDoors;
                    baseCab.IncDoorsInList = IncDoorsInList;
                }

                if (ApplySupplyDrawerFronts)
                {
                    baseCab.IncDrwFronts = IncDrwFronts;
                    baseCab.IncDrwFront1 = IncDrwFronts;
                    baseCab.IncDrwFront2 = IncDrwFronts;
                    baseCab.IncDrwFront3 = IncDrwFronts;
                    baseCab.IncDrwFront4 = IncDrwFronts;

                    baseCab.IncDrwFrontsInList = IncDrwFrontsInList;
                    baseCab.IncDrwFrontInList1 = IncDrwFrontsInList;
                    baseCab.IncDrwFrontInList2 = IncDrwFrontsInList;
                    baseCab.IncDrwFrontInList3 = IncDrwFrontsInList;
                    baseCab.IncDrwFrontInList4 = IncDrwFrontsInList;
                }

                // Recalculate drawer layout when reveals or gap changed
                if (ApplyReveals || ApplyGapWidth)
                    RecalculateDrawerLayout(baseCab);
            }
            else if (cab is UpperCabinetModel upperCab)
            {
                if (ApplyDoorSpecies)
                {
                    upperCab.DoorSpecies = DoorSpecies;
                    upperCab.CustomDoorSpecies = DoorSpecies == "Custom" ? CustomDoorSpecies : "";
                }

                if (ApplyReveals)
                {
                    upperCab.LeftReveal = LeftReveal;
                    upperCab.RightReveal = RightReveal;
                    upperCab.TopReveal = TopReveal;
                    upperCab.BottomReveal = BottomReveal;
                }

                if (ApplyGapWidth)
                    upperCab.GapWidth = GapWidth;

                if (ApplySupplyDoors)
                {
                    upperCab.IncDoors = IncDoors;
                    upperCab.IncDoorsInList = IncDoorsInList;
                }
            }
        }
    }

    /// <summary>
    /// Mirrors BaseCabinetViewModel.RecalculateDrawerLayout — recomputes
    /// opening heights and drawer front heights so they stay consistent
    /// with the newly applied reveals/gap values.
    /// </summary>
    private static void RecalculateDrawerLayout(BaseCabinetModel b)
    {
        if (b.DrwCount <= 0) return;

        var input = new CabinetLayoutCalculator.LayoutInputs(
            b.Style,
            b.DrwCount,
            ConvertDimension.FractionToDouble(b.Height),
            ConvertDimension.FractionToDouble(b.TKHeight),
            b.HasTK,
            ConvertDimension.FractionToDouble(b.TopReveal),
            ConvertDimension.FractionToDouble(b.BottomReveal),
            ConvertDimension.FractionToDouble(b.GapWidth),
            ConvertDimension.FractionToDouble(b.OpeningHeight1),
            ConvertDimension.FractionToDouble(b.OpeningHeight2),
            ConvertDimension.FractionToDouble(b.OpeningHeight3),
            ConvertDimension.FractionToDouble(b.OpeningHeight4),
            ConvertDimension.FractionToDouble(b.DrwFrontHeight1),
            ConvertDimension.FractionToDouble(b.DrwFrontHeight2),
            ConvertDimension.FractionToDouble(b.DrwFrontHeight3),
            ConvertDimension.FractionToDouble(b.DrwFrontHeight4));

        if (b.EqualizeAllDrwFronts || b.EqualizeBottomDrwFronts)
        {
            double tkH = b.HasTK ? ConvertDimension.FractionToDouble(b.TKHeight) : 0;
            double height = ConvertDimension.FractionToDouble(b.Height) - tkH;
            double topRev = ConvertDimension.FractionToDouble(b.TopReveal);
            double botRev = ConvertDimension.FractionToDouble(b.BottomReveal);
            double gap = ConvertDimension.FractionToDouble(b.GapWidth);

            if (b.EqualizeAllDrwFronts)
            {
                double each = CabinetLayoutCalculator.EqualizeAll(height, topRev, botRev, gap, b.DrwCount);
                b.DrwFrontHeight1 = each.ToString();
                b.DrwFrontHeight2 = each.ToString();
                b.DrwFrontHeight3 = each.ToString();
                if (b.DrwCount >= 4) b.DrwFrontHeight4 = each.ToString();
            }
            else // EqualizeBottomDrwFronts
            {
                double top = ConvertDimension.FractionToDouble(b.DrwFrontHeight1);
                double eachBot = CabinetLayoutCalculator.EqualizeBottom(height, topRev, botRev, gap, b.DrwCount, top);
                if (b.DrwCount >= 2) b.DrwFrontHeight2 = eachBot.ToString();
                if (b.DrwCount >= 3) b.DrwFrontHeight3 = eachBot.ToString();
                if (b.DrwCount >= 4) b.DrwFrontHeight4 = eachBot.ToString();
            }

            // Rebuild input with updated drawer front heights for the final pass
            input = input with
            {
                DrwFront1 = ConvertDimension.FractionToDouble(b.DrwFrontHeight1),
                DrwFront2 = ConvertDimension.FractionToDouble(b.DrwFrontHeight2),
                DrwFront3 = ConvertDimension.FractionToDouble(b.DrwFrontHeight3),
                DrwFront4 = ConvertDimension.FractionToDouble(b.DrwFrontHeight4)
            };
        }

        // Compute from openings first (recalculates drawer fronts), then from
        // drawer fronts (recalculates openings) — mirrors RecalculateDrawerLayout.
        var r1 = CabinetLayoutCalculator.ComputeFromOpenings(input);
        b.OpeningHeight1 = r1.Opening1.ToString();
        b.OpeningHeight2 = r1.Opening2.ToString();
        b.OpeningHeight3 = r1.Opening3.ToString();
        b.OpeningHeight4 = r1.Opening4.ToString();
        b.DrwFrontHeight1 = r1.DrwFront1.ToString();
        b.DrwFrontHeight2 = r1.DrwFront2.ToString();
        b.DrwFrontHeight3 = r1.DrwFront3.ToString();
        b.DrwFrontHeight4 = r1.DrwFront4.ToString();

        var r2 = CabinetLayoutCalculator.ComputeFromDrawerFronts(input with
        {
            Opening1 = r1.Opening1,
            Opening2 = r1.Opening2,
            Opening3 = r1.Opening3,
            Opening4 = r1.Opening4,
            DrwFront1 = r1.DrwFront1,
            DrwFront2 = r1.DrwFront2,
            DrwFront3 = r1.DrwFront3,
            DrwFront4 = r1.DrwFront4
        });
        b.OpeningHeight1 = r2.Opening1.ToString();
        b.OpeningHeight2 = r2.Opening2.ToString();
        b.OpeningHeight3 = r2.Opening3.ToString();
        b.OpeningHeight4 = r2.Opening4.ToString();
        b.DrwFrontHeight1 = r2.DrwFront1.ToString();
        b.DrwFrontHeight2 = r2.DrwFront2.ToString();
        b.DrwFrontHeight3 = r2.DrwFront3.ToString();
        b.DrwFrontHeight4 = r2.DrwFront4.ToString();
    }
}