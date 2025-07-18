using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces.Implementations;

internal sealed class AudioSessionManager : IAudioSessionManager
{
    private readonly ConcurrentDictionary<Guid, IAudioSession> _sessions = new();
    private readonly IServiceProvider _sp; // small Service‑Locator for per‑session dependencies

    public AudioSessionManager(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(Guid meetingId, string fileName, CancellationToken ct)
    {
        if (_sessions.ContainsKey(meetingId))
            throw new InvalidOperationException("Session already exists");

        var session = CreateSession(meetingId, fileName);
        if (!_sessions.TryAdd(meetingId, session))
            await session.DisposeAsync();
    }

    public Task PauseAsync(Guid meetingId, CancellationToken ct) => ExecuteAsync(meetingId, s => s.PauseAsync(ct));
    public Task ResumeAsync(Guid meetingId, CancellationToken ct) => ExecuteAsync(meetingId, s => s.ResumeAsync(ct));
    public async Task<string> StopAsync(Guid meetingId, CancellationToken ct)
    {
        if (!_sessions.TryRemove(meetingId, out var session))
            throw new InvalidOperationException("No active session");
        var path = await session.StopAsync(ct);
        await session.DisposeAsync();
        return path;
    }

    private IAudioSession CreateSession(Guid meetingId, string fileName)
    {
        // Resolve scoped dependencies manually (factory pattern)
        var waveFactory = _sp.GetRequiredService<IWaveInFactory>();
        var storage = _sp.GetRequiredService<IAudioFileStorage>();
        var eventLogger = _sp.GetRequiredService<IAudioEventLogger>();
        var logger = _sp.GetRequiredService<ILogger<NativeAudioSession>>();

        return new NativeAudioSession(meetingId, fileName, waveFactory, storage, eventLogger, logger);
    }

    private async Task ExecuteAsync(Guid meetingId, Func<IAudioSession, Task> action)
    {
        if (!_sessions.TryGetValue(meetingId, out var session))
            throw new InvalidOperationException("No active session");
        await action(session);
    }
}