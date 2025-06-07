using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Meetings;

public class GetStatsHandler : IRequestHandler<GetStatsQuery, GlobalStatsDto>
{
    private readonly ILogger<GetStatsHandler> _logger;
    private readonly IMeetingRepository _repository;
    public GetStatsHandler(IMeetingRepository repository, ILogger<GetStatsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<GlobalStatsDto> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des statistiques globales");
        var meetings = await _repository.GetMeetingToday(cancellationToken);
        if (meetings == null)
        {
            _logger.LogWarning("Aucune statistique trouvée");
            throw new KeyNotFoundException("Aucune statistique trouvée");
        }

        var meetingsCount = meetings.Count;

        var totalSeconds = meetings
            .Where(m => m.Duration.HasValue)
            .Sum(m => m.Duration.Value.TotalSeconds);

        var totalHours = Math.Round(totalSeconds / 3600.0, 2);

        var transcriptions = meetings.Count(m => m.TranscriptState == "Completed");

        var summaries = meetings.Count(m => m.SummaryState == "Completed");

        var stats = new GlobalStatsDto
        {
            MeetingsCount = meetingsCount,
            TotalDuration = totalHours,
            TranscriptionsCount = transcriptions,
            SummariesCount = summaries
        };

        return stats;
    }
}

