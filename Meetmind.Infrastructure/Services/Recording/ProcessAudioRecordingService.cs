using System.Collections.Concurrent;
using System.Diagnostics;
using Meetmind.Application.Helper;
using Meetmind.Application.Services;
using Meetmind.Domain.Enums;

namespace Meetmind.Infrastructure.Services.Recording;

public class ProcessAudioRecordingService : IAudioRecordingService
{
    private static readonly ConcurrentDictionary<Guid, List<string>> _audioFragments = new();
    private static readonly ConcurrentDictionary<Guid, Process> _recordingProcesses = new();

    public AudioRecordingType BackendType => AudioRecordingType.Process;

    public async Task StartAsync(Guid meetingId, string filePath, CancellationToken ct)
    {
        _audioFragments.TryAdd(meetingId, new List<string>());
        await StartFragment(meetingId, filePath, ct);
    }

    private Task StartFragment(Guid meetingId, string title, CancellationToken ct)
    {
        var audioPath = AudioFileHelper.GenerateAudioPath(title, meetingId);

        if (!_audioFragments.TryGetValue(meetingId, out var fragments))
            throw new InvalidOperationException("Session non initialisée");

        var directory = Path.GetDirectoryName(audioPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        fragments.Add(audioPath);

        var ffmpegCmd = $"-f dshow -i audio=\"Microphone (Realtek Audio)\" -acodec pcm_s16le -ar 16000 -ac 1 \"{audioPath}\" -y";
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = ffmpegCmd,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        if (!_recordingProcesses.TryAdd(meetingId, proc))
            throw new InvalidOperationException("Un enregistrement est déjà en cours pour ce meeting.");

        proc.Start();
        return Task.CompletedTask;
    }

    public Task PauseAsync(Guid meetingId, CancellationToken ct)
    {
        if (!_recordingProcesses.TryRemove(meetingId, out var proc))
            throw new InvalidOperationException("Aucun enregistrement actif à mettre en pause.");
        proc.Kill(entireProcessTree: true);
        return Task.CompletedTask;
    }

    public Task ResumeAsync(Guid meetingId, CancellationToken ct)
    {
        if (!_audioFragments.ContainsKey(meetingId))
            throw new InvalidOperationException("Session non initialisée.");
        var fragments = _audioFragments[meetingId];
        var pathBase = Path.GetDirectoryName(fragments.First()) ?? "Resources";
        var fileBase = Path.GetFileNameWithoutExtension(fragments.First()) ?? $"meeting_{meetingId}";
        var newFilePath = Path.Combine(pathBase, $"{fileBase}_{DateTime.UtcNow:HHmmssfff}.wav");
        return StartFragment(meetingId, newFilePath, ct);
    }

    public async Task<string> StopAsync(Guid meetingId, CancellationToken ct)
    {
        if (_recordingProcesses.TryRemove(meetingId, out var proc))
        {
            try { proc.Kill(entireProcessTree: true); } catch { }
        }
        if (!_audioFragments.TryRemove(meetingId, out var fragments))
            throw new InvalidOperationException("Aucune session à terminer.");
        var outputFile = Path.Combine("Resources", $"meeting_{meetingId}_final.wav");
        await ConcatWaveFilesAsync(fragments, outputFile);
        foreach (var frag in fragments) try { File.Delete(frag); } catch { }
        return outputFile;
    }

    private async Task ConcatWaveFilesAsync(List<string> inputFiles, string outputFile)
    {
        using var writer = new NAudio.Wave.WaveFileWriter(outputFile, new NAudio.Wave.WaveFormat(16000, 1));
        foreach (var file in inputFiles)
        {
            using var reader = new NAudio.Wave.WaveFileReader(file);
            var buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                await writer.WriteAsync(buffer, 0, bytesRead);
        }
        writer.Flush();
    }
}