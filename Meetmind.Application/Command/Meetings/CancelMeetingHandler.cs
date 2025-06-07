

using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Meetings;

public sealed class CancelMeetingHandler : IRequestHandler<CancelMeetingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _meetingNotifierService;
    private readonly ILogger<CancelMeetingHandler> _logger;

    public CancelMeetingHandler(IMeetingRepository repo, IUnitOfWork uow, INotificationService meetingNotifierService, ILogger<CancelMeetingHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _meetingNotifierService = meetingNotifierService;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("❌ Meeting not found: {Id}", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }

        if (meeting.IsCancelled)
        {
            _logger.LogInformation("ℹ️ Meeting {Id} already cancelled", meeting.Id);
            return Unit.Value;
        }

        meeting.Cancel();
        await _uow.SaveChangesAsync(cancellationToken);

        await _meetingNotifierService.NotifyMeetingCancelledAsync(meeting.Id, cancellationToken);

        await _meetingNotifierService.NotifyMeetingAsync(new Domain.Models.Notifications
        {
            MeetingId = meeting.Id,
            Title = meeting.Title,
            Message = $"Meeting {meeting.Id} has been cancelled",
            Time = DateTime.UtcNow
        }, cancellationToken);

        _logger.LogInformation("🗑️ Meeting {Id} cancelled successfully", meeting.Id);
        return Unit.Value;
    }
}
