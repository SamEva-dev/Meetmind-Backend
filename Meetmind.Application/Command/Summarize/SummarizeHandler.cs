using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Summarize;

public class SummarizeHandler : IRequestHandler<SummarizeCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SummarizeHandler> _logger;

    public SummarizeHandler(IMeetingRepository repo, IUnitOfWork uow, ILogger<SummarizeHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Unit> Handle(SummarizeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SummarizeCommand for MeetingId: {MeetingId}", request.MeetingId);

        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting with ID {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        if (meeting.State != Domain.Enums.MeetingState.Done)
        {
            _logger.LogWarning("Meeting {MeetingId} is not in a completed state", request.MeetingId);
            throw new InvalidOperationException("Meeting must be completed before requesting summarization");
        }
        if (meeting.SummaryState != Domain.Enums.SummaryState.NotRequested)
        {
            _logger.LogWarning("Summarization already requested or in progress for MeetingId: {MeetingId}", request.MeetingId);
            throw new InvalidOperationException("Summarization already requested or in progress");
        }
        meeting.QueueSummary();
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
