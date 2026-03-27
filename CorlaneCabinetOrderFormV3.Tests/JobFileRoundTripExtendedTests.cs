using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Tests;

/// <summary>
/// Round-trip tests for complex cabinet configurations:
/// drawer cabinets, corner cabinets, fillers, panels, rollouts, customer info.
/// </summary>
public class JobFileRoundTripExtendedTests
{
    //############################################################################################################
    // Drawer cabinet preserves opening heights, drawer front heights, equalize state
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_DrawerCabinet_PreservesDrawerProperties()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Drw1",
            Style = CabinetStyles.Base.Drawer,
            Width = "18",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            DrwCount = 3,
            DrwStyle = "Blum Tandem H/Equivalent Undermount",
            OpeningHeight1 = "8",
            OpeningHeight2 = "8",
            OpeningHeight3 = "10.75",
            DrwFrontHeight1 = "8.4375",
            DrwFrontHeight2 = "8.5625",
            DrwFrontHeight3 = "11.1875",
            EqualizeAllDrwFronts = false,
            EqualizeBottomDrwFronts = true,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 250m, "Test");

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.NotNull(loaded);
            var cab = Assert.IsType<BaseCabinetModel>(loaded.Cabinets[0]);

            Assert.Equal(CabinetStyles.Base.Drawer, cab.Style);
            Assert.Equal(3, cab.DrwCount);
            Assert.Equal("8", cab.OpeningHeight1);
            Assert.Equal("8", cab.OpeningHeight2);
            Assert.Equal("10.75", cab.OpeningHeight3);
            Assert.Equal("8.4375", cab.DrwFrontHeight1);
            Assert.Equal("8.5625", cab.DrwFrontHeight2);
            Assert.Equal("11.1875", cab.DrwFrontHeight3);
            Assert.False(cab.EqualizeAllDrwFronts);
            Assert.True(cab.EqualizeBottomDrwFronts);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Corner cabinet preserves corner dimensions
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_CornerCabinet_PreservesCornerDimensions()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "Corner1",
            Style = CabinetStyles.Base.Corner90,
            Width = "36",
            Height = "34.5",
            Depth = "24",
            Species = "Cherry",
            EBSpecies = "Wood Cherry",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            LeftBackWidth = "36",
            RightBackWidth = "36",
            LeftDepth = "24",
            RightDepth = "24",
            DoorCount = 2,
            MaterialThickness34 = 0.75,
            MaterialThickness14 = 0.25
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 100m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            var cab = Assert.IsType<BaseCabinetModel>(loaded!.Cabinets[0]);
            Assert.Equal(CabinetStyles.Base.Corner90, cab.Style);
            Assert.Equal("36", cab.LeftBackWidth);
            Assert.Equal("36", cab.RightBackWidth);
            Assert.Equal("24", cab.LeftDepth);
            Assert.Equal("24", cab.RightDepth);
            Assert.Equal("0.75", cab.BackThickness);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Filler & Panel round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_FillerAndPanel_PreservesTypes()
    {
        var svc = new CabinetService();
        svc.Add(new FillerModel
        {
            Name = "Filler 1",
            Width = "3",
            Height = "34.5",
            Depth = "0.75",
            Species = "Maple",
            EBSpecies = "Wood Maple"
        });
        svc.Add(new PanelModel
        {
            Name = "Panel 1",
            Width = "24",
            Height = "34.5",
            Depth = "0.75",
            Species = "Maple",
            EBSpecies = "None",
            PanelEBTop = true,
            PanelEBBottom = false,
            PanelEBLeft = true,
            PanelEBRight = false
        });

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, new JobCustomerInfo(), 0m, null);

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.Equal(2, loaded!.Cabinets.Count);
            Assert.IsType<FillerModel>(loaded.Cabinets[0]);

            var panel = Assert.IsType<PanelModel>(loaded.Cabinets[1]);
            Assert.True(panel.PanelEBTop);
            Assert.False(panel.PanelEBBottom);
            Assert.True(panel.PanelEBLeft);
            Assert.False(panel.PanelEBRight);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Customer info round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_CustomerInfo_PreservesAllFields()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "B1", Species = "Maple", EBSpecies = "Wood Maple" });

        var customerInfo = new JobCustomerInfo
        {
            CompanyName = "Acme Cabinets",
            ContactName = "John Doe",
            PhoneNumber = "555-1234",
            EMail = "john@acme.com",
            Street = "123 Main St",
            City = "Springfield",
            ZipCode = "62701"
        };

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cor");
        try
        {
            await svc.SaveAsync(tempFile, customerInfo, 999.99m, "v3.0.1.38");

            var loadSvc = new CabinetService();
            var loaded = await loadSvc.LoadAsync(tempFile);

            Assert.NotNull(loaded);
            Assert.Equal("Acme Cabinets", loaded.CustomerInfo.CompanyName);
            Assert.Equal("John Doe", loaded.CustomerInfo.ContactName);
            Assert.Equal("555-1234", loaded.CustomerInfo.PhoneNumber);
            Assert.Equal("john@acme.com", loaded.CustomerInfo.EMail);
            Assert.Equal("123 Main St", loaded.CustomerInfo.Street);
            Assert.Equal("Springfield", loaded.CustomerInfo.City);
            Assert.Equal("62701", loaded.CustomerInfo.ZipCode);
            Assert.Equal(999.99m, loaded.QuotedTotalPrice);
            Assert.Equal("v3.0.1.38", loaded.SubmittedWithAppTitle);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    //############################################################################################################
    // Rollout properties round-trip
    //############################################################################################################

    [Fact]
    public async Task RoundTrip_BaseCabinetWithRollouts_PreservesRolloutProperties()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "B-Rollout",
            Style = CabinetStyles.Base.Standard,
            Width = "24",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            EBSpecies = "Wood Maple",
            HasTK = true,
            TKHeight = "4",
            TKDepth = "3.75",
            BackThickness = "0.75",
            TopType = CabinetOptions.TopType.Full,
            IncRollouts = true,
            RolloutCount = 2,
            RolloutStyle = "Blum Tandem H/Equivalent Undermount",
            IncRolloutsInList = true,
            DrillSlideHolesForRollouts = true,
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
            Assert.True(cab.IncRollouts);
            Assert.Equal(2, cab.RolloutCount);
            Assert.Equal("Blum Tandem H/Equivalent Undermount", cab.RolloutStyle);
            Assert.True(cab.IncRolloutsInList);
            Assert.True(cab.DrillSlideHolesForRollouts);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}