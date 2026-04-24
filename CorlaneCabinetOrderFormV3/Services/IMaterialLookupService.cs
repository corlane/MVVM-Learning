using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

// IMaterialLookupService.cs
// Defines the contract for the material lookup service that provides the master lists
// of available cabinet species and edge banding species used throughout the app.
//
// These lists populate species and EB species dropdowns on the base cabinet, upper cabinet,
// filler, and panel forms. The concrete implementation (MaterialLookupService) seeds both
// collections at startup with the standard Corlane material options (plywood species,
// melamine, MDF, PVC and wood edge banding options, plus "Custom" for user-defined entries).
//
// Abstracting behind this interface keeps ViewModels decoupled from the concrete
// implementation and makes the lists easy to mock for testing.
//
// Contract covers:
//   - CabinetSpecies: the master list of cabinet panel materials bound to species pickers
//   - EBSpecies: the master list of edge banding species bound to EB species pickers
//   - AddCabinetSpecies / RemoveCabinetSpecies: mutate the CabinetSpecies list on the
//     UI thread (used when a user adds or removes a custom species entry)

public interface IMaterialLookupService
{
    ObservableCollection<string> CabinetSpecies { get; }
    ObservableCollection<string> EBSpecies { get; }
    void AddCabinetSpecies(string name);
    void RemoveCabinetSpecies(string name);
    // optionally Save/Load methods
}