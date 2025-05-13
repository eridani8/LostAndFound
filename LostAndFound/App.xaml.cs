using System.IO;
using System.Windows;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LostAndFound;

public partial class App : Application
{
    private readonly IHost _host;
    
    public static User? CurrentUser { get; set; }
    
    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();
    }
    
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        await _host.StartAsync();
        
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        
        base.OnExit(e);
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
        
        services.AddSingleton<IDatabaseConnectionFactory, SqlConnectionFactory>();
        
        services.AddTransient<UserRepository>();
        services.AddTransient<ActionLogRepository>();
        
        services.AddTransient<IUserService, UserService>();
        
        services.AddTransient<MainWindow>();
    }
}