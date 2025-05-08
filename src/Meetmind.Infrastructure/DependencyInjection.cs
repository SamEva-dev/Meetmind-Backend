using Meetmind.Application.Common.Interfaces;
using Meetmind.Infrastructure.Auth;
using Meetmind.Infrastructure.Db;
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
            services.AddHttpContextAccessor(); // 🔁 scoped
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Auth JWT
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = config["Authentication:Authority"];
                    options.Audience = config["Authentication:Audience"];
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
            });

            var dbPath = config.GetConnectionString("Sqlite") ?? "Data/meetmind.db";
            services.AddDbContext<MeetMindDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();


            return services;
        }
    }
}
