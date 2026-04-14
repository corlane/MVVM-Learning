using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels
{
    public partial class BaseCabinetViewModel : ObservableValidator
    {
        [RelayCommand]
        private void AddCabinet()
        {
            if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
                return;

            EnforceStyleConstraints();

            string tempTopType = TopType; // User-selected top type, which may be overridden by depth-specific rules below
            EnforceTopTypeForShallowDepth();

            var newCabinet = new BaseCabinetModel();
            ApplyViewModelToModel(newCabinet);

            try
            {
                _cabinetService?.Add(newCabinet);  // Adds to shared list as base type
                _mainVm!.SelectedCabinet = newCabinet;
            }
            catch (InvalidOperationException ex)
            {
                _mainVm?.Notify(ex.Message, Brushes.Red, 3000);
                return;
            }

            Notes = ""; // Clear notes field after adding, since it can contain cabinet-specific info that shouldn't be copied to next cabinet

            TopType = tempTopType; // Restore user's top type choice after forcing a full top for shallow depths

            _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
            _mainVm?.IsModified = true;
        }

        [RelayCommand]
        private void UpdateCabinet()
        {
            if (_mainVm is not null && _mainVm.SelectedCabinet is BaseCabinetModel selected)
            {
                if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
                    return;

                if (!ViewModelValidationHelper.ValidateUniqueName(Name, selected, _cabinetService, _mainVm))
                    return;

                EnforceStyleConstraints();

                string tempTopType = TopType; // User-selected top type, which may be overridden by depth-specific rules below
                EnforceTopTypeForShallowDepth();

                ApplyViewModelToModel(selected);

                _mainVm?.Notify("Cabinet Updated", Brushes.Green);
                _mainVm?.IsModified = true;

                TopType = tempTopType; // Restore user's top type choice after enforcing depth-specific rules
            }

            else
            {
                // No cabinet selected or wrong type
                _mainVm?.Notify("No cabinet selected, or incorrect cabinet tab selected. Nothing updated.", Brushes.Red, 3000);
                return;
            }

            // Optional: clear selection after update
            _mainVm!.SelectedCabinet = null;

            Notes = ""; // Clear notes field after adding, since it can contain cabinet-specific info that shouldn't be copied to next cabinet

        }

        [RelayCommand]
        private void LoadDefaults()
        {
            if (_defaults is null) return;

            if (Style == Style2)
            {
                // Drawer cabinet selected
                ListDrwCount = [1, 2, 3, 4];
            }
            else if (Style == Style1)
            {
                // Standard or corner cabinet selected
                ListDrwCount = [0, 1];
            }

            // Suppress intermediate resize calls while batch-setting defaults.
            // Many of these properties (HasTK, TKHeight, DrwCount, EqualizeAll/Bottom,
            // DrwFrontHeight*, reveals, GapWidth) fire changed handlers that call
            // ResizeOpeningHeights / ResizeDrwFrontHeights against partially-updated state.
            _isResizing = true;
            try
            {
                Species = _defaults.DefaultSpecies;
                EBSpecies = _defaults.DefaultEBSpecies;
                HasTK = _defaults.DefaultHasTK;
                TKHeight = _defaults.DefaultTKHeight;
                TKDepth = _defaults.DefaultTKDepth;
                DoorCount = _defaults.DefaultDoorCount;
                DoorGrainDir = _defaults.DefaultDoorGrainDir;
                IncDoorsInList = _defaults.DefaultIncDoorsInList;
                IncDoors = _defaults.DefaultIncDoors;
                DrillHingeHoles = _defaults.DefaultDrillHingeHoles;
                DoorSpecies = _defaults.DefaultDoorDrwSpecies;
                if (_defaults.DefaultDimensionFormat == "Decimal") { BackThickness = _defaults.DefaultBaseBackThickness; }
                else { BackThickness = ConvertDimension.DoubleToFraction(Convert.ToDouble(_defaults.DefaultBaseBackThickness)); }
                TopType = _defaults.DefaultTopType;
                ShelfCount = _defaults.DefaultShelfCount;
                ShelfDepth = _defaults.DefaultShelfDepth;
                DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
                DrwFrontGrainDir = _defaults.DefaultDrwGrainDir;
                IncDrwFrontsInList = _defaults.DefaultIncDrwFrontsInList;
                IncDrwFronts = _defaults.DefaultIncDrwFronts;
                if (IncDrwFronts)
                {
                    IncDrwFront1 = true;
                    IncDrwFront2 = true;
                    IncDrwFront3 = true;
                    IncDrwFront4 = true;
                }
                else
                {
                    IncDrwFront1 = false;
                    IncDrwFront2 = false;
                    IncDrwFront3 = false;
                    IncDrwFront4 = false;
                }

                IncDrwBoxesInList = _defaults.DefaultIncDrwBoxesInList;
                IncDrwBoxes = _defaults.DefaultIncDrwBoxes;
                IncRollouts = _defaults.DefaultIncDrwBoxes;
                DrillSlideHoles = _defaults.DefaultDrillSlideHoles;
                if (Style == Style1) { DrwCount = _defaults.DefaultStdDrawerCount; }
                if (Style == Style2) { DrwCount = _defaults.DefaultDrawerStackDrawerCount; }
                DrwStyle = _defaults.DefaultDrwStyle;
                RolloutStyle = _defaults.DefaultDrwStyle;
                EqualizeAllDrwFronts = _defaults.DefaultEqualizeAllDrwFronts;
                EqualizeBottomDrwFronts = _defaults.DefaultEqualizeBottomDrwFronts;

                DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                DrwFrontHeight2 = _defaults.DefaultDrwFrontHeight2;
                DrwFrontHeight3 = _defaults.DefaultDrwFrontHeight3;


                if (_defaults.DefaultEqualizeBottomDrwFronts && Style == Style2)
                {
                    DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                    DrwFrontHeight2 = "7"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                    DrwFrontHeight3 = "7";
                }
                if (_defaults.DefaultEqualizeAllDrwFronts && Style == Style2)
                {
                    DrwFrontHeight1 = "3"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                    DrwFrontHeight2 = "3";
                    DrwFrontHeight3 = "3";
                    DrwFrontHeight4 = "3";
                }

                LeftReveal = _defaults.DefaultBaseLeftReveal;
                RightReveal = _defaults.DefaultBaseRightReveal;
                TopReveal = _defaults.DefaultBaseTopReveal;
                BottomReveal = _defaults.DefaultBaseBottomReveal;
                GapWidth = _defaults.DefaultGapWidth;
                SinkCabinet = false;
                TrashDrawer = false;
            }
            finally
            {
                _isResizing = false;
            }

            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
            DrwFrontHeight1Edit = DrwFrontHeight1;
            ApplyStyleVisibility(Style);
        }

        [RelayCommand]
        private void LoadDefaultDrwSettings()
        {
            if (_defaults is null) return;

            // Suppress intermediate resize calls while batch-setting defaults.
            // Preserve the outer _isResizing state so we don't break a parent
            // batch (e.g. LoadDefaults) that also set _isResizing = true.
            bool wasResizing = _isResizing;
            _isResizing = true;
            try
            {
                EqualizeAllDrwFronts = _defaults.DefaultEqualizeAllDrwFronts;
                EqualizeBottomDrwFronts = _defaults.DefaultEqualizeBottomDrwFronts;

                DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                DrwFrontHeight2 = _defaults.DefaultDrwFrontHeight2;
                DrwFrontHeight3 = _defaults.DefaultDrwFrontHeight3;


                if (_defaults.DefaultEqualizeBottomDrwFronts && Style == Style2)
                {
                    DrwFrontHeight1 = _defaults.DefaultDrwFrontHeight1;
                    DrwFrontHeight2 = "7"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                    DrwFrontHeight3 = "7";
                }
                if (_defaults.DefaultEqualizeAllDrwFronts && Style == Style2)
                {
                    DrwFrontHeight1 = "3"; // lmao magic numbers. Need to pre-seed these so the resize routine works correctly
                    DrwFrontHeight2 = "3";
                    DrwFrontHeight3 = "3";
                    DrwFrontHeight4 = "3";
                }
            }
            finally
            {
                _isResizing = wasResizing;
            }

            ApplyDrawerFrontEqualization();
            ResizeDrwFrontHeights();
        }

        private void EnforceTopTypeForShallowDepth()
        {
            // If the user chose "Stretcher" but the cabinet is very shallow, force "Full".

            double depth = ConvertDimension.FractionToDouble(Depth);

            if (depth > 0 && depth < 10)
            {
                TopType = CabinetOptions.TopType.Full;
            }
        }

        /// <summary>
        /// Applies style-specific constraints before saving to a model
        /// (e.g. drawer cabs have 0 doors, corner cabs force 3/4" back).
        /// </summary>
        private void EnforceStyleConstraints()
        {
            if (Style == Style2)
            {
                DoorCount = 0;
                DrillHingeHoles = false;
                DrillShelfHoles = false;
                RolloutCount = 0;
                ShelfCount = 0;
            }

            if (Style == Style3)
            {
                if (DoorCount == 1)
                { DoorCount = 2; }
                DrwCount = 0;
                TopType = CabinetOptions.TopType.Full;
                BackThickness = "0.75"; // Force 3/4" back
                ShelfDepth = CabinetOptions.ShelfDepth.FullDepth; // Force full-depth shelves
            }

            if (Style == Style4)
            {
                DrwCount = 0;
                RolloutCount = 0;
                TopType = CabinetOptions.TopType.Full;
                BackThickness = "0.75"; // Force 3/4" back
            }
        }
    }
}
