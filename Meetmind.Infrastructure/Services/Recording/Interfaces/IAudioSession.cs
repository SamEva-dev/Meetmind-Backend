
namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IAudioSession : IAsyncDisposable
{
    Guid MeetingId { get; }
    Task PauseAsync(CancellationToken ct);
    Task ResumeAsync(CancellationToken ct);
    Task<string> StopAsync(CancellationToken ct);
}