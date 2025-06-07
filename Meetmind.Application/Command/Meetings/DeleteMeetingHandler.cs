using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Meetings
{
    public class DeleteMeetingHandler : IRequestHandler<DeleteMeetingCommand, Unit>
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IDateTimeProvider _clock;
        private readonly ILogger<DeleteMeetingHandler> _logger;
        private readonly INotificationService _meetingNotifierService;
        private readonly IUnitOfWork _uow;

        public DeleteMeetingHandler(IMeetingRepository meetingRepository, 
            IDateTimeProvider clock,
            IUnitOfWork uow,
            ILogger<DeleteMeetingHandler> logger,
            INotificationService meetingNotifierService)
        {
            _meetingRepository = meetingRepository;
            _clock = clock;
            _logger = logger;
            _meetingNotifierService = meetingNotifierService;
            _uow = uow;
        }

        public async Task<Unit> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting meeting with id: {Id}", request.MeetingId);

            var meeting = await _meetingRepository.GetMeetingById(request.MeetingId, cancellationToken);
            if (meeting == null)
            {
                _logger.LogWarning("Meeting with id {Id} not found", request.MeetingId);
                throw new KeyNotFoundException($"Meeting with id {request.MeetingId} not found");
            }

            await _meetingRepository.DeleteAsync(meeting, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _meetingNotifierService.NotifyMeetingDeletedAsync(meeting.Id, cancellationToken);

            await _meetingNotifierService.NotifyMeetingAsync(new Domain.Models.Notifications
            {
                MeetingId = meeting.Id,
                Title = meeting.Title,
                Message = $"Meeting {meeting.Id} has been deleted",
                Time = _clock.UtcNow
            }, cancellationToken);

            return Unit.Value;
        }
    }
}
