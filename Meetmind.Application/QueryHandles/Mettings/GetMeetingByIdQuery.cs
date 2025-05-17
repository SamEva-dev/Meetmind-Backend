using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Mettings;

public sealed record GetMeetingByIdQuery(Guid Id) : IRequest<MeetingDto>;
