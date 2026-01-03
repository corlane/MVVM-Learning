using CommunityToolkit.Mvvm.Input;
using CorlaneCabinetOrderFormV3.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace CorlaneCabinetOrderFormV3.ViewModels;

public partial class MainWindowViewModel
{
    [RelayCommand]
    private void PrintCabinetList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        printer.PrintCabinetList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            dimensionFormat: defaults.DefaultDimensionFormat ?? "Fraction",
            cabinets: _cabinet_service.Cabinets.ToList());
    }

    [RelayCommand]
    private void PrintDoorList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        var doorVm = App.ServiceProvider.GetRequiredService<DoorSizesListViewModel>();
        doorVm.Rebuild();

        printer.PrintDoorList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            doors: doorVm.DoorSizes.ToList());
    }

    [RelayCommand]
    private void PrintDrawerBoxList()
    {
        var defaults = App.ServiceProvider.GetRequiredService<DefaultSettingsService>();
        var printer = App.ServiceProvider.GetRequiredService<IPrintService>();

        var drawerVm = App.ServiceProvider.GetRequiredService<DrawerBoxSizesListViewModel>();
        drawerVm.Rebuild();

        printer.PrintDrawerBoxList(
            companyName: defaults.CompanyName ?? "",
            jobName: CurrentJobName,
            drawerBoxes: drawerVm.DrawerBoxSizes.ToList());
    }
}