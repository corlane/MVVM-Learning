using System.Collections.ObjectModel;


namespace CorlaneCabinetOrderFormV3.Services;

// MaterialLookupService.cs
// Concrete implementation of IMaterialLookupService. Provides the master lists of
// cabinet panel species and edge banding species that populate species/EB dropdowns
// throughout the app (base cabinet, upper cabinet, filler, and panel forms).
//
// Both collections are seeded at startup with Corlane's standard material options:
//   - CabinetSpecies: common plywood species (Maple, Red Oak, White Oak, Cherry, Alder,
//     Mahogany, Walnut, Hickory), Prefinished Ply, MDF, White/Black Melamine, and Custom
//   - EBSpecies: PVC options (White, Black, Hardrock Maple, Paint Grade), wood edge
//     banding in standard and prefinished variants for all supported species, and Custom
//
// "Custom" is always the last entry in both lists, allowing the user to type a free-form
// species name that isn't in the standard list.
//
// AddCabinetSpecies / RemoveCabinetSpecies marshal to the UI thread via Dispatcher.Invoke
// before mutating CabinetSpecies, since ObservableCollection raises CollectionChanged on
// the thread it is modified from and WPF requires that to be the UI thread.
//
// Note: EBSpecies is currently read-only at runtime (no Add/Remove exposed on the
// interface). Save/Load of custom species entries is not yet implemented.

public class MaterialLookupService : IMaterialLookupService
{
    public ObservableCollection<string> CabinetSpecies { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> EBSpecies { get; } = new ObservableCollection<string>();

    public MaterialLookupService()
    {
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
        CabinetSpecies.Add("White Melamine");
        CabinetSpecies.Add("Black Melamine");
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