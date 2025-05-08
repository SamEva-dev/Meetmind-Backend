using MediatR;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Dtos;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Queries;

public class GetTranscriptQueryHandler : IRequestHandler<GetTranscriptQuery, string>
{
    private readonly IMeetingRepository _repository;
    private readonly ILogger<GetTranscriptQueryHandler> _logger;

    public GetTranscriptQueryHandler(IMeetingRepository repository, ILogger<GetTranscriptQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> Handle(GetTranscriptQuery request, CancellationToken ct)
    {
        var meeting = await _repository.GetTranscription(request.MeetingId, ct);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {Id} not found", request.MeetingId);
            throw new KeyNotFoundException("Meeting not found");
        }

        if (meeting.TranscriptState != TranscriptState.Completed || string.IsNullOrWhiteSpace(meeting.TranscriptPath))
        {
            _logger.LogInformation("Transcript for meeting {Id} is not ready", request.MeetingId);
            throw new InvalidOperationException("Transcript not available");
        }

        if (!File.Exists(meeting.TranscriptPath))
        {
            _logger.LogError("Transcript file missing at {Path}", meeting.TranscriptPath);
            throw new FileNotFoundException("Transcript file missing");
        }

        return await File.ReadAllTextAsync(meeting.TranscriptPath, ct);
    }
}