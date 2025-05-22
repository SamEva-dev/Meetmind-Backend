
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Repositories;

public interface IAudioFragmentRepository
{
    Task AddFragment(AudioMetadata fragment, CancellationToken cancellationToken);
    Task DeleteAsync(AudioMetadata fragment, CancellationToken cancellationToken);
    Task<AudioMetadata> GetFragmentIdAsync(Guid meetingId, CancellationToken cancellationToken);
}
