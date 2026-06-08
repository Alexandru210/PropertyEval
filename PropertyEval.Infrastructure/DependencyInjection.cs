using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyEval.Infrastructure.Data;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add services
        services.AddScoped<UserService>();
        services.AddScoped<PropertyService>();
        services.AddScoped<PropertyImageService>();
        services.AddScoped<ListingService>();
        services.AddScoped<EvaluationService>();
        services.AddScoped<PropertyValuationService>();

        return services;
    }
}
