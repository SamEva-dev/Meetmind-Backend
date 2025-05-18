

using MediatR;

namespace Meetmind.Application.Command.Meetings;

public sealed record CancelMeetingCommand(Guid MeetingId) : IRequest<Unit>;