using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class REALLYProcessOrderViewModel : ObservableValidator
{
    public REALLYProcessOrderViewModel()
    {
        // design-time
    }

    public REALLYProcessOrderViewModel(
        POCustomerInfoViewModel poCustomerInfoVm,
        POJobMaterialListViewModel poJobMaterialListVm,
        POToekickViewModel poToekickVm,
        POEdgebandingViewModel poEdgebandingVm,
        POHingeHolesViewModel poHingeHolesVm,
        POCabinetSpeciesViewModel poCabinetSpeciesVm,
        POIncludeDoorsViewModel poIncludeDoorsVm,
        PODoorSpeciesViewModel poDoorSpeciesVm,
        POCornerCabinetDimsViewModel poCornerCabinetDimsVm,
        PODoorDrwGrainDirViewModel poDoorDrwGrainDirVm,
        PORevealsGapsViewModel poRevealsGapsVm,
        POBaseCabTopTypeViewModel poBaseCabTopTypeVm,
        PONotesViewModel poNotesVm)
    {
        POCustomerInfoVm = poCustomerInfoVm ?? throw new ArgumentNullException(nameof(poCustomerInfoVm));
        POJobMaterialListVm = poJobMaterialListVm ?? throw new ArgumentNullException(nameof(poJobMaterialListVm));
        POToekickVm = poToekickVm ?? throw new ArgumentNullException(nameof(poToekickVm));
        POEdgebandingVm = poEdgebandingVm ?? throw new ArgumentNullException(nameof(poEdgebandingVm));
        POHingeHolesVm = poHingeHolesVm ?? throw new ArgumentNullException(nameof(poHingeHolesVm));
        POCabinetSpeciesVm = poCabinetSpeciesVm ?? throw new ArgumentNullException(nameof(poCabinetSpeciesVm));
        POIncludeDoorsVm = poIncludeDoorsVm ?? throw new ArgumentNullException(nameof(poIncludeDoorsVm));
        PODoorSpeciesVm = poDoorSpeciesVm ?? throw new ArgumentNullException(nameof(poDoorSpeciesVm));
        POCornerCabinetDimsVm = poCornerCabinetDimsVm ?? throw new ArgumentNullException(nameof(poCornerCabinetDimsVm));
        PODoorDrwGrainDirVm = poDoorDrwGrainDirVm ?? throw new ArgumentNullException(nameof(poDoorDrwGrainDirVm));
        PORevealsGapsVm = poRevealsGapsVm ?? throw new ArgumentNullException(nameof(poRevealsGapsVm));
        POBaseCabTopTypeVm = poBaseCabTopTypeVm ?? throw new ArgumentNullException(nameof(poBaseCabTopTypeVm));
        PONotesVm = poNotesVm ?? throw new ArgumentNullException(nameof(poNotesVm));
    }

    public POCustomerInfoViewModel POCustomerInfoVm { get; }
    public POJobMaterialListViewModel POJobMaterialListVm { get; }
    public POToekickViewModel POToekickVm { get; }
    public POEdgebandingViewModel POEdgebandingVm { get; }
    public POHingeHolesViewModel POHingeHolesVm { get; }
    public POCabinetSpeciesViewModel POCabinetSpeciesVm { get; }
    public POIncludeDoorsViewModel POIncludeDoorsVm { get; }
    public PODoorSpeciesViewModel PODoorSpeciesVm { get; }
    public POCornerCabinetDimsViewModel POCornerCabinetDimsVm { get; }
    public PODoorDrwGrainDirViewModel PODoorDrwGrainDirVm { get; }
    public PORevealsGapsViewModel PORevealsGapsVm { get; }
    public POBaseCabTopTypeViewModel POBaseCabTopTypeVm { get; }
    public PONotesViewModel PONotesVm { get; }
}