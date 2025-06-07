using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Meetings;

public class GetUpComingMeetingHandler : IRequestHandler<GetUpComingMeetingQuery, PagedResult<MeetingDto>>
{
    private readonly IMeetingRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUpComingMeetingHandler> _logger;
    public GetUpComingMeetingHandler(IMeetingRepository repository, IMapper mapper, ILogger<GetUpComingMeetingHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<MeetingDto>> Handle(GetUpComingMeetingQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetBecomeMeetingQuery");

        if (request.number == null || request.number <= 0)
        {
            var result = await _repository.ListAsync(m => 
            m.State == MeetingState.Pending &&
            m.Start > DateTime.Now, false);
            if (result == null || !result.Any())
            {
                _logger.LogWarning("No meetings found ");
                throw new KeyNotFoundException("No meetings found .");
            }

            var meeting = _mapper.Map<List<MeetingDto>>(result);

            return new PagedResult<MeetingDto>
            {
                Items = meeting,
                TotalCount = meeting.Count
            };
        }

        var resultPaged = await _repository.ListPagedAsync(
                page: 1,
                pageSize: request.number.Value,
                filter: m => m.State == MeetingState.Pending && m.Start > DateTime.Now,
                tracking: false);
        if (resultPaged == null || !resultPaged.Items.Any())
        {
            _logger.LogWarning("No meetings found with audio path and end time.");
            throw new KeyNotFoundException("No meetings found with audio path and end time.");
        }

       var  meetings = _mapper.Map<List<MeetingDto>>(resultPaged.Items);
        _logger.LogInformation($"Found {meetings.Count} meetings with audio path and end time.");

        return new PagedResult<MeetingDto>
        {
            Items = meetings,
            PageSize = request.number.Value,
            TotalCount = resultPaged.TotalCount
        };
    }
}
