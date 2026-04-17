using CommunityToolkit.Mvvm.ComponentModel;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class BaseCabinetViewModel : ObservableValidator
{
    internal void MapModelToViewModel(BaseCabinetModel model, string dimFormat)
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

            // Only update visibility/state — do NOT recalculate values.
            // The model's values are authoritative after mapping.
            ApplyStyleVisibility(model.Style);
            RunValidationVisible();
        }
    }

    /// <summary>
    /// Copies all current ViewModel property values into the target model,
    /// converting dimension strings to numeric format.
    /// </summary>
    private void ApplyViewModelToModel(BaseCabinetModel target)
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
        target.TKHeight = ConvertDimension.FractionToDouble(TKHeight).ToString();
        target.Style = Style;
        target.LeftBackWidth = ConvertDimension.FractionToDouble(LeftBackWidth).ToString();
        target.RightBackWidth = ConvertDimension.FractionToDouble(RightBackWidth).ToString();
        target.LeftFrontWidth = ConvertDimension.FractionToDouble(LeftFrontWidth).ToString();
        target.RightFrontWidth = ConvertDimension.FractionToDouble(RightFrontWidth).ToString();
        target.LeftDepth = ConvertDimension.FractionToDouble(LeftDepth).ToString();
        target.RightDepth = ConvertDimension.FractionToDouble(RightDepth).ToString();
        target.HasTK = HasTK;
        target.TKDepth = ConvertDimension.FractionToDouble(TKDepth).ToString();
        target.DoorSpecies = DoorSpecies;
        target.CustomDoorSpecies = CustomDoorSpecies;
        target.BackThickness = ConvertDimension.FractionToDouble(BackThickness).ToString();
        target.TopType = TopType;
        target.ShelfCount = ShelfCount;
        target.ShelfDepth = ShelfDepth;
        target.DrillShelfHoles = DrillShelfHoles;
        target.DoorCount = DoorCount;
        target.DoorGrainDir = DoorGrainDir;
        target.IncDoorsInList = IncDoorsInList;
        target.IncDoors = IncDoors;
        target.DrillHingeHoles = DrillHingeHoles;
        target.DrwFrontGrainDir = DrwFrontGrainDir;
        // Derive vestigial master flags from per-item state (backward compat for saved files)
        target.IncDrwFrontsInList = IncDrwFrontInList1 || IncDrwFrontInList2 || IncDrwFrontInList3 || IncDrwFrontInList4;
        target.IncDrwFronts = IncDrwFront1 || IncDrwFront2 || IncDrwFront3 || IncDrwFront4;
        target.IncDrwBoxesInList = IncDrwBoxInListOpening1 || IncDrwBoxInListOpening2 || IncDrwBoxInListOpening3 || IncDrwBoxInListOpening4;
        target.IncDrwBoxes = IncDrwBoxOpening1 || IncDrwBoxOpening2 || IncDrwBoxOpening3 || IncDrwBoxOpening4;
        target.DrillSlideHoles = DrillSlideHolesOpening1 || DrillSlideHolesOpening2 || DrillSlideHolesOpening3 || DrillSlideHolesOpening4;
        target.DrwCount = DrwCount;
        target.DrwStyle = DrwStyle;
        target.OpeningHeight1 = ConvertDimension.FractionToDouble(OpeningHeight1).ToString();
        target.OpeningHeight2 = ConvertDimension.FractionToDouble(OpeningHeight2).ToString();
        target.OpeningHeight3 = ConvertDimension.FractionToDouble(OpeningHeight3).ToString();
        target.OpeningHeight4 = ConvertDimension.FractionToDouble(OpeningHeight4).ToString();
        target.IncDrwBoxOpening1 = IncDrwBoxOpening1;
        target.IncDrwBoxOpening2 = IncDrwBoxOpening2;
        target.IncDrwBoxOpening3 = IncDrwBoxOpening3;
        target.IncDrwBoxOpening4 = IncDrwBoxOpening4;
        target.DrillSlideHolesOpening1 = DrillSlideHolesOpening1;
        target.DrillSlideHolesOpening2 = DrillSlideHolesOpening2;
        target.DrillSlideHolesOpening3 = DrillSlideHolesOpening3;
        target.DrillSlideHolesOpening4 = DrillSlideHolesOpening4;
        target.IncDrwBoxInListOpening1 = IncDrwBoxInListOpening1;
        target.IncDrwBoxInListOpening2 = IncDrwBoxInListOpening2;
        target.IncDrwBoxInListOpening3 = IncDrwBoxInListOpening3;
        target.IncDrwBoxInListOpening4 = IncDrwBoxInListOpening4;
        target.DrwFrontHeight1 = ConvertDimension.FractionToDouble(DrwFrontHeight1).ToString();
        target.DrwFrontHeight2 = ConvertDimension.FractionToDouble(DrwFrontHeight2).ToString();
        target.DrwFrontHeight3 = ConvertDimension.FractionToDouble(DrwFrontHeight3).ToString();
        target.DrwFrontHeight4 = ConvertDimension.FractionToDouble(DrwFrontHeight4).ToString();
        target.IncDrwFront1 = IncDrwFront1;
        target.IncDrwFront2 = IncDrwFront2;
        target.IncDrwFront3 = IncDrwFront3;
        target.IncDrwFront4 = IncDrwFront4;
        target.IncDrwFrontInList1 = IncDrwFrontInList1;
        target.IncDrwFrontInList2 = IncDrwFrontInList2;
        target.IncDrwFrontInList3 = IncDrwFrontInList3;
        target.IncDrwFrontInList4 = IncDrwFrontInList4;
        target.LeftReveal = ConvertDimension.FractionToDouble(LeftReveal).ToString();
        target.RightReveal = ConvertDimension.FractionToDouble(RightReveal).ToString();
        target.TopReveal = ConvertDimension.FractionToDouble(TopReveal).ToString();
        target.BottomReveal = ConvertDimension.FractionToDouble(BottomReveal).ToString();
        target.GapWidth = ConvertDimension.FractionToDouble(GapWidth).ToString();
        target.IncRollouts = IncRollouts;
        target.IncRolloutsInList = IncRolloutsInList;
        target.RolloutCount = RolloutCount;
        target.RolloutStyle = RolloutStyle;
        target.DrillSlideHolesForRollouts = DrillSlideHolesForRollouts;
        target.SinkCabinet = SinkCabinet;
        target.TrashDrawer = TrashDrawer;
        target.IncTrashDrwBox = IncTrashDrwBox;
        target.EqualizeAllDrwFronts = EqualizeAllDrwFronts;
        target.EqualizeBottomDrwFronts = EqualizeBottomDrwFronts;
    }

    private void LoadSelectedIfMine() // Populate fields on Cab List click if selected cabinet is of this type
    {
        string dimFormat = _defaults?.DefaultDimensionFormat ?? "Decimal";

        if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel baseCab)
        {
            // Map model -> VM with proper formatting for dimension properties
            MapModelToViewModel(baseCab, dimFormat);

            // Kill any stale debounce timer that fired during mapping
            // and resync the edit buffer to the final correct value.
            _drwFrontHeight1DebounceTimer.Stop();
            _isEditingDrwFrontHeight1 = false;
            DrwFrontHeight1Edit = DrwFrontHeight1; // restarts timer via OnDrwFrontHeight1EditChanged
            _drwFrontHeight1DebounceTimer.Stop();  // kill it again immediately
            _isEditingDrwFrontHeight1 = false;

            // Recalculate derived angle-front fields that were skipped
            // during mapping (change handlers bail out while _isMapping is true).
            RecalculateFrontWidth();
            RecalculateBackWidths90();

            UpdatePreview();
        }
    }

    // Helper: property name set that should be treated as a "dimension" (string -> numeric -> formatted string)
    private static readonly HashSet<string> s_dimensionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Width","Height","Depth","TKHeight","TKDepth",
        "LeftBackWidth","RightBackWidth","LeftFrontWidth","RightFrontWidth",
        "LeftDepth","RightDepth","BackThickness", "FrontWidth",
        "OpeningHeight1","OpeningHeight2","OpeningHeight3","OpeningHeight4",
        "DrwFrontHeight1","DrwFrontHeight2","DrwFrontHeight3","DrwFrontHeight4",
        "LeftReveal","RightReveal","TopReveal","BottomReveal","GapWidth"
    };
}
