using System.Collections.ObjectModel;


namespace CorlaneCabinetOrderFormV3.Services;


public class MaterialLookupService : IMaterialLookupService
{
    public ObservableCollection<string> CabinetSpecies { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> EBSpecies { get; } = new ObservableCollection<string>();

    public MaterialLookupService()
    {
        // seed defaults or load from persisted store (JSON, appsettings, DB)
        CabinetSpecies.Add("Prefinished Ply");
        CabinetSpecies.Add("Maple Ply");
        CabinetSpecies.Add("Red Oak Ply");
        CabinetSpecies.Add("White Oak Ply");
        CabinetSpecies.Add("Cherry Ply");
        CabinetSpecies.Add("Alder Ply");
        CabinetSpecies.Add("Mahogany Ply");
        CabinetSpecies.Add("Walnut Ply");
        CabinetSpecies.Add("Hickory Ply");
        CabinetSpecies.Add("MDF");
        CabinetSpecies.Add("Melamine");
        CabinetSpecies.Add("Custom");



        EBSpecies.Add("None");
        EBSpecies.Add("PVC White");
        EBSpecies.Add("PVC Black");
        EBSpecies.Add("PVC Hardrock Maple");
        EBSpecies.Add("PVC Paint Grade");
        EBSpecies.Add("Wood Maple");
        EBSpecies.Add("Wood Red Oak");
        EBSpecies.Add("Wood White Oak");
        EBSpecies.Add("Wood Walnut");
        EBSpecies.Add("Wood Cherry");
        EBSpecies.Add("Wood Alder");
        EBSpecies.Add("Wood Hickory");
        EBSpecies.Add("Wood Mahogany");
        EBSpecies.Add("Wood Prefinished Maple");
        EBSpecies.Add("Wood Prefinished Red Oak");
        EBSpecies.Add("Wood Prefinished White Oak");
        EBSpecies.Add("Wood Prefinished Cherry");
        EBSpecies.Add("Wood Prefinished Alder");
        EBSpecies.Add("Wood Prefinished Mahogany");
        EBSpecies.Add("Wood Prefinished Walnut");
        EBSpecies.Add("Wood Prefinished Hickory");
        EBSpecies.Add("Custom");
    }

    public void AddCabinetSpecies(string name) => App.Current.Dispatcher.Invoke(() => CabinetSpecies.Add(name));
    public void RemoveCabinetSpecies(string name) => App.Current.Dispatcher.Invoke(() => CabinetSpecies.Remove(name));
    // Save/Load implementations if desired
}