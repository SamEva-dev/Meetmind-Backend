
using MediatR;
using Meetmind.Application.Command.Summarize;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.QueryHandles.Summarize;

public class GetSummaryByIdHandler : IRequestHandler<GetSummaryByIdQuery, SummarizeDto>
{
    private readonly IMeetingRepository _repo;
    private readonly ILogger<GetSummaryByIdHandler> _logger;

    public GetSummaryByIdHandler(IMeetingRepository repo, ILogger<GetSummaryByIdHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<SummarizeDto> Handle(GetSummaryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetSummaryByIdQuery for MeetingId: {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting with ID {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        if (meeting.SummaryState != Domain.Enums.SummaryState.Completed)
        {
            _logger.LogWarning("Summary not available for MeetingId: {MeetingId}", request.MeetingId);
            throw new InvalidOperationException("Summary not available");
        }
        if (string.IsNullOrEmpty(meeting.SummaryPath))
        {
            _logger.LogWarning("Summary path is empty for MeetingId: {MeetingId}", request.MeetingId);
            throw new InvalidOperationException("Summary path is empty");
        }
        _logger.LogInformation("Summary retrieved successfully for MeetingId: {MeetingId}", request.MeetingId);

        return new SummarizeDto() {MeetingTitle = meeting.Title, SummaryText = meeting.SummaryPath };
    }
}
