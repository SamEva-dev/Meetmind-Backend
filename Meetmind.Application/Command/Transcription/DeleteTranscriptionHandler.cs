using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Transcription
{
    public class DeleteTranscriptionHandler : IRequestHandler<DeleteTranscriptionCommand, Unit>
    {
        private readonly ITranscriptionRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTranscriptionHandler> _logger;
        private readonly INotificationService _notificationService;

        public DeleteTranscriptionHandler(
            ITranscriptionRepository repo, 
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<DeleteTranscriptionHandler> logger)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<Unit> Handle(DeleteTranscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting transcription {TranscriptionId}", request.MeetingId);
            var transcription = await _repo.GetTranscriptionByIdAsync(request.MeetingId, cancellationToken);
            if (transcription == null)
            {
                _logger.LogInformation("Transcription {TranscriptionId} not found", request.MeetingId);
                throw new KeyNotFoundException($"Transcription {request.MeetingId} not found");
            }
            await _repo.DeleteTransition(transcription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyTranscriptionDeletedAsync(cancellationToken);

            await _notificationService.NotifyMeetingAsync(new Domain.Models.Notifications
            {
                MeetingId = transcription.MeetingId,
                Title = transcription.Tilte,
                Message = $"Transcription for meeting {transcription.MeetingId} has been deleted",
                Time = DateTime.UtcNow
            }, cancellationToken);

            return Unit.Value;
        }
    }

}
