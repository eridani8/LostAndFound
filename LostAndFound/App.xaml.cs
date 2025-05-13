using System.IO;
using System.Windows;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LostAndFound;

public partial class App : Application
{
    private static IServiceProvider ServiceProvider { get; set; } = null!;
    private static AppSettings Settings { get; set; } = null!;
    
    public static User? CurrentUser { get; set; }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        Settings = new AppSettings();
        
        Directory.CreateDirectory(Settings.ReportsDirectory);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        ServiceProvider = services.BuildServiceProvider();
        
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Settings);
        
        services.AddSingleton<IDatabaseConnectionFactory, SqlConnectionFactory>();
        
        services.AddTransient<UserRepository>();
        services.AddTransient<ActionLogRepository>();
        
        services.AddTransient<IUserService, UserService>();
        
        services.AddTransient<MainWindow>();
    }
}