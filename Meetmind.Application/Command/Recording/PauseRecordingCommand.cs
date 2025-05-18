
using MediatR;

namespace Meetmind.Application.Command.Recording;

public sealed record PauseRecordingCommand(Guid MeetingId) : IRequest<Unit>;
