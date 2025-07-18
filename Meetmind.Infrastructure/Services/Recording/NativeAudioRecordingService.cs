
using Meetmind.Application.Services.AudioRecorder;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Services.Recording.Interfaces;
using NAudio.CoreAudioApi;


namespace Meetmind.Infrastructure.Services.Recording;

public sealed class NativeAudioRecordingService : IAudioRecordingService
{
    private readonly IAudioSessionManager _manager;
    public NativeAudioRecordingService(IAudioSessionManager manager) => _manager = manager;

    public AudioRecordingType BackendType => AudioRecordingType.Native;

    public Task StartAsync(Guid meetingId, string fileName, CancellationToken ct) => _manager.StartAsync(meetingId, fileName, ct);
    public Task PauseAsync(Guid meetingId, CancellationToken ct) => _manager.PauseAsync(meetingId, ct);
    public Task ResumeAsync(Guid meetingId, CancellationToken ct) => _manager.ResumeAsync(meetingId, ct);
    public Task<string> StopAsync(Guid meetingId, CancellationToken ct) => _manager.StopAsync(meetingId, ct);
}