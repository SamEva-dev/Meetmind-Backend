using System.Text.Json;
using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Infrastructure.Mapping;

public class SettingsProfile : Profile
{
    public SettingsProfile()
    {
        CreateMap<SettingsEntity, SettingsDto>()
            .ReverseMap();
    }
}