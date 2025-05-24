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
using Serilog.Core;

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
var requiredPythonLibs = new[] { "torch", 
                                "faster-whisper", 
                                "pyannote.audio", 
                                "sentencepiece", 
                                "python-multipart", 
                                "pydantic",
                                "langdetect"};

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

    string scriptPath = Path.GetFullPath("Scripts/transcribe_and_diarize.py");

    if (!File.Exists(scriptPath))
    {
        Log.Error("TranscribeWorker script not found at path: {Path}", scriptPath);
        // Tu peux aussi lever une exception ou retourner selon ton besoin :
        throw new FileNotFoundException($"Le script Python transcribe_worker.py est introuvable à l’emplacement {scriptPath}");
    }


    var psi = new ProcessStartInfo
    {
        FileName = "python",
        Arguments = $"-m uvicorn {scriptPath.Replace(".py", "")}:app --host {workerHost} --port {workerPort}",
        WorkingDirectory = "Scripts",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    
    Log.Information("Start process: " + psi.FileName + " " + psi.Arguments);
    var process = new Process { StartInfo = psi };
    process.OutputDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrWhiteSpace(args.Data))
            Log.Information("[TranscribeWorker STDOUT] {Message}", args.Data);
    };
    process.ErrorDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrWhiteSpace(args.Data))
            Log.Error("[TranscribeWorker STDERR] {Message}", args.Data);
    };

    if (process.Start())
    {
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        Log.Information("TranscribeWorker process started successfully: {FileName} (PID: {PID})", psi.FileName, process.Id);
    }
    else
    {
        Log.Error("Failed to start TranscribeWorker process: {FileName}", psi.FileName);
    }
    Task.Run(async () =>
    {
        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            Log.Information("[PYTHON] " + line);
        }
    });

    // 3. Arrêt propre du worker
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
    {
        try { 
            Log.Information("Stopping transcription worker...");
            if (!process.HasExited) process.Kill(); 
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