using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IAudioEventLogger
{
    Task LogAsync(AudioEventLog log, CancellationToken ct);
}