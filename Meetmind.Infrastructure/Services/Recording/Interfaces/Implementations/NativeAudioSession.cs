using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces.Implementations;

internal sealed class NativeAudioSession : IAudioSession
{
    private readonly IAudioFileStorage _storage;
    private readonly IAudioEventLogger _eventLogger;
    private readonly ILogger<NativeAudioSession> _logger;
    private readonly List<string> _fragments = new();
    private readonly WaveInEvent _waveIn;
    private WaveFileWriter _writer;
    private bool _disposed;

    public Guid MeetingId { get; }

    public NativeAudioSession(Guid meetingId,
                               string firstFileName,
                               IWaveInFactory waveFactory,
                               IAudioFileStorage storage,
                               IAudioEventLogger eventLogger,
                               ILogger<NativeAudioSession> logger)
    {
        MeetingId = meetingId;
        _storage = storage;
        _eventLogger = eventLogger;
        _logger = logger;

        _waveIn = waveFactory.Create();
        _writer = CreateWriter(firstFileName);
        _waveIn.DataAvailable += (_, a) => { _writer.Write(a.Buffer, 0, a.BytesRecorded); };
        _waveIn.RecordingStopped += (_, __) => _writer?.Dispose();
        _waveIn.StartRecording();
        _logger.LogInformation("Recording started for meeting {Id}", meetingId);
    }

    private WaveFileWriter CreateWriter(string filePath)
    {
        var writer = new WaveFileWriter(filePath, _waveIn.WaveFormat);
        _fragments.Add(filePath);
        return writer;
    }

    public async Task PauseAsync(CancellationToken ct)
    {
        _waveIn.StopRecording();
        await _eventLogger.LogAsync(new AudioEventLog
        {
            MeetingId = MeetingId,
            Action = "Pause",
            UtcTimestamp = DateTime.UtcNow
        }, ct);
    }

    public async Task ResumeAsync(CancellationToken ct)
    {
        var nextPath = _storage.GetNewFragmentPath("resume", MeetingId);
        _writer = CreateWriter(nextPath);
        _waveIn.StartRecording();
        await _eventLogger.LogAsync(new AudioEventLog
        {
            MeetingId = MeetingId,
            Action = "Resume",
            UtcTimestamp = DateTime.UtcNow
        }, ct);
    }

    public async Task<string> StopAsync(CancellationToken ct)
    {
        _waveIn.StopRecording();
        var output = await _storage.ConcatenateAsync(_fragments, ct);
        _storage.Delete(_fragments);

        await _eventLogger.LogAsync(new AudioEventLog
        {
            MeetingId = MeetingId,
            Action = "Stop",
            Details = $"Fragments={_fragments.Count}",
            UtcTimestamp = DateTime.UtcNow
        }, ct);
        return output;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _waveIn?.Dispose();
        _writer?.Dispose();
        await Task.CompletedTask;
    }
}