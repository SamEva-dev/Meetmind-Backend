using MediatR;

namespace Meetmind.Application.Commands;

public record StartMeetingCommand(Guid MeetingId) : IRequest<Unit>;

