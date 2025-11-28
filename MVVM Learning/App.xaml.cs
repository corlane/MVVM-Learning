using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVVM_Learning.Services;
using MVVM_Learning.ViewModels;
using System.Windows;

namespace MVVM_Learning;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {

        base.OnStartup(e);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Register the shared service as singleton
                services.AddSingleton<ICabinetService, CabinetService>();

                // Register ViewModels as transients
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<BaseCabinetViewModel>();
                services.AddTransient<UpperCabinetViewModel>();
                services.AddTransient<CabinetListViewModel>();
                services.AddTransient<FillerViewModel>();
                services.AddTransient<PanelViewModel>();
            })
            .Build();

        ServiceProvider = host.Services;

        // Set MainWindow DataContext
        var mainWindow = new MainWindow
        {
//#pragma warning disable WPF0001
//            ThemeMode = ThemeMode.System,
//#pragma warning restore WPF0001

            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()

        };
        mainWindow.Show();
    }
}