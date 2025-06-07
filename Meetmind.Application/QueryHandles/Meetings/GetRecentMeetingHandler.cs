
using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Meetings
{
    public class GetRecentMeetingHandler : IRequestHandler<GetRecentMeetingQuery, PagedResult<MeetingDto>>
    {
        private readonly IMeetingRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetRecentMeetingHandler> _logger;
        public GetRecentMeetingHandler(IMeetingRepository repository, IMapper mapper, ILogger<GetRecentMeetingHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MeetingDto>> Handle(GetRecentMeetingQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetMeetingQuery");
            var meetings = new List<MeetingDto>();
            if (request.number == null || request.number <= 0)
            {
              var result =  await _repository.ListAsync(filter: m => !string.IsNullOrEmpty(m.AudioPath) && m.EndUtc.HasValue, tracking: false);
                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No meetings found with audio path and end time.");
                    throw new KeyNotFoundException("No meetings found with audio path and end time.");
                }
                meetings = _mapper.Map<List<MeetingDto>>(result);
                _logger.LogInformation($"Found {meetings.Count} meetings with audio path and end time.");
                return new PagedResult<MeetingDto>
                {
                    Items = meetings,
                    TotalCount = meetings.Count
                };
            }
            var resultPaged = await _repository.ListPagedAsync(
                page: 1,
                pageSize: request.number.Value,
                filter: m => !string.IsNullOrEmpty(m.AudioPath) && m.EndUtc.HasValue,
                tracking: false);
            if (resultPaged == null || !resultPaged.Items.Any())
            {
                _logger.LogWarning("No meetings found with audio path and end time.");
                throw new KeyNotFoundException("No meetings found with audio path and end time.");
            }
            
            meetings = _mapper.Map<List<MeetingDto>>(resultPaged.Items);
            _logger.LogInformation($"Found {meetings.Count} meetings with audio path and end time.");

            return new PagedResult<MeetingDto>
            {
                Items = meetings,
                PageSize = request.number.Value,
                TotalCount = resultPaged.TotalCount
            };
        }
    }
}
