using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Mettings;

public sealed record GetTodayMeetingsQuery() : IRequest<List<MeetingDto>>;
