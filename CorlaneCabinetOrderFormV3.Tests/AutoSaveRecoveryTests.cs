using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.IO;

namespace CorlaneCabinetOrderFormV3.Tests;

public class AutoSaveRecoveryTests
{
    [Fact]
    public void HasRecoveryFile_ReturnsFalse_WhenNoFileExists()
    {
        // Clean slate
        AutoSaveService.DeleteRecoveryFile();

        Assert.False(AutoSaveService.HasRecoveryFile());
    }

    [Fact]
    public async Task SaveRecoveryAsync_CreatesRecoveryFile()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "TestCab", Width = "24", Height = "30", Depth = "24" });

        var autoSave = new AutoSaveService(svc);
        autoSave.Configure(
            () => new JobCustomerInfo { CompanyName = "Test Co" },
            () => 123.45m);

        try
        {
            await autoSave.SaveRecoveryAsync();

            Assert.True(AutoSaveService.HasRecoveryFile());
            Assert.True(File.Exists(AutoSaveService.RecoveryFilePath));
        }
        finally
        {
            AutoSaveService.DeleteRecoveryFile();
            autoSave.Dispose();
        }
    }

    [Fact]
    public async Task SaveRecoveryAsync_SkipsWrite_WhenNoCabinets()
    {
        AutoSaveService.DeleteRecoveryFile();

        var svc = new CabinetService(); // empty — no cabinets
        var autoSave = new AutoSaveService(svc);

        try
        {
            await autoSave.SaveRecoveryAsync();

            Assert.False(AutoSaveService.HasRecoveryFile());
        }
        finally
        {
            autoSave.Dispose();
        }
    }

    [Fact]
    public async Task RecoveryFile_CanBeLoadedBack()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel
        {
            Name = "RecoverMe",
            Width = "30",
            Height = "34.5",
            Depth = "24",
            Species = "Maple",
            Style = CabinetStyles.Base.Standard
        });

        var autoSave = new AutoSaveService(svc);
        autoSave.Configure(
            () => new JobCustomerInfo { CompanyName = "Acme Cabinets", ContactName = "Bob" },
            () => 999.99m);

        try
        {
            // Simulate auto-save
            await autoSave.SaveRecoveryAsync();
            Assert.True(AutoSaveService.HasRecoveryFile());

            // Simulate next-launch recovery: load it with a fresh service
            var loadSvc = new CabinetService();
            var job = await loadSvc.LoadAsync(AutoSaveService.RecoveryFilePath);

            Assert.NotNull(job);
            Assert.Single(job.Cabinets);
            Assert.Equal("RecoverMe", job.Cabinets[0].Name);
            Assert.Equal("Acme Cabinets", job.CustomerInfo.CompanyName);
            Assert.Equal("Bob", job.CustomerInfo.ContactName);
            Assert.Equal(999.99m, job.QuotedTotalPrice);

            // submittedWithAppTitle is null for recovery files
            Assert.Null(job.SubmittedWithAppTitle);
        }
        finally
        {
            AutoSaveService.DeleteRecoveryFile();
            autoSave.Dispose();
        }
    }

    [Fact]
    public async Task DeleteRecoveryFile_RemovesFile()
    {
        var svc = new CabinetService();
        svc.Add(new BaseCabinetModel { Name = "Temp" });

        var autoSave = new AutoSaveService(svc);
        autoSave.Configure(() => new JobCustomerInfo(), () => 0m);

        try
        {
            await autoSave.SaveRecoveryAsync();
            Assert.True(AutoSaveService.HasRecoveryFile());

            AutoSaveService.DeleteRecoveryFile();
            Assert.False(AutoSaveService.HasRecoveryFile());
        }
        finally
        {
            autoSave.Dispose();
        }
    }

    [Fact]
    public void DeleteRecoveryFile_DoesNotThrow_WhenNoFile()
    {
        AutoSaveService.DeleteRecoveryFile(); // ensure clean
        AutoSaveService.DeleteRecoveryFile(); // should not throw
    }
}