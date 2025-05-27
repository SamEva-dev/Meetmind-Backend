using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Meetings;

public class CreateMeetingHandler : IRequestHandler<CreateMeetingCommand, Guid>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateMeetingHandler> _logger;

    public CreateMeetingHandler(IMeetingRepository meetingRepository,
        IUnitOfWork uow, 
        INotificationService notificationService, 
        ILogger<CreateMeetingHandler> logger)
    {
        _meetingRepository = meetingRepository;
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateMeetingCommand for {Title}", request.Title);

        var meetingId = await _meetingRepository.CreateMeetingAsync(request, cancellationToken);
        _logger.LogInformation("Created meeting with ID {MeetingId}", meetingId);
        _uow.SaveChangesAsync(cancellationToken).GetAwaiter().GetResult();

        var meeting = await _meetingRepository.GetMeetingById(meetingId, cancellationToken);
        await _notificationService.NotifyMeetingCreatedAsync(meeting, cancellationToken);

        return meetingId;
    }
}

