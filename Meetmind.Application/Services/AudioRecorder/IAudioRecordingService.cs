using Meetmind.Domain.Enums;

namespace Meetmind.Application.Services.AudioRecorder;

public interface IAudioRecordingService
{
    AudioRecordingType BackendType { get; }
    Task StartAsync(Guid meetingId, string title, CancellationToken ct);
    Task<string> StopAsync(Guid meetingId, CancellationToken ct);
    Task PauseAsync(Guid meetingId, CancellationToken ct);
    Task ResumeAsync(Guid meetingId, CancellationToken ct);
}
