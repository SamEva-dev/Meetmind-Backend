
using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.Command.Transcription
{
    public record DeleteTranscriptionCommand(Guid MeetingId) : IRequest<Unit>
    {
    }
}
