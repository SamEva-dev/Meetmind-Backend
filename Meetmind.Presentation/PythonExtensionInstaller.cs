using System.Diagnostics;

namespace Meetmind.Presentation;

public static class PythonExtensionInstaller
{
    public static async Task EnsurePythonAndLibsAsync(string[] requiredLibs)
    {
        // 1. Vérifier si Python est là
        bool pythonPresent = await CheckPythonAsync();
        if (!pythonPresent)
        {
            Console.Error.WriteLine("Python is not installed or not in PATH. Please install Python 3.10+ before starting the API.");
            return;
        }

        // 2. Vérifier les packages Python requis
        foreach (var lib in requiredLibs)
        {
            Console.WriteLine($"Checking Python package...: {lib}");
            if (!await CheckPythonPackageAsync(lib))
            {
                // Essayer d'installer
                Console.WriteLine($"Installing Python package...: {lib}");
                bool installed = await InstallPythonPackageAsync(lib);
                if (!installed)
                {
                    Console.Error.WriteLine($"Failed to install Python package: {lib}");
                    PythonEnvironmentStatus.IsPythonReady = false;
                    PythonEnvironmentStatus.StatusMessage = $"Failed to install Python package: {lib}";
                    continue; 
                }
                
            }
            Console.WriteLine($"Python package...: {lib} installed OK");
        }
    }

    static async Task<bool> CheckPythonAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            bool ok = process.ExitCode == 0 && output.Contains("Python");
            if (!ok)
            {
                PythonEnvironmentStatus.IsPythonReady = false;
                PythonEnvironmentStatus.StatusMessage = "Python n'est pas installé ou n'est pas dans le PATH. Merci de l'installer (>= 3.10) avant d'utiliser la transcription.";
                // Log l'erreur (Microsoft.Extensions.Logging ou autre)
                Console.Error.WriteLine(PythonEnvironmentStatus.StatusMessage);
            }
            else
            {
                PythonEnvironmentStatus.IsPythonReady = true;
                PythonEnvironmentStatus.StatusMessage = "Python et dépendances présents.";
            }
            return ok;
        }
        catch
        {
            PythonEnvironmentStatus.IsPythonReady = false;
            PythonEnvironmentStatus.StatusMessage = "Erreur lors de la vérification de Python. Merci de l'installer (>= 3.10).";
            Console.Error.WriteLine(PythonEnvironmentStatus.StatusMessage);
            return false;
        }
    }


    static async Task<bool> CheckPythonPackageAsync(string packageName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m pip show {packageName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) && output.Contains("Name:");
        }
        catch { return false; }
    }

    static async Task<bool> InstallPythonPackageAsync(string packageName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m pip install {packageName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch { return false; }
    }

    public static class PythonEnvironmentStatus
    {
        public static bool IsPythonReady { get; set; } = false;
        public static string StatusMessage { get; set; } = "Vérification en cours...";
    }


}
