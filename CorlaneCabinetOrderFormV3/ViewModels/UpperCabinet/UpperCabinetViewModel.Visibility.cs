using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    private void ApplyStyleVisibility(string style)
    {
        BackThicknessVisible = (style == Style1 || style == Style2);

        // DoorCount-dependent visibility
        DoorGrainDirVisible = DoorCount > 0;
        IncDoorsInListVisible = DoorCount > 0;
        DrillHingeHolesVisible = DoorCount > 0;
        SupplySlabDoorsVisible = DoorCount > 0;
    }
    
}
