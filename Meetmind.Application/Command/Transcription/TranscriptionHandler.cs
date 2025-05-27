
using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Transcription;

public sealed class TranscriptionHandler : IRequestHandler<TranscriptionCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TranscriptionHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public TranscriptionHandler(
        IMeetingRepository repo, 
        IUnitOfWork uow,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<TranscriptionHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(TranscriptionCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");

        if (meeting.State != MeetingState.Done)
            throw new InvalidOperationException("Meeting must be completed before requesting transcription");

        //if (meeting.TranscriptState != TranscriptState.NotRequested)
        //    throw new InvalidOperationException("Transcription already requested or in progress");

        meeting.QueueTranscription();

        _logger.LogInformation("📥 Transcription queued for meeting {Id}", meeting.Id);

        await _uow.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyTranscriptionQueuedAsync(_mapper.Map<MeetingDto>(meeting), cancellationToken);

        return Unit.Value;
    }
}
