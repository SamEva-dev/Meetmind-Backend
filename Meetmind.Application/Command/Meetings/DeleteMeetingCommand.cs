using MediatR;

namespace Meetmind.Application.Command.Meetings;

public record DeleteMeetingCommand(Guid MeetingId) : IRequest<Unit>;
