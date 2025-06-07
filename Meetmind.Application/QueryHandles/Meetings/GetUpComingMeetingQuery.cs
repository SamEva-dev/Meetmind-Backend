
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Enums;

namespace Meetmind.Application.QueryHandles.Meetings;

public record GetUpComingMeetingQuery(int? number) : IRequest<PagedResult<MeetingDto>>
{
}
