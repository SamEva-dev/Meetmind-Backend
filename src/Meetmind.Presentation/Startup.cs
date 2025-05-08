using System.Text.Json.Serialization;
using Asp.Versioning;
using System.Text.Json;
using Asp.Versioning.ApiExplorer;
using Meetmind.Presentation.Extensions;

namespace Meetmind.Presentation
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });
            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                //  cfg.AddExpressionMapping();
                // cfg.AddCollectionMappers();
                //cfg.AddProfile<PresentationMappingProfile>();
            }, new System.Reflection.Assembly[0]);
            services.AddEndpointsApiExplorer();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<Startup>();
                //cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly);
                //cfg.RegisterServicesFromAssembly(typeof(GetMeetingsTodayQuery).Assembly);
            });


            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader());
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                //options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter()); // no native solution with System.Text.Json.Serialization;
            });
            services.AddSignalR();
            services.AddSwaggerDocumentation();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
            });

            //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, IApiVersionDescriptionProvider provider)
        {

            app.UseCors("CorsPolicy");

            // #if DEBUG
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            //A common endpoint that contains both versions
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"{description.GroupName}/swagger.yaml", $"MeetMind Service API {description.GroupName}");
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // #endif
        }
    }
}
