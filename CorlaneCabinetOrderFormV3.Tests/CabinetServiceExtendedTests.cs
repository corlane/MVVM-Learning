using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Extended CabinetService tests: update semantics, reorder, clear.
/// </summary>
public class CabinetServiceExtendedTests
{
    //############################################################################################################
    // Update (remove + re-add) preserves logical state
    //############################################################################################################

    [Fact]
    public void UpdateCabinet_ReplaceInList_PreservesCount()
    {
        var svc = new CabinetService();
        var original = new BaseCabinetModel { Name = "Base 1", Width = "24" };
        svc.Add(original);

        // Simulate update: remove old, add new with same name
        svc.Remove(original);
        var updated = new BaseCabinetModel { Name = "Base 1", Width = "30" };
        svc.Add(updated);

        Assert.Single(svc.Cabinets);
        Assert.Equal("30", svc.Cabinets[0].Width);
    }

    [Fact]
    public void UpdateCabinet_MutateInPlace_ChangesReflected()
    {
        var svc = new CabinetService();
        var cab = new BaseCabinetModel { Name = "Base 1", Width = "24", Species = "Maple" };
        svc.Add(cab);

        // Direct mutation (how the app actually updates)
        cab.Width = "30";
        cab.Species = "Cherry";

        Assert.Equal("30", svc.Cabinets[0].Width);
        Assert.Equal("Cherry", svc.Cabinets[0].Species);
    }

    //############################################################################################################
    // Reorder
    //############################################################################################################

    [Fact]
    public void MoveCabinet_ReordersCorrectly()
    {
        var svc = new CabinetService();
        var cab1 = new BaseCabinetModel { Name = "A" };
        var cab2 = new BaseCabinetModel { Name = "B" };
        var cab3 = new BaseCabinetModel { Name = "C" };
        svc.Add(cab1);
        svc.Add(cab2);
        svc.Add(cab3);

        // Move "C" to position 0
        svc.Cabinets.Move(2, 0);

        Assert.Equal("C", svc.Cabinets[0].Name);
        Assert.Equal("A", svc.Cabinets[1].Name);
        Assert.Equal("B", svc.Cabinets[2].Name);
    }

    //############################################################################################################
    // Clear
    //############################################################################################################

    [Fact]
    public void ClearCabinets_EmptiesList()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Base 1" });
        svc.Add(new UpperCabinetModel { Name = "Upper 1" });

        svc.Cabinets.Clear();

        Assert.Empty(svc.Cabinets);
    }

    //############################################################################################################
    // All four cabinet types can be added
    //############################################################################################################

    [Fact]
    public void Add_AllFourCabinetTypes_Succeeds()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Base 1" });
        svc.Add(new UpperCabinetModel { Name = "Upper 1" });
        svc.Add(new FillerModel { Name = "Filler 1" });
        svc.Add(new PanelModel { Name = "Panel 1" });

        Assert.Equal(4, svc.Cabinets.Count);
        Assert.IsType<BaseCabinetModel>(svc.Cabinets[0]);
        Assert.IsType<UpperCabinetModel>(svc.Cabinets[1]);
        Assert.IsType<FillerModel>(svc.Cabinets[2]);
        Assert.IsType<PanelModel>(svc.Cabinets[3]);
    }

    //############################################################################################################
    // ExceptionDoneKeys
    //############################################################################################################

    [Fact]
    public void ExceptionDoneKeys_MultipleTabIds_Independent()
    {
        var svc = new CabinetService();
        svc.ExceptionDoneKeys["HingeHoles"] = new HashSet<string> { "B1", "B2" };
        svc.ExceptionDoneKeys["Reveals"] = new HashSet<string> { "B1" };

        Assert.Equal(2, svc.ExceptionDoneKeys["HingeHoles"].Count);
        Assert.Single(svc.ExceptionDoneKeys["Reveals"]);
    }
}