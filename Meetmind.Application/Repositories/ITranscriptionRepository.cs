
using Meetmind.Application.Dto;

namespace Meetmind.Application.Repositories;

public interface ITranscriptionRepository
{
    Task DeleteTransition(TranscriptionDto transcription, CancellationToken cancellationToken);
    Task AddTransition(TranscriptionDto transcription, CancellationToken cancellationToken);
    Task<List<TranscriptionDto>> GetTranscriptionAsync(CancellationToken cancellationToken);
    Task<TranscriptionDto> GetTranscriptionByIdAsync(Guid meetingId, CancellationToken cancellationToken);
}
