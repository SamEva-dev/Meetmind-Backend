using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.Services;

public interface ITranscriptionService
{
    public TranscriptionType BackendType{ get; }

    Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct);
}