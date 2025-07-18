
namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IAudioSessionManager
{
    Task StartAsync(Guid meetingId, string fileName, CancellationToken ct);
    Task PauseAsync(Guid meetingId, CancellationToken ct);
    Task ResumeAsync(Guid meetingId, CancellationToken ct);
    Task<string> StopAsync(Guid meetingId, CancellationToken ct);
}