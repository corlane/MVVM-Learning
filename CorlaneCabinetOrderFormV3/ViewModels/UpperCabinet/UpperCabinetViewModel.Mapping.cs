using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    /// <summary>
    /// Copies all current ViewModel property values into the target model,
    /// converting dimension strings to numeric format.
    /// </summary>
    internal void ApplyViewModelToModel(UpperCabinetModel target)
    {
        target.Width = ConvertDimension.FractionToDouble(Width).ToString();
        target.Height = ConvertDimension.FractionToDouble(Height).ToString();
        target.Depth = ConvertDimension.FractionToDouble(Depth).ToString();
        target.Species = Species;
        target.CustomSpecies = CustomSpecies;
        target.EBSpecies = EBSpecies;
        target.CustomEBSpecies = CustomEBSpecies;
        target.Name = Name;
        target.Qty = Qty;
        target.Notes = Notes;
        target.Style = Style;
        target.LeftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth).ToString();
        target.RightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth).ToString();
        target.LeftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidth).ToString();
        target.RightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidth).ToString();
        target.LeftDepth = ConvertDimension.FractionToDouble(LeftDepth).ToString();
        target.RightDepth = ConvertDimension.FractionToDouble(RightDepth).ToString();
        target.DoorSpecies = DoorSpecies;
        target.CustomDoorSpecies = CustomDoorSpecies;
        target.BackThickness = ConvertDimension.FractionToDouble(BackThickness).ToString();
        target.ShelfCount = ShelfCount;
        target.DrillShelfHoles = DrillShelfHoles;
        target.DoorCount = DoorCount;
        target.DoorGrainDir = DoorGrainDir;
        target.IncDoorsInList = IncDoorsInList;
        target.IncDoors = IncDoors;
        target.DrillHingeHoles = DrillHingeHoles;
        target.LeftReveal = ConvertDimension.FractionToDouble(LeftReveal).ToString();
        target.RightReveal = ConvertDimension.FractionToDouble(RightReveal).ToString();
        target.TopReveal = ConvertDimension.FractionToDouble(TopReveal).ToString();
        target.BottomReveal = ConvertDimension.FractionToDouble(BottomReveal).ToString();
        target.GapWidth = ConvertDimension.FractionToDouble(GapWidth).ToString();
        target.EdgebandDoorsAndDrawers = EdgebandDoorsAndDrawers;
    }

    private void MapModelToViewModel(UpperCabinetModel model, string dimFormat)
    {
        if (model is null) return;

        _isMapping = true;
        try
        {
            ViewModelMappingHelper.MapModelToViewModel(this, model, dimFormat, s_dimensionProperties);
        }
        finally
        {
            _isMapping = false;

            ApplyStyleVisibility(Style);
        }
    }

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is UpperCabinetModel upperCab)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(upperCab, dimFormat);

            // Recalculate derived angle-front fields that were skipped
            // during mapping (change handlers bail out while _isMapping is true).
            RecalculateFrontWidth();
            RecalculateBackWidths90();

            // Any additional logic that must run after loading (visibility, resize, preview)
            UpdatePreview();
        }
    }


    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Width","Height","Depth","TKHeight","TKDepth",
        "LeftBackWidth","RightBackWidth","LeftFrontWidth","RightFrontWidth",
        "LeftDepth","RightDepth","BackThickness", "FrontWidth",
        "LeftReveal","RightReveal","TopReveal","BottomReveal","GapWidth"
    };

}
