using AutoMapper;
using MediatR;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Recording;

public sealed class GetRecordingStatusHandler : IRequestHandler<GetRecordingStatusQuery, string>
{
    private readonly IMeetingRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRecordingStatusHandler> _logger;

    public GetRecordingStatusHandler(IMeetingRepository repo, IMapper mapper, ILogger<GetRecordingStatusHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<string> Handle(GetRecordingStatusQuery request, CancellationToken ct)
    {
        var meeting = await _repo.GetMeetingById(request.MeetingId, ct);
        if (meeting is null)
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");

        return meeting.State.ToString();
    }
}
