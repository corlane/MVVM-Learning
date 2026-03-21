using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;

namespace CorlaneCabinetOrderFormV3.Tests;

public class CabinetServiceTests
{
    [Fact]
    public void Add_UniqueNames_Succeeds()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Base 1" });
        svc.Add(new UpperCabinetModel { Name = "Upper 1" });

        Assert.Equal(2, svc.Cabinets.Count);
    }

    [Fact]
    public void Add_DuplicateName_Throws()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Base 1" });

        Assert.Throws<InvalidOperationException>(() =>
            svc.Add(new UpperCabinetModel { Name = "Base 1" }));
    }

    [Fact]
    public void Add_DuplicateName_CaseInsensitive()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Base 1" });

        Assert.Throws<InvalidOperationException>(() =>
            svc.Add(new BaseCabinetModel { Name = "base 1" }));
    }

    [Fact]
    public void Add_BlankNames_DoNotConflict()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "" });
        svc.Add(new BaseCabinetModel { Name = "" });
        svc.Add(new BaseCabinetModel { Name = "  " });

        Assert.Equal(3, svc.Cabinets.Count);
    }

    [Fact]
    public void Remove_RemovesCabinet()
    {
        var svc = new CabinetService();
        var cab = new BaseCabinetModel { Name = "Base 1" };
        svc.Add(cab);
        svc.Remove(cab);

        Assert.Empty(svc.Cabinets);
    }
}