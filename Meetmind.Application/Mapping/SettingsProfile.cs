using System.Text.Json;
using AutoMapper;
using Meetmind.Application.Command.Settings;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Mapping;

public class SettingsProfile : Profile
{
    public SettingsProfile()
    {
        CreateMap<SettingsCommand, SettingsDto>();

        CreateMap<SettingsCommand, SettingsEntity>();
    }
}