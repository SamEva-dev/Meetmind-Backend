

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Meetmind
{
    public static class FastApiWorkerManager
    {
        public static void StartAndMonitor(IConfiguration configuration, IHost host)
        {
            var workerHost = configuration["TranscriptionWorker:Host"] ?? "127.0.0.1";
            var workerPort = configuration["TranscriptionWorker:Port"] ?? "8000";
            var scriptsDir = Path.Combine(AppContext.BaseDirectory, "Scripts");
            var scriptFile = "transcribe_and_diarize.py";
            var scriptPath = Path.Combine(scriptsDir, scriptFile);

            if (!File.Exists(scriptPath))
            {
                Log.Error("TranscribeWorker script not found at path: {Path}", scriptPath);
                throw new FileNotFoundException($"Le script Python est introuvable : {scriptPath}");
            }

            var moduleName = Path.GetFileNameWithoutExtension(scriptFile);
            KillOldFastApiProcesses(moduleName);


            var arguments = $"-m uvicorn {moduleName}:app --host {workerHost} --port {workerPort}";

            CancellationTokenSource cts = new();
            Process? fastApiProcess = null; // Pour kill à l'arrêt

            // Redémarrage auto avec backoff si le worker crash (max 5 essais consécutifs en 2min)
            Task.Run(async () =>
            {
                int restartCount = 0;
                DateTime? firstRestart = null;

                while (!cts.Token.IsCancellationRequested)
                {
                    fastApiProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = arguments,
                            WorkingDirectory = scriptsDir,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        },
                        EnableRaisingEvents = true
                    };

                    fastApiProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrWhiteSpace(args.Data))
                            Log.Information("[TranscribeWorker STDOUT] {Message}", args.Data);
                    };
                    fastApiProcess.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrWhiteSpace(args.Data))
                            Log.Error("[TranscribeWorker STDERR] {Message}", args.Data);
                    };

                    Log.Information("Launching Python FastAPI worker: {FileName} {Args}", fastApiProcess.StartInfo.FileName, fastApiProcess.StartInfo.Arguments);

                    bool started = fastApiProcess.Start();
                    if (!started)
                    {
                        Log.Error("Failed to start TranscribeWorker process.");
                        break;
                    }

                    fastApiProcess.BeginOutputReadLine();
                    fastApiProcess.BeginErrorReadLine();

                    // Healthcheck FastAPI au boot
                    bool isHealthy = false;
                    using var httpClient = new HttpClient();
                    string fastApiUrl = $"http://{workerHost}:{workerPort}/docs";
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(1000, cts.Token);
                        try
                        {
                            var resp = await httpClient.GetAsync(fastApiUrl, cts.Token);
                            if (resp.IsSuccessStatusCode)
                            {
                                isHealthy = true;
                                Log.Information("TranscribeWorker is up and running! ({Url})", fastApiUrl);
                                break;
                            }
                        }
                        catch { /* Ignorer tant que la connexion n'est pas établie */ }
                    }
                    if (!isHealthy)
                        Log.Warning("TranscribeWorker FastAPI endpoint not responding after 10s. Check: {Url}", fastApiUrl);

                    // Attente de l’arrêt ou du crash du process
                    await Task.Run(() => fastApiProcess.WaitForExit(), cts.Token);

                    if (cts.Token.IsCancellationRequested)
                    {
                        Log.Information("TranscribeWorker monitor stopping (API shutdown)...");
                        break;
                    }

                    if (fastApiProcess.ExitCode == 0)
                    {
                        Log.Information("TranscribeWorker exited cleanly.");
                        break; // arrêt normal = pas de redémarrage
                    }
                    else
                    {
                        // Crash détecté !
                        Log.Error("TranscribeWorker crashed with code {ExitCode}", fastApiProcess.ExitCode);

                        if (firstRestart == null)
                            firstRestart = DateTime.UtcNow;
                        restartCount++;

                        // Protection contre boucle infinie de crash
                        if (restartCount >= 5 && (DateTime.UtcNow - firstRestart) < TimeSpan.FromMinutes(2))
                        {
                            Log.Error("Too many worker restarts in a short period. Not restarting anymore. Please check your FastAPI worker.");
                            break;
                        }

                        Log.Warning("Restarting TranscribeWorker in 5s (attempt {Attempt})...", restartCount);
                        await Task.Delay(5000, cts.Token);
                        continue;
                    }
                }
            });

            // Gestion arrêt API : on annule le monitoring et on kill le worker proprement
            host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
            {
                try
                {
                    Log.Information("Application stopping: requesting worker shutdown...");
                    cts.Cancel();

                    if (fastApiProcess != null && !fastApiProcess.HasExited)
                    {
                        fastApiProcess.Kill(entireProcessTree: true);
                        if (!fastApiProcess.WaitForExit(3000))
                        {
                            Log.Warning("TranscribeWorker did not exit within timeout, killed forcefully.");
                        }
                        else
                        {
                            Log.Information("TranscribeWorker stopped.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error stopping transcription worker monitor");
                }
            });
        }

        public static void KillOldFastApiProcesses(string scriptFile)
        {
            try
            {
                Log.Information("Checking for old Python FastAPI processes...");

                // Récupère tous les process "python" ou "python.exe"
                var pythonProcesses = Process.GetProcesses()
                                                .Where(p => p.ProcessName.StartsWith("python", StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                foreach (var proc in pythonProcesses)
                {
                    try
                    {
                        // Vérifie la ligne de commande (disponible sous Windows 10+/Linux avec droits admin)
                        string commandLine = GetCommandLine(proc);

                        if (!string.IsNullOrEmpty(commandLine) && commandLine.Contains(scriptFile))
                        {
                            Log.Warning($"Killing old FastAPI process (PID={proc.Id}): {commandLine}");
                            proc.Kill(entireProcessTree: true);
                            proc.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Failed to kill process {proc.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in KillOldFastApiProcesses");
            }
        }

        private static string GetCommandLine(Process process)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var searcher = new System.Management.ManagementObjectSearcher(
                        $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
                    {
                        foreach (var @object in searcher.Get())
                        {
                            return @object["CommandLine"]?.ToString() ?? "";
                        }
                    }
                }
                else // Linux/Mac
                {
                    string cmdlinePath = $"/proc/{process.Id}/cmdline";
                    if (File.Exists(cmdlinePath))
                    {
                        return File.ReadAllText(cmdlinePath);
                    }
                }
            }
            catch
            {
                // ignore
            }
            return "";
        }
    }
}
