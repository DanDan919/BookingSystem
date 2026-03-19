using Booking.Application.Interfaces;
using Booking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Console.WriteLine("=== AddInfrastructure START ===");

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"[INFRA] ConnectionString: {connectionString}");

        services.AddDbContext<Persistence.BookingDbContext>(options =>
        {
            Console.WriteLine("[INFRA] Конфигурация BookingDbContext (PostgreSQL)");
            options.UseNpgsql(connectionString);
        });

        Console.WriteLine("[INFRA] Регистрация сервисов");

        services.AddScoped<IRoomService, RoomService>();
        Console.WriteLine("[INFRA] IRoomService -> RoomService");

        services.AddScoped<IBookingService, BookingService>();
        Console.WriteLine("[INFRA] IBookingService -> BookingService");

        Console.WriteLine("=== AddInfrastructure END ===");

        return services;
    }
}