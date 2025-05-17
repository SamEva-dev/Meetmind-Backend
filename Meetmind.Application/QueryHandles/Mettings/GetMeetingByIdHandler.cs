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

public sealed class GetMeetingByIdHandler : IRequestHandler<GetMeetingByIdQuery, MeetingDto?>
{
    private readonly IMeetingRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMeetingByIdHandler> _logger;

    public GetMeetingByIdHandler(IMeetingRepository repository, IMapper mapper, ILogger<GetMeetingByIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MeetingDto?> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _repository.GetMeetingById(request.Id, cancellationToken);

        if(meeting is null)
        {
            _logger.LogWarning("Meeting with id {Id} not found", request.Id);
            throw new KeyNotFoundException("Meeting not found");
        }

        return meeting;
    }
}
