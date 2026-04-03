using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class BaseCabinetViewModel : ObservableValidator
    {
        /// <summary>
        /// Sets visibility/state/list properties that depend on the cabinet style.
        /// Does NOT modify any dimension values or trigger recalculation.
        /// </summary>
        private void ApplyStyleVisibility(string style)
        {
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
            GroupRolloutsVisible = (style == Style1 && !TrashDrawer);
            TrashDrawerEnabled = (style == Style1);
            SinkCabinetEnabled = (style == Style1 || style == Style3 || style == Style4);
            if (style == Style2)
                ListDrwCount = [1, 2, 3, 4];
            else if (style == Style1)
                ListDrwCount = [0, 1];

            // DrwCount-dependent visibility
            DrawersStackPanelVisible = DrwCount > 0;
            DrwFront1Visible = DrwCount >= 1;
            DrwFront2Visible = DrwCount >= 2;
            DrwFront3Visible = DrwCount >= 3;
            DrwFront4Visible = DrwCount == 4;
            Opening1Visible = DrwCount >= 1;
            Opening2Visible = DrwCount >= 2;
            Opening3Visible = DrwCount >= 3;
            Opening4Visible = DrwCount == 4;

            // RolloutCount-dependent visibility
            IncRolloutsVisible = RolloutCount > 0;
            IncRolloutsInListVisible = RolloutCount > 0;
            RolloutStyleVisible = RolloutCount > 0;
            DrillSlideHolesForRolloutsVisible = RolloutCount > 0;

            // DoorCount-dependent visibility
            DoorGrainDirVisible = DoorCount > 0;
            IncDoorsInListVisible = DoorCount > 0;
            DrillHingeHolesVisible = DoorCount > 0;
            SupplySlabDoorsVisible = DoorCount > 0;

            // Trash Drawer dependent visibility
            GroupRolloutsVisible = (style == Style1);
        }


    }
}
