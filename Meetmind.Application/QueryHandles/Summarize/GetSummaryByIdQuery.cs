using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Summarize;

public record GetSummaryByIdQuery(Guid MeetingId) : IRequest<SummarizeDto>
{
}
