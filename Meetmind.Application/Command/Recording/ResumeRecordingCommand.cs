
using MediatR;

namespace Meetmind.Application.Command.Recording;

public sealed record ResumeRecordingCommand(Guid MeetingId) : IRequest<Unit>;
