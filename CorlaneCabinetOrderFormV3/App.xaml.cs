using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.Themes;
using CorlaneCabinetOrderFormV3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Windows;

namespace CorlaneCabinetOrderFormV3;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// Builds a user-facing message for an unhandled UI-thread exception.
    /// Returns (message, title, shouldHandle).
    /// </summary>
    internal static (string Message, string Title, bool Handled) BuildDispatcherExceptionResponse(Exception ex)
    {
        var msg = $"An unexpected error occurred:\n\n{ex.Message}\n\nThe application will attempt to continue.";
        return (msg, "Unexpected Error", true);
    }

    /// <summary>
    /// Builds a user-facing message for a fatal (non-UI-thread) exception.
    /// Returns null if the exception object is not an Exception.
    /// </summary>
    internal static (string Message, string Title)? BuildFatalExceptionResponse(object exceptionObject)
    {
        if (exceptionObject is not Exception ex)
            return null;

        var msg = $"A fatal error occurred:\n\n{ex.Message}";
        return (msg, "Fatal Error");
    }

    protected override async void OnStartup(StartupEventArgs e)
    {

        base.OnStartup(e);

        // ── Global exception safety net ──────────────────────────────
        DispatcherUnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[UnhandledException] {args.Exception}");
            var (msg, title, handled) = BuildDispatcherExceptionResponse(args.Exception);
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = handled;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[FatalException] {args.ExceptionObject}");
            var response = BuildFatalExceptionResponse(args.ExceptionObject);
            if (response is var (msg, title))
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        };

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

                services.AddSingleton<Cabinet3DViewModel>();

                services.AddTransient<PlaceOrderViewModel>();
                services.AddTransient<MaterialPricesViewModel>();
                services.AddTransient<ProcessOrderViewModel>();
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
                services.AddTransient<PONotesViewModel>();
                services.AddTransient<POOpeningDrwFrontHeightsViewModel>();
                services.AddTransient<PODrawerBoxesViewModel>();
                services.AddTransient<POBatchListViewModel>();
                services.AddTransient<PODrwStretcherWidthsViewModel>();
            })
            .Build();

        ServiceProvider = host.Services;
        var cabinetService = ServiceProvider.GetRequiredService<ICabinetService>();
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


        mainWindow.Show();

        // ── Crash recovery check ─────────────────────────────────────
        if (AutoSaveService.HasRecoveryFile())
        {
            var result = MessageBox.Show(
                "A recovery file was found from a previous session.\n\n" +
                "Would you like to restore it?",
                "Recover Unsaved Work",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var job = await cabinetService.LoadAsync(AutoSaveService.RecoveryFilePath);
                    if (job != null)
                    {
                        mainVm.CurrentJobName = "Recovered Job";
                        mainVm.CurrentJobPath = null;
                        mainVm.IsModified = true;

                        mainVm.POCustomerInfoVm.CompanyName = job.CustomerInfo.CompanyName;
                        mainVm.POCustomerInfoVm.ContactName = job.CustomerInfo.ContactName;
                        mainVm.POCustomerInfoVm.PhoneNumber = job.CustomerInfo.PhoneNumber;
                        mainVm.POCustomerInfoVm.EMail = job.CustomerInfo.EMail;
                        mainVm.POCustomerInfoVm.Street = job.CustomerInfo.Street;
                        mainVm.POCustomerInfoVm.City = job.CustomerInfo.City;
                        mainVm.POCustomerInfoVm.ZipCode = job.CustomerInfo.ZipCode;
                        mainVm.POCustomerInfoVm.QuotedTotalPrice = job.QuotedTotalPrice;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not restore recovery file:\n\n{ex.Message}",
                        "Recovery Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }

            // Either way, clean up the recovery file
            AutoSaveService.DeleteRecoveryFile();
        }

        // One-time popup — bump this version string whenever you have a new notice
        const string currentPopupVersion = "3.0.1.35";
        if (defaults.HasSeenPopup != currentPopupVersion)
        {
            MessageBox.Show(
                "NEW FEATURES:\n\n" + 
                "You can now adjust the interface scale using the slider in the bottom-right corner of the window.\n" +
                "This is helpful for smaller screens (scale down) or larger displays (scale up).\n\n" + 
                "Also, there is now a colored horizontal bar between the 3D Preview and the Cabinet List. " +
                "Dragging this bar up or down allows you to resize the 3D Preview and Cabinet List to your liking.",
                "What's New",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            defaults.HasSeenPopup = currentPopupVersion;
            _ = defaults.SaveAsync();
        }
    }
}