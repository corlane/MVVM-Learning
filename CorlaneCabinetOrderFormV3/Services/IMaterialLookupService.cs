using System.Collections.ObjectModel;

namespace CorlaneCabinetOrderFormV3.Services;

public interface IMaterialLookupService
{
    ObservableCollection<string> CabinetSpecies { get; }
    ObservableCollection<string> EBSpecies { get; }
    void AddCabinetSpecies(string name);
    void RemoveCabinetSpecies(string name);
    // optionally Save/Load methods
}