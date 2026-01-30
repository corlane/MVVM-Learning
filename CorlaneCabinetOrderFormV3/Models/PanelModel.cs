using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.Models;

public partial class PanelModel : CabinetModel
{
    // Type-specific properties for PanelModel

    [ObservableProperty] public partial bool PanelEBTop { get; set; }
    [ObservableProperty] public partial bool PanelEBBottom { get; set; }
    [ObservableProperty] public partial bool PanelEBLeft { get; set; }
    [ObservableProperty] public partial bool PanelEBRight { get; set; }


    private void BumpGeometry() => BumpGeometryVersion();

    partial void OnPanelEBTopChanged(bool value) => BumpGeometry();
    partial void OnPanelEBBottomChanged(bool value) => BumpGeometry();
    partial void OnPanelEBLeftChanged(bool value) => BumpGeometry();
    partial void OnPanelEBRightChanged(bool value) => BumpGeometry();
}
