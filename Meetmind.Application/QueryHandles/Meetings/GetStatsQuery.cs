
using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Meetings;

public record GetStatsQuery : IRequest<GlobalStatsDto>
{
}
