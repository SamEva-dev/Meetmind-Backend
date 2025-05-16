using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Hubs;
using Meetmind.Infrastructure.Mapping;
using Meetmind.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Meetmind.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var dbPath = config.GetConnectionString("Sqlite") ?? "Data/meetmind.db";
            services.AddDbContext<MeetMindDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                //  cfg.AddExpressionMapping();
                // cfg.AddCollectionMappers();
                cfg.AddProfile<SettingsProfile>();
            }, new System.Reflection.Assembly[0]);

            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<INotificationService, SignalRNotificationService>();

            return services;
        }
    }
}
