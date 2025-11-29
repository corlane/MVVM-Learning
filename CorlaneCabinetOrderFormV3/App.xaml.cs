using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CorlaneCabinetOrderFormV3.Services;
using CorlaneCabinetOrderFormV3.ViewModels;
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

                // Register ViewModels as transients
                //services.AddTransient<MainWindowViewModel>();
                services.AddTransient<BaseCabinetViewModel>();
                services.AddTransient<UpperCabinetViewModel>();
                services.AddTransient<CabinetListViewModel>();
                services.AddTransient<FillerViewModel>();
                services.AddTransient<PanelViewModel>();
                services.AddTransient<DefaultSettingsViewModel>();
            })
            .Build();

        ServiceProvider = host.Services;
        var defaults = ServiceProvider.GetRequiredService<DefaultSettingsService>();
        await defaults.LoadAsync();

        // Set MainWindow DataContext
        var mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()

        };
        mainWindow.Show();
    }

}