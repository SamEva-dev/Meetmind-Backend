using MediatR;

namespace Meetmind.Application.Command.Audio;

public record DeleteAudioCommand(Guid MeetingId) : IRequest<Unit>
{
}
