using System.Windows;
using CafePos.Application.Services;
using CafePos.Infrastructure;
using CafePos.Infrastructure.Services;
using CafePos.Presentation.Services;
using CafePos.Presentation.ViewModels;
using CafePos.Presentation.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CafePos.Presentation;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder => builder.AddConsole());
                services.AddInfrastructure(context.Configuration);

                services.AddScoped<AuthService>();
                services.AddScoped<MenuService>();
                services.AddScoped<OrderService>();
                services.AddScoped<OrderQueryService>();
                services.AddScoped<PaymentService>();
                services.AddScoped<ShiftService>();
                services.AddScoped<ReportService>();
                services.AddScoped<ReportExportService>();
                services.AddScoped<UserService>();

                services.AddSingleton<SessionState>();
                services.AddSingleton<NavigationService>();

                services.AddSingleton<MainViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<SetupViewModel>();
                services.AddTransient<OpenShiftViewModel>();
                services.AddTransient<MainPosViewModel>();
                services.AddTransient<PaymentViewModel>();
                services.AddTransient<ShiftCloseViewModel>();
                services.AddTransient<HoldRecallViewModel>();
                services.AddTransient<MenuSetupViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<UsersViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var initializer = _host.Services.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
