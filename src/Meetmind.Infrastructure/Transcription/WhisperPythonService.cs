using System.Diagnostics;
using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Transcription;

public class WhisperPythonService : IWhisperService
{
    private readonly ILogger<WhisperPythonService> _logger;

    public WhisperPythonService(ILogger<WhisperPythonService> logger)
    {
        _logger = logger;
    }

    public async Task<string> TranscribeAsync(Guid meetingId, CancellationToken ct)
    {
        var audioPath = Path.Combine("Data", "Audio", $"{meetingId}.wav");
        var outputPath = Path.Combine("Data", "Transcript", $"{meetingId}.txt");

        if (!File.Exists(audioPath))
        {
            _logger.LogError("Audio file not found: {Path}", audioPath);
            throw new FileNotFoundException("Audio file not found", audioPath);
        }

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"transcribe.py \"{audioPath}\" \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogInformation("Running Whisper transcription: {Command}", psi.Arguments);

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start transcription process.");
        }

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Whisper process failed. ExitCode={Code} Error={Error}", process.ExitCode, stderr);
            throw new ApplicationException("Whisper failed: " + stderr);
        }

        

        _logger.LogInformation("Transcription completed. Output file: {Output}", outputPath);
        return outputPath;
    }
}