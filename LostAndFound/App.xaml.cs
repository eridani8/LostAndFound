using System.IO;
using System.Windows;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Services;
using LostAndFound.ViewModels;
using LostAndFound.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.DependencyInjection;

namespace LostAndFound;

public partial class App : Application
{
    private readonly IHost _host;

    public static User? CurrentUser { get; set; }

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                (context, config) =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false);
                }
            )
            .ConfigureServices(
                (context, services) =>
                {
                    ConfigureServices(context.Configuration, services);
                }
            )
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await _host.StartAsync();

        var loginWindow = _host.Services.GetRequiredService<LoginView>();
        loginWindow.Show();

        var usersRepository = _host.Services.GetRequiredService<UserRepository>();
        if (await usersRepository.GetByLoginAsync("root") is null)
        {
            await usersRepository.AddAsync(
                new User
                {
                    Login = "root",
                    Email = "tsuuzetsu@gmail.com",
                    FullName = "Eridani",
                    Phone = "+1234567890",
                    IsActive = true,
                    PasswordHash =
                        "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                    CreatedDate = DateTime.Now,
                    RoleId = 1,
                }
            );
        }

        Current.MainWindow = loginWindow;
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

        services.AddSingleton<UserRepository>();
        services.AddSingleton<ActionLogRepository>();
        services.AddSingleton<CategoryRepository>();

        services.AddSingleton<IUserService, UserService>();

        services.AddNavigationViewPageProvider();

        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IContentDialogService, ContentDialogService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<UsersViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<CategoriesViewModel>();

        services.AddSingleton<LoginView>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<UsersView>();
        services.AddSingleton<LogsView>();
        services.AddSingleton<CategoriesView>();
    }
}
