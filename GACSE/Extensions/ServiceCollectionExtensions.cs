using GACSE.Application.Interfaces.Repositories;
using GACSE.Application.Interfaces.Services;
using GACSE.Application.Services;
using GACSE.Infrastructure.Data;
using GACSE.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GACSE.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IMedicoService, MedicoService>();
            services.AddScoped<IPacienteService, PacienteService>();
            services.AddScoped<ICitaService, CitaService>();

            // Repositories
            services.AddScoped<IMedicoRepository, MedicoRepository>();
            services.AddScoped<IPacienteRepository, PacienteRepository>();
            services.AddScoped<ICitaRepository, CitaRepository>();

            return services;
        }

        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
