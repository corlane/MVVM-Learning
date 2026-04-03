using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    /// <summary>
    /// Single source of truth for all visibility / enabled / disabled state.
    /// Derives everything from current VM property values.
    /// Does NOT modify any dimension values or trigger recalculation.
    /// </summary>
    private void ApplyStyleVisibility(string style)
    {
        // ── Style-dependent ──
        StandardDimsVisibility = (style == Style1);
        Corner90DimsVisibility = (style == Style2);
        Corner45DimsVisibility = (style == Style3);
        BackThicknessVisible = (style == Style1); // corners force 3/4" back

        // ── DoorCount-dependent ──
        DoorGrainDirVisible = DoorCount > 0;
        IncDoorsInListVisible = DoorCount > 0;
        DrillHingeHolesVisible = DoorCount > 0;
        SupplySlabDoorsVisible = DoorCount > 0;

        // ── Species Custom-dependent ──
        CustomCabSpeciesEnabled = (Species == "Custom");
        CustomEBSpeciesEnabled = (EBSpecies == "Custom");
        CustomDoorSpeciesEnabled = (DoorSpecies == "Custom");
    }
}