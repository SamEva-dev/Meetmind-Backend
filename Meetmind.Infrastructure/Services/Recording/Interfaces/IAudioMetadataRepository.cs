using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Services.Recording.Interfaces;

public interface IAudioMetadataRepository
{
    Task AddAsync(AudioMetadata meta, CancellationToken ct);
    Task<AudioMetadata?> GetIncompleteAsync(Guid meetingId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}