using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Meetmind.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Summarize;

public class SummarizeHandler : IRequestHandler<SummarizeCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SummarizeHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public SummarizeHandler(IMeetingRepository repo, 
        IUnitOfWork uow,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<SummarizeHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
        _notificationService = notificationService;
        _mapper = mapper;
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

        _logger.LogInformation("Meeting {MeetingId} queued for summarization", request.MeetingId);

        await _notificationService.NotifySummaryQueuedAsync(_mapper.Map<MeetingDto>(meeting), cancellationToken);

        await _notificationService.NotifyMeetingAsync(new Domain.Models.Notifications
        {
            MeetingId = meeting.Id,
            Title = meeting.Title,
            Message = $"Summarization for meeting {meeting.Id} has been queued",
            Time = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}
