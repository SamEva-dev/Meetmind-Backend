using Meetmind.Infrastructure;
using Meetmind.Presentation;
using Meetmind.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTracing;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Meetmind;
using System.Diagnostics;

var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true);

IConfiguration configuration = configurationBuilder.Build();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .WriteTo.Console()
    .ReadFrom.Configuration(configuration)
    .Filter.ByExcluding(evt =>
        evt.Properties.ContainsKey("transcriptPath") ||
        evt.Properties.ContainsKey("audioPath") ||
        evt.Properties.ContainsKey("summaryPath")
    )
    .CreateLogger();

using var listener = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .TraceToSharedLogger();
if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");


Log.Information("*** STARTUP ***");
var requiredPythonLibs = new[] { "torch", "faster-whisper", "pyannote.audio", "sentencepiece", "python-multipart" };

await PythonExtensionInstaller.EnsurePythonAndLibsAsync(requiredPythonLibs);


var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(configuration!);
                if (configuration == null)
                {
                    throw new InvalidOperationException("Configuration not initialized");
                }
                services.AddApplication();
                services.AddInfrastructure(configuration);
            })
            .ConfigureLogging(logger =>
            {
                logger.ClearProviders();
                logger.AddConsole();
                logger.AddSerilog(dispose: true);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(5001, listenOptions =>
                    {
                        listenOptions.UseHttps(); 
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });

                    options.ListenAnyIP(5000, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                });
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog()
            .Build();

RunMigration(host);

RunTranscriptionFastApi(configuration, host);

host.Run();

static void RunTranscriptionFastApi(IConfiguration configuration, IHost host)
{
    var workerHost = configuration["TranscriptionWorker:Host"] ?? "127.0.0.1";
    var workerPort = configuration["TranscriptionWorker:Port"] ?? "8000";

    var workerProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"transcribe_and_diarize:app --host {workerHost} --port {workerPort}", // chemin absolu si besoin
            WorkingDirectory = "Scripts", // chemin où se trouve le script Python
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    Log.Information("Start process: " + workerProcess.StartInfo.FileName + " " + workerProcess.StartInfo.Arguments);
    workerProcess.Start();
    Task.Run(async () =>
    {
        while (!workerProcess.StandardOutput.EndOfStream)
        {
            var line = await workerProcess.StandardOutput.ReadLineAsync();
            Log.Information("[PYTHON] " + line);
        }
    });

    // 3. Arrêt propre du worker
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
    {
        try { 
            Log.Information("Stopping transcription worker...");
            if (!workerProcess.HasExited) workerProcess.Kill(); 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error stopping transcription worker");
        }
    });
}

static void RunMigration(IHost host)
{
    using (var scope = host.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
            Console.WriteLine("Applying migrations...");
            context.Database.Migrate();

            var pendingMigrations = context.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Current database migration version: {pendingMigrations.Last()}");
                Console.WriteLine("Migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }
}