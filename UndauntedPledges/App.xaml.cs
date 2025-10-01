using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using UndauntedPledges.Implementations;
using UndauntedPledges.Interfaces;
using UndauntedPledges.Models;
using UndauntedPledges.View;
using UndauntedPledges.ViewModel;

namespace UndauntedPledges;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection services = new();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Views
        services.AddTransient<MainWindow>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        services.AddSingleton(configuration);

        services.AddTransient<IPledgeSource, ApplicationPledgeSource>();
        services.AddTransient<IPledgeSource, EsoHubPledgeSource>();
        services.AddTransient<IPledgeContext, PledgeContext>();

        services.AddOptions<ApplicationSettings>()
            .BindConfiguration(string.Empty);

        services.AddOptions<PledgesMap>()
            .BindConfiguration("PledgesMap");
    }
}
