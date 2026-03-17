using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.ViewModels;

namespace CorlaneCabinetOrderFormV3.Tests;

public class ViewModelMappingHelperTests
{
    // Lightweight stand-in so we don't need the real VM with all its dependencies
    private class FakeViewModel
    {
        public string Width { get; set; } = "";
        public string Height { get; set; } = "";
        public string Species { get; set; } = "";
        public int ShelfCount { get; set; }
        public bool HasTK { get; set; }
        public string Notes { get; set; } = "";
    }

    private static BaseCabinetModel MakeModel() => new()
    {
        Width = "24.5",
        Height = "34.5",
        Species = "Maple",
        ShelfCount = 3,
        HasTK = true,
        Notes = "Test note"
    };

    [Fact]
    public void MapModelToViewModel_FractionFormat_ConvertsDimensionProperties()
    {
        var vm = new FakeViewModel();
        var model = MakeModel();
        var dimProps = new HashSet<string> { "Width", "Height" };

        ViewModelMappingHelper.MapModelToViewModel(vm, model, "Fraction", dimProps);

        Assert.Equal("24 1/2", vm.Width);   // 24.5 → "24 1/2"
        Assert.Equal("34 1/2", vm.Height);  // 34.5 → "34 1/2"
    }

    [Fact]
    public void MapModelToViewModel_DecimalFormat_KeepsDecimalStrings()
    {
        var vm = new FakeViewModel();
        var model = MakeModel();
        var dimProps = new HashSet<string> { "Width", "Height" };

        ViewModelMappingHelper.MapModelToViewModel(vm, model, "Decimal", dimProps);

        Assert.Equal("24.5", vm.Width);
        Assert.Equal("34.5", vm.Height);
    }

    [Fact]
    public void MapModelToViewModel_NonDimensionStrings_CopiedVerbatim()
    {
        var vm = new FakeViewModel();
        var model = MakeModel();
        var dimProps = new HashSet<string> { "Width", "Height" }; // Species NOT in this set

        ViewModelMappingHelper.MapModelToViewModel(vm, model, "Fraction", dimProps);

        Assert.Equal("Maple", vm.Species);
        Assert.Equal("Test note", vm.Notes);
    }

    [Fact]
    public void MapModelToViewModel_IntAndBoolProperties_MappedCorrectly()
    {
        var vm = new FakeViewModel();
        var model = MakeModel();

        ViewModelMappingHelper.MapModelToViewModel(vm, model, "Fraction", []);

        Assert.Equal(3, vm.ShelfCount);
        Assert.True(vm.HasTK);
    }

    [Fact]
    public void MapModelToViewModel_PropertyNotOnVm_IsSkipped()
    {
        // FakeViewModel doesn't have TKDepth — should not throw
        var vm = new FakeViewModel();
        var model = MakeModel();
        model.TKDepth = "4";

        var ex = Record.Exception(() =>
            ViewModelMappingHelper.MapModelToViewModel(vm, model, "Fraction", []));

        Assert.Null(ex);
    }
}