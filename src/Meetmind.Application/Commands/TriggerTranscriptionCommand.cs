using MediatR;

namespace Meetmind.Application.Commands;

public record TriggerTranscriptionCommand(Guid MeetingId) : IRequest<Unit>;