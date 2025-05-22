using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Transcription;

public record GetTranscriptionQuery : IRequest<List<TranscriptionDto>>
{
}
