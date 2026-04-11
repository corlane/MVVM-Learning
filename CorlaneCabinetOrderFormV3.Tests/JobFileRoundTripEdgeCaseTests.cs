using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Round-trip serialization tests for edge-case cabinet configurations:
/// sink cabinets, trash drawers, custom species, and IncTrashDrwBox.
/// </summary>
public class JobFileRoundTripEdgeCaseTests
{
    //############################################################################################################
    // Sink cabinet round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_SinkCabinet_PreservesSinkFlag()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Sink1",
            Style = CabinetStyles.Base.Standard,
            Width = "36",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            SinkCabinet = true,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            var cab = Assert.IsType<BaseCabinetModel>(loaded!.Cabinets[0]);
            Assert.True(cab.SinkCabinet);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Trash drawer + IncTrashDrwBox round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_TrashDrawer_PreservesFlags()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Trash1",
            Style = CabinetStyles.Base.Standard,
            Width = "18",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            TrashDrawer = true,
            IncTrashDrwBox = true,
            DrwCount = 1,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            var cab = Assert.IsType<BaseCabinetModel>(loaded!.Cabinets[0]);
            Assert.True(cab.TrashDrawer);
            Assert.True(cab.IncTrashDrwBox);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Custom species round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_CustomSpecies_PreservesCustomNames()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Custom1",
            Style = CabinetStyles.Base.Standard,
            Width = "24",
            Height = "34.5",
            Depth = "24",
            Species = "Custom",
            CustomSpecies = "Bamboo",
            EBSpecies = "Custom",
            CustomEBSpecies = "Bamboo EB",
            DoorSpecies = "Custom",
            CustomDoorSpecies = "Bamboo Doors",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 250m, "Test");

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            var cab = Assert.IsType<BaseCabinetModel>(loaded!.Cabinets[0]);
            Assert.Equal("Custom", cab.Species);
            Assert.Equal("Bamboo", cab.CustomSpecies);
            Assert.Equal("Custom", cab.EBSpecies);
            Assert.Equal("Bamboo EB", cab.CustomEBSpecies);
            Assert.Equal("Custom", cab.DoorSpecies);
            Assert.Equal("Bamboo Doors", cab.CustomDoorSpecies);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Equalize drawer front state round-trip (3.0.1.31)
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_EqualizeDrawerFrontState_Preserved()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Eq1",
            Style = CabinetStyles.Base.Drawer,
            Width = "18",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Stretcher,
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            DrwCount = 4,
            EqualizeAllDrwFronts = true,
            EqualizeBottomDrwFronts = false,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            var cab = Assert.IsType<BaseCabinetModel>(loaded!.Cabinets[0]);
            Assert.True(cab.EqualizeAllDrwFronts);
            Assert.False(cab.EqualizeBottomDrwFronts);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Qty round-trip (ensures Qty > 1 survives serialization)
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_CabinetQty_PreservesQuantity()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Multi",
            Style = CabinetStyles.Base.Standard,
            Width = "24",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            Qty = 5,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.Equal(5, loaded!.Cabinets[0].Qty);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Cabinet notes round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_CabinetNotes_PreservesText()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Noted",
            Style = CabinetStyles.Base.Standard,
            Width = "24",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            Notes = "Special instructions: use left-over maple from previous job",
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.Equal("Special instructions: use left-over maple from previous job",
                loaded!.Cabinets[0].Notes);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}