using CafePos.Application.Interfaces;
using CafePos.Infrastructure.Persistence;
using CafePos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CafePos.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PosDb") ?? "Data Source=pos.db";
        services.AddDbContext<CafePosDbContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Singleton);
        services.AddSingleton<IPosDbContext>(sp => sp.GetRequiredService<CafePosDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IReceiptNumberService, ReceiptNumberService>();

        var printerName = configuration.GetSection("Printer").GetValue<string>("Name");
        services.AddScoped<IPrinterService>(_ => new ReceiptPrinterService(printerName));

        services.AddSingleton<DatabaseInitializer>();
        return services;
    }
}
