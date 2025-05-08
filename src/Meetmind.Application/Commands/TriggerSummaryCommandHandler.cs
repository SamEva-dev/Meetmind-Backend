using MediatR;
using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Commands;

public class TriggerSummaryCommandHandler : IRequestHandler<TriggerSummaryCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TriggerSummaryCommandHandler> _logger;

    public TriggerSummaryCommandHandler(IMeetingRepository repo, IUnitOfWork uow, ILogger<TriggerSummaryCommandHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Unit> Handle(TriggerSummaryCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Trigger summary requested for Meeting {Id}", request.MeetingId);

        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {Id} not found", request.MeetingId);
            throw new KeyNotFoundException();
        }

        meeting.QueueSummary();
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}