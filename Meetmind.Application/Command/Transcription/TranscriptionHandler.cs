
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Transcription;

public sealed class TranscriptionHandler : IRequestHandler<TranscriptionCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TranscriptionHandler> _logger;

    public TranscriptionHandler(IMeetingRepository repo, IUnitOfWork uow, ILogger<TranscriptionHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Unit> Handle(TranscriptionCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");

        if (meeting.State != MeetingState.Done)
            throw new InvalidOperationException("Meeting must be completed before requesting transcription");

        if (meeting.TranscriptState != TranscriptState.NotRequested)
            throw new InvalidOperationException("Transcription already requested or in progress");

        meeting.QueueTranscription();

        _logger.LogInformation("📥 Transcription queued for meeting {Id}", meeting.Id);

        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
