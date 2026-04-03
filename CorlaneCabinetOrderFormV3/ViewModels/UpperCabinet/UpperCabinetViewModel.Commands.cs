using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class UpperCabinetViewModel : ObservableValidator
{
    [RelayCommand]
    private void AddCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
            return;

        EnforceStyleConstraints();

        var newCabinet = new UpperCabinetModel();
        ApplyViewModelToModel(newCabinet);

        try
        {
            _cabinetService?.Add(newCabinet);
            _mainVm!.SelectedCabinet = newCabinet;
        }
        catch (InvalidOperationException ex)
        {
            _mainVm?.Notify(ex.Message, Brushes.Red, 3000);
            return;
        }

        Notes = "";

        _mainVm?.Notify($"{newCabinet.Style} {newCabinet.CabinetType} {newCabinet.Name} Added", Brushes.MediumBlue);
        _mainVm?.IsModified = true;
    }

    [RelayCommand]
    private void UpdateCabinet()
    {
        if (!ViewModelValidationHelper.ValidateCustomSpecies(Species, CustomSpecies, EBSpecies, CustomEBSpecies, DoorSpecies, CustomDoorSpecies))
            return;

        if (_mainVm!.SelectedCabinet is UpperCabinetModel selected)
        {
            if (!ViewModelValidationHelper.ValidateUniqueName(Name, selected, _cabinetService, _mainVm))
                return;

            EnforceStyleConstraints();

            ApplyViewModelToModel(selected);

            _mainVm?.Notify("Cabinet Updated", Brushes.Green);
            _mainVm?.IsModified = true;
        }
        else
        {
            _mainVm?.Notify("No cabinet selected, or incorrect cabinet tab selected. Nothing updated.", Brushes.Red, 3000);
            return;
        }

        _mainVm!.SelectedCabinet = null;

        Notes = "";
    }

    [RelayCommand]
    private void LoadDefaults()
    {
        if (_defaults is null) return;
        Species = _defaults.DefaultSpecies;
        EBSpecies = _defaults.DefaultEBSpecies;
        ShelfCount = _defaults.DefaultUpperShelfCount;
        DrillShelfHoles = _defaults.DefaultDrillShelfHoles;
        if (_defaults.DefaultDimensionFormat == "Decimal") { BackThickness = _defaults.DefaultUpperBackThickness; }
        else { BackThickness = ConvertDimension.DoubleToFraction(Convert.ToDouble(_defaults.DefaultUpperBackThickness)); }
        //BackThickness = _defaults.DefaultUpperBackThickness;
        DoorCount = _defaults.DefaultDoorCount;
        IncDoors = _defaults.DefaultIncDoors;
        IncDoorsInList = _defaults.DefaultIncDoorsInList;
        DoorSpecies = _defaults.DefaultDoorDrwSpecies;
        DrillHingeHoles = _defaults.DefaultDrillHingeHoles;
        DoorGrainDir = _defaults.DefaultDoorGrainDir;
        LeftReveal = _defaults.DefaultUpperLeftReveal;
        RightReveal = _defaults.DefaultUpperRightReveal;
        TopReveal = _defaults.DefaultUpperTopReveal;
        BottomReveal = _defaults.DefaultUpperBottomReveal;
        GapWidth = _defaults.DefaultGapWidth;
    }

    /// <summary>
    /// Applies style-specific constraints before saving to a model
    /// (e.g. corner cabs force 3/4" back).
    /// </summary>
    private void EnforceStyleConstraints()
    {
        if (Style == Style2 || Style == Style3)
        {
            BackThickness = CabinetOptions.BackThickness.ThreeQuarterDecimal; // Force 3/4" back
        }
    }

}
