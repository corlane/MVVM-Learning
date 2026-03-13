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
                services.AddSingleton<IPriceBreakdownService, PriceBreakdownService>();
                services.AddSingleton<POCustomerInfoViewModel>();
                services.AddSingleton<IMaterialLookupService, MaterialLookupService>();
                services.AddTransient<POJobMaterialListViewModel>();

                // Register ViewModels as transients
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
                services.AddTransient<POToekickViewModel>();
                services.AddTransient<POEdgebandingViewModel>();
                services.AddTransient<POHingeHolesViewModel>();
                services.AddTransient<POCabinetSpeciesViewModel>();
                services.AddSingleton<POIncludeDoorsViewModel>();
                services.AddTransient<PODoorDrwGrainDirViewModel>();
                services.AddTransient<PORevealsGapsViewModel>();
                services.AddTransient<POCornerCabinetDimsViewModel>();
                //services.AddTransient<POBaseCabTopTypeViewModel>();
                services.AddTransient<PONotesViewModel>();
                services.AddTransient<POOpeningDrwFrontHeightsViewModel>();
                services.AddTransient<PODrawerBoxesViewModel>();
                services.AddTransient<POBatchListViewModel>();
                services.AddTransient<PODrwStretcherWidthsViewModel>();
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
                //"Red Black Theme" => ThemeType.RedBlackTheme,
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


        // Window size/position/state persistence with some sanity checks to help ensure window isn't lost off-screen
        try
        {
            if (defaults.WindowWidth.HasValue && defaults.WindowHeight.HasValue)
            {
                if (defaults.WindowWidth > 0 && defaults.WindowHeight > 0)
                {
                    mainWindow.Width = defaults.WindowWidth.Value;
                    mainWindow.Height = defaults.WindowHeight.Value;
                }
            }

            if (defaults.WindowLeft.HasValue && defaults.WindowTop.HasValue)
            {
                mainWindow.Left = defaults.WindowLeft.Value;
                mainWindow.Top = defaults.WindowTop.Value;
            }

            if (!string.IsNullOrWhiteSpace(defaults.WindowState) &&
                Enum.TryParse<WindowState>(defaults.WindowState, out var ws) &&
                ws != WindowState.Minimized)
            {
                mainWindow.WindowState = ws;
            }

            // Ensure window is at least mostly visible on the current display(s)
            void ClampToVirtualScreen(Window w)
            {
                var vsLeft = SystemParameters.VirtualScreenLeft;
                var vsTop = SystemParameters.VirtualScreenTop;
                var vsWidth = SystemParameters.VirtualScreenWidth;
                var vsHeight = SystemParameters.VirtualScreenHeight;

                // If window bigger than virtual screen, shrink to fit
                if (w.Width > vsWidth) w.Width = Math.Max(300, vsWidth);
                if (w.Height > vsHeight) w.Height = Math.Max(200, vsHeight);

                // Ensure Left/Top are within virtual screen so at least 100px of window is visible
                var minVisible = 100.0;
                var maxLeft = vsLeft + vsWidth - minVisible;
                var maxTop = vsTop + vsHeight - minVisible;
                var minLeft = vsLeft - w.Width + minVisible;
                var minTop = vsTop - w.Height + minVisible;

                if (w.Left < minLeft) w.Left = Math.Max(vsLeft, minLeft);
                if (w.Left > maxLeft) w.Left = maxLeft;
                if (w.Top < minTop) w.Top = Math.Max(vsTop, minTop);
                if (w.Top > maxTop) w.Top = maxTop;
            }

            ClampToVirtualScreen(mainWindow);
        }
        catch
        {
            // best-effort — ignore any issues applying persisted bounds
        }

        mainWindow.Show();
    }

}