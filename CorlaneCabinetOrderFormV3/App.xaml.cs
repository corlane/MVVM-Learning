using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Themes;
using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CorlaneCabinetOrderFormV3;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {

        base.OnStartup(e);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Register the shared service as singleton
                services.AddSingleton<ICabinetService, CabinetService>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<DefaultSettingsService>();
                services.AddSingleton<IPreviewService, PreviewService>();
                services.AddSingleton<IPrintService, PrintService>();
                services.AddSingleton<IMaterialPricesService, MaterialPricesService>();
                services.AddSingleton<POCustomerInfoViewModel>();

                // Register ViewModels as transients
                //services.AddTransient<MainWindowViewModel>();
                services.AddTransient<BaseCabinetViewModel>();
                services.AddTransient<UpperCabinetViewModel>();
                services.AddTransient<CabinetListViewModel>();
                services.AddTransient<FillerViewModel>();
                services.AddTransient<PanelViewModel>();
                services.AddTransient<DefaultSettingsViewModel>();
                services.AddTransient<Cabinet3DViewModel>();
                services.AddTransient<PlaceOrderViewModel>();
                services.AddTransient<ProcessOrderViewModel>();
                services.AddTransient<REALLYProcessOrderViewModel>();
                services.AddTransient<DoorSizesListViewModel>();
                services.AddTransient<DrawerBoxSizesListViewModel>();
            })
            .Build();

        ServiceProvider = host.Services;
        var defaults = ServiceProvider.GetRequiredService<DefaultSettingsService>();
        await defaults.LoadAsync();

        // NEW: best-effort material prices refresh on startup
        try
        {
            var prices = ServiceProvider.GetRequiredService<IMaterialPricesService>();
            await prices.RefreshFromServerAsync();
        }
        catch
        {
            // ignore: app can run offline
        }

        // Apply persisted theme (if any) before creating/showing MainWindow
        if (!string.IsNullOrWhiteSpace(defaults.DefaultTheme))
        {
            var theme = defaults.DefaultTheme;
            ThemeType t = theme switch
            {
                "Soft Dark" => ThemeType.SoftDark,
                "Red Black Theme" => ThemeType.RedBlackTheme,
                "Deep Dark" => ThemeType.DeepDark,
                "Grey Theme" => ThemeType.GreyTheme,
                "Dark Grey Theme" => ThemeType.DarkGreyTheme,
                "Light Theme" => ThemeType.LightTheme,
                _ => ThemeType.LightTheme
            };
            ThemesController.SetTheme(t);
        }

        // Set MainWindow DataContext
        var mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
        };

        // Ensure the PreviewService has an initial active owner (the current SelectedTabIndex)
        // Otherwise RequestPreview(ownerIndex, model) calls made by the tab VMs are ignored
        var previewSvc = ServiceProvider.GetRequiredService<IPreviewService>();
        var mainVm = ServiceProvider.GetRequiredService<MainWindowViewModel>();
        previewSvc.SetActiveOwner(mainVm.SelectedTabIndex);

        mainWindow.Show();
    }

}