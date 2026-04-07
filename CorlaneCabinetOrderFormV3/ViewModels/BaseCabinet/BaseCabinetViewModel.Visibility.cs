using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class BaseCabinetViewModel : ObservableValidator
    {
        /// <summary>
        /// Single source of truth for all visibility / enabled / disabled state.
        /// Derives everything from current VM property values.
        /// Does NOT modify any dimension values or trigger recalculation.
        /// </summary>
        private void ApplyStyleVisibility(string style)
        {
            // ── Style-dependent ──
            StdOrDrwBaseVisibility = (style == Style1 || style == Style2);
            Corner90Visibility = (style == Style3);
            Corner45Visibility = (style == Style4);
            GroupShelvesVisibility = (style == Style1 || style == Style3 || style == Style4);
            ComboShelfDepthEnabled = (style == Style1 || style == Style3);
            GroupDrawersVisibility = (style == Style1 || style == Style2);
            GroupCabinetTopTypeVisibility = (style == Style1 || style == Style2);
            GroupDrawerFrontHeightsVisibility = (style == Style1 || style == Style2);
            GroupDoorsVisibility = (style == Style1 || style == Style3 || style == Style4);
            BackThicknessVisible = (style == Style1 || style == Style2);
            SinkCabinetEnabled = (style == Style1 || style == Style3 || style == Style4);

            if (style == Style2)
                ListDrwCount = [1, 2, 3, 4];
            else if (style == Style1)
                ListDrwCount = [0, 1];

            // ── DrwCount-dependent ──
            DrawersStackPanelVisible = DrwCount > 0;
            DrwFront1Visible = DrwCount >= 1;
            DrwFront2Visible = DrwCount >= 2;
            DrwFront3Visible = DrwCount >= 3;
            DrwFront4Visible = DrwCount == 4;
            Opening1Visible = DrwCount >= 1;
            Opening2Visible = DrwCount >= 2;
            Opening3Visible = DrwCount >= 3;
            Opening4Visible = DrwCount == 4;

            // ── RolloutCount-dependent ──
            IncRolloutsVisible = RolloutCount > 0;
            IncRolloutsInListVisible = RolloutCount > 0;
            RolloutStyleVisible = RolloutCount > 0;
            DrillSlideHolesForRolloutsVisible = RolloutCount > 0;

            // ── DoorCount-dependent ──
            DoorGrainDirVisible = DoorCount > 0;
            IncDoorsInListVisible = DoorCount > 0;
            DrillHingeHolesVisible = DoorCount > 0;
            SupplySlabDoorsVisible = DoorCount > 0;

            // ── TrashDrawer / Rollouts mutual-exclusion ──
            // Use RolloutCount > 0 (not IncRollouts) because IncRollouts can be
            // true from defaults even when no rollouts are structurally present.
            GroupRolloutsVisible = (style == Style1) && !TrashDrawer && ConvertDimension.FractionToDouble(Depth) >= 10.625;
            TrashDrawerEnabled = (style == Style1) && RolloutCount == 0;
            IncRolloutsEnabled = !TrashDrawer && ConvertDimension.FractionToDouble(Depth) >= 10.625;
            IncRolloutsInListEnabled = !TrashDrawer && ConvertDimension.FractionToDouble(Depth) >= 10.625;

            // ── SinkCabinet-dependent ──
            IncDrwBoxesVisible = !SinkCabinet;
            IncDrwBoxesInListVisible = !SinkCabinet;
            DrillSlideHolesVisible = !SinkCabinet;
            ListDrawerStyleVisible = !SinkCabinet;

            // ── Equalization-dependent ──
            if (EqualizeAllDrwFronts)
            {
                DrwFront1Disabled = true;
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
                Opening1Disabled = true;
                Opening2Disabled = true;
                Opening3Disabled = true;
            }
            else if (EqualizeBottomDrwFronts)
            {
                DrwFront1Disabled = false;
                DrwFront2Disabled = true;
                DrwFront3Disabled = true;
                Opening1Disabled = false;
                Opening2Disabled = true;
                Opening3Disabled = true;
            }
            else
            {
                DrwFront1Disabled = false;
                DrwFront2Disabled = false;
                DrwFront3Disabled = false;
                Opening1Disabled = false;
                Opening2Disabled = false;
                Opening3Disabled = false;
            }

            // ── Species Custom-dependent ──
            CustomCabSpeciesEnabled = (Species == "Custom");
            CustomEBSpeciesEnabled = (EBSpecies == "Custom");
            CustomDoorSpeciesEnabled = (DoorSpecies == "Custom");
        }
    }
}