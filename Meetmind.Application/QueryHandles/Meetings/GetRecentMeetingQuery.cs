
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;

namespace Meetmind.Application.QueryHandles.Meetings;

public record GetRecentMeetingQuery(int? number) : IRequest<PagedResult<MeetingDto>>
{
}
