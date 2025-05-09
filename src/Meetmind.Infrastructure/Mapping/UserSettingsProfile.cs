using System.Text.Json;
using AutoMapper;
using Meetmind.Application.Dtos;
using Meetmind.Domain.Entities;
using Meetmind.Domain.ValueObjects;

namespace Meetmind.Infrastructure.Mapping;

public class UserSettingsProfile : Profile
{
    public UserSettingsProfile()
    {
        CreateMap<UserSettingsEntity, UserSettings>()
            .ForMember(dest => dest.NotifyBeforeMinutes,
                opt => opt.MapFrom(src =>
                    JsonSerializer.Deserialize<List<int>>(src.NotifyBeforeMinutesJson, new JsonSerializerOptions()) ?? new List<int> { 10, 5, 1 }));

        CreateMap<UserSettingsDto, UserSettingsEntity>()
            .ForMember(dest => dest.NotifyBeforeMinutesJson,
                opt => opt.MapFrom(src => JsonSerializer.Serialize(src.NotifyBeforeMinutes, (JsonSerializerOptions?)null)));
    }
}