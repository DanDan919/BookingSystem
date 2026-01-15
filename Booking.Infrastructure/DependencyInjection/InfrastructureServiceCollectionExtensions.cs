/*//using Booking.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Booking.Infrastructure.Persistence; // чтобы был виден BookingDbContext
using Booking.Infrastructure.Services; // чтобы RoomService и BookingService были видны

namespace Booking.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Application-сервисы
            services.AddScoped<RoomService>();
            services.AddScoped<BookingService>();

            return services;
        }
    }
}
*/