
using MediatR;
using Meetmind.Application.Dto;

namespace Meetmind.Application.QueryHandles.Settings;

public record GetSettingsQuery() : IRequest<SettingsDto>
{
}
