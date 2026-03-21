using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Tests;

public class JobFileRoundTripTests
{
    [Fact]
    public async Task SaveAndLoad_PreservesCabinetData()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Base 1",
            Width = "24",
            Height = "30",
            Depth = "24",
            Species = "Maple",
            Style = CabinetStyles.Base.Standard
        });
        svc.Add(new UpperCabinetModel
        {
            Name = "Upper 1",
            Width = "30",
            Height = "36",
            Depth = "12",
            Species = "Cherry",
            Style = CabinetStyles.Upper.Standard
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 500m, "TestApp");

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.NotNull(loaded);
            Assert.Equal(2, loaded.Cabinets.Count);
            Assert.Equal(500m, loaded.QuotedTotalPrice);
            Assert.Equal("TestApp", loaded.SubmittedWithAppTitle);

            // Polymorphic types preserved
            Assert.IsType<BaseCabinetModel>(loaded.Cabinets[0]);
            Assert.IsType<UpperCabinetModel>(loaded.Cabinets[1]);

            // Data preserved
            var baseCab = (BaseCabinetModel)loaded.Cabinets[0];
            Assert.Equal("Base 1", baseCab.Name);
            Assert.Equal("24", baseCab.Width);
            Assert.Equal("Maple", baseCab.Species);
            Assert.Equal(CabinetStyles.Base.Standard, baseCab.Style);

            var upperCab = (UpperCabinetModel)loaded.Cabinets[1];
            Assert.Equal("Upper 1", upperCab.Name);
            Assert.Equal("Cherry", upperCab.Species);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAndLoad_PreservesExceptionDoneKeys()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "B1" });
        svc.ExceptionDoneKeys["HingeHoles"] = new HashSet<string> { "B1" };

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            await loadSvc.LoadAsync(tempFile);

            Assert.True(loadSvc.ExceptionDoneKeys.ContainsKey("HingeHoles"));
            Assert.Contains("B1", loadSvc.ExceptionDoneKeys["HingeHoles"]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}