using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Mapping;

public class MeetingProfile : Profile
{
    public MeetingProfile()
    {
        CreateMap<MeetingEntity, MeetingDto>();

        CreateMap<MeetingDto, MeetingEntity>();
    }
}
