using Meetmind.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Meetmind.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                //  cfg.AddExpressionMapping();
                // cfg.AddCollectionMappers();
                cfg.AddProfile<SettingsProfile>();
            }, new System.Reflection.Assembly[0]);

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });
            return services;
        }
    }
}
