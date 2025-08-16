using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyEval.Domain.Interfaces;
using PropertyEval.Infrastructure.Data;
using PropertyEval.Infrastructure.Repositories;

namespace PropertyEval.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Entity Framework
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
