
using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Mapping;

internal class CalendarSyncLogProfile : Profile
{
    public CalendarSyncLogProfile()
    {
        CreateMap<CalendarSyncLog, CalendarSyncLogDto>();

        CreateMap<CalendarSyncLogDto, CalendarSyncLog>();
    }
}
