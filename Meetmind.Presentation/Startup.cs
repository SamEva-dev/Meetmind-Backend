using System.Text.Json.Serialization;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Meetmind.Presentation.Extensions;
using Meetmind.Presentation.Hubs;
using static Meetmind.Presentation.PythonExtensionInstaller;
using System.Diagnostics;

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
            // Charger settings worker
            var workerHost = Configuration["TranscriptionWorker:Host"] ?? "127.0.0.1";
            var workerPort = Configuration["TranscriptionWorker:Port"] ?? "8000";

            var workerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"transcribe_worker.py", // chemin absolu si besoin
                    WorkingDirectory = "Scripts", // Dossier où est le script et logger_config.py
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    // Ici tu peux passer des ENV si tu veux
                    // Environment = { ["WHISPER_MODEL"] = "...", ... }
                }
            };
            workerProcess.Start();

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

            services.AddEndpointsApiExplorer();

          

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins("http://localhost:4200")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithOrigins()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
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
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
            //});

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

            //app.UseAuthentication();
           // app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SettingsHub>("/hubs/settings");
                endpoints.MapHub<MeetingHub>("/hubs/meetings");
                endpoints.MapHub<RecordingHub>("/hubs/recording");
                endpoints.MapHub<AudioHub>("/hubs/audio");
                endpoints.MapHub<TranscriptHub>("/hubs/transcipt");
                endpoints.MapHub<SummaryHub>("/hubs/summary");
                endpoints.MapHub<NotificationHub>("/hubs/Notification");

                endpoints.MapGet("/status/python", () =>
                {
                    return Results.Ok(new
                    {
                        ready = PythonEnvironmentStatus.IsPythonReady,
                        message = PythonEnvironmentStatus.StatusMessage
                    });
                });
            });
            // #endif
        }
    }
}