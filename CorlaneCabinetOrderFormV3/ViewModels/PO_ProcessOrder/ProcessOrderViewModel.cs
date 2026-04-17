using CommunityToolkit.Mvvm.ComponentModel;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class ProcessOrderViewModel : ObservableValidator
{
    public ProcessOrderViewModel()
    {
        // design-time
    }

    public ProcessOrderViewModel(
        POCustomerInfoViewModel poCustomerInfoVm,
        POBatchListViewModel poBatchListVm,
        POJobMaterialListViewModel poJobMaterialListVm,
        POToekickViewModel poToekickVm,
        POEdgebandingViewModel poEdgebandingVm,
        POHingeHolesViewModel poHingeHolesVm,
        PODrawerBoxesViewModel poDrawerBoxesVm,
        PODrwStretcherWidthsViewModel poDrwStretcherWidthsVm,
        POCabinetSpeciesViewModel poCabinetSpeciesVm,
        POIncludeDoorsViewModel poIncludeDoorsVm,
        POCornerCabinetDimsViewModel poCornerCabinetDimsVm,
        PODoorDrwGrainDirViewModel poDoorDrwGrainDirVm,
        PORevealsGapsViewModel poRevealsGapsVm,
        POOpeningDrwFrontHeightsViewModel poOpeningDrwFrontHeightsVm,
        PONotesViewModel poNotesVm,
        POBackGrainDirViewModel poBackGrainDirVm,
        POEdgebandDoorsViewModel poEdgebandDoorsVm)
    {
        POCustomerInfoVm = poCustomerInfoVm ?? throw new ArgumentNullException(nameof(poCustomerInfoVm));
        POBatchListVm = poBatchListVm ?? throw new ArgumentNullException(nameof(poBatchListVm));
        POJobMaterialListVm = poJobMaterialListVm ?? throw new ArgumentNullException(nameof(poJobMaterialListVm));
        POToekickVm = poToekickVm ?? throw new ArgumentNullException(nameof(poToekickVm));
        POEdgebandingVm = poEdgebandingVm ?? throw new ArgumentNullException(nameof(poEdgebandingVm));
        POHingeHolesVm = poHingeHolesVm ?? throw new ArgumentNullException(nameof(poHingeHolesVm));
        PODrawerBoxesVm = poDrawerBoxesVm ?? throw new ArgumentNullException(nameof(poDrawerBoxesVm));
        PODrwStretcherWidthsVm = poDrwStretcherWidthsVm ?? throw new ArgumentNullException(nameof(poDrwStretcherWidthsVm));
        POCabinetSpeciesVm = poCabinetSpeciesVm ?? throw new ArgumentNullException(nameof(poCabinetSpeciesVm));
        POIncludeDoorsVm = poIncludeDoorsVm ?? throw new ArgumentNullException(nameof(poIncludeDoorsVm));
        POCornerCabinetDimsVm = poCornerCabinetDimsVm ?? throw new ArgumentNullException(nameof(poCornerCabinetDimsVm));
        PODoorDrwGrainDirVm = poDoorDrwGrainDirVm ?? throw new ArgumentNullException(nameof(poDoorDrwGrainDirVm));
        PORevealsGapsVm = poRevealsGapsVm ?? throw new ArgumentNullException(nameof(poRevealsGapsVm));
        POOpeningDrwFrontHeightsVm = poOpeningDrwFrontHeightsVm ?? throw new ArgumentNullException(nameof(poOpeningDrwFrontHeightsVm));
        PONotesVm = poNotesVm ?? throw new ArgumentNullException(nameof(poNotesVm));
        POBackGrainDirVm = poBackGrainDirVm ?? throw new ArgumentNullException(nameof(poBackGrainDirVm));
        POEdgebandDoorsVm = poEdgebandDoorsVm ?? throw new ArgumentNullException(nameof(poEdgebandDoorsVm));
    }

    public POCustomerInfoViewModel POCustomerInfoVm { get; }
    public POBatchListViewModel POBatchListVm { get; }
    public POJobMaterialListViewModel POJobMaterialListVm { get; }
    public POToekickViewModel POToekickVm { get; }
    public POEdgebandingViewModel POEdgebandingVm { get; }
    public POHingeHolesViewModel POHingeHolesVm { get; }
    public PODrawerBoxesViewModel PODrawerBoxesVm { get; }
    public PODrwStretcherWidthsViewModel PODrwStretcherWidthsVm { get; }
    public POCabinetSpeciesViewModel POCabinetSpeciesVm { get; }
    public POIncludeDoorsViewModel POIncludeDoorsVm { get; }
    public POCornerCabinetDimsViewModel POCornerCabinetDimsVm { get; }
    public PODoorDrwGrainDirViewModel PODoorDrwGrainDirVm { get; }
    public PORevealsGapsViewModel PORevealsGapsVm { get; }
    public POOpeningDrwFrontHeightsViewModel POOpeningDrwFrontHeightsVm { get; }
    public PONotesViewModel PONotesVm { get; }
    public POBackGrainDirViewModel POBackGrainDirVm { get; }
    public POEdgebandDoorsViewModel POEdgebandDoorsVm { get; }
}