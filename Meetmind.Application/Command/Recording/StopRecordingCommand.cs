using MediatR;

namespace Meetmind.Application.Command.Recording;

public sealed record StopRecordingCommand(Guid MeetingId, DateTime EndTime) : IRequest<Unit>;
