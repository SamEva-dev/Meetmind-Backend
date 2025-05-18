
using MediatR;

namespace Meetmind.Application.Command.Recording;

public sealed record StartRecordingCommand(Guid MeetingId) : IRequest<Unit>;

