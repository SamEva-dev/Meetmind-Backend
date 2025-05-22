using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Transcription;

public record GetTranscriptionByIdQuery(Guid meetingId) : IRequest<TranscriptionDto>
{
}
