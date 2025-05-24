using MediatR;

namespace Meetmind.Application.Command.Summarize;

public record SummarizeCommand(Guid MeetingId) : IRequest<Unit>
{
}
