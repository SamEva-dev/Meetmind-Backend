using AutoMapper;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;
using Microsoft.Graph.Models.CallRecords;

namespace Meetmind.Infrastructure.Mapping;
public class TranscriptionProfile : Profile
{
    public TranscriptionProfile()
    {
        CreateMap<TranscriptionEntity, TranscriptionDto>()
            .ReverseMap();
        CreateMap<TranscriptionSegment, SegmentDto>()
            .ReverseMap();
    }
}
