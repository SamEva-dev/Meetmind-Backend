using MediatR;

namespace Meetmind.Application.Command.Transcription;

public sealed record TranscriptionCommand(Guid MeetingId) : IRequest<Unit>;
