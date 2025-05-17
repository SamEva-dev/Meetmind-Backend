using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Mettings;

public sealed class GetTodayMeetingsHandler : IRequestHandler<GetTodayMeetingsQuery, List<MeetingDto>>
{
    private readonly IMeetingRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodayMeetingsHandler> _logger;

    public GetTodayMeetingsHandler(IMeetingRepository repository, IMapper mapper, ILogger<GetTodayMeetingsHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<MeetingDto>> Handle(GetTodayMeetingsQuery request, CancellationToken cancellationToken)
    {
        var meetings = await _repository.GetMeetingToday(cancellationToken);
        if (meetings is null)
        {
            _logger.LogWarning("Not meeting today found");
            throw new KeyNotFoundException("Meeting not found");
        }

        return meetings;
    }
}
