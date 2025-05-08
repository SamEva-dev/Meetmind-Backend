using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Commands;

public class TriggerTranscriptionCommandHandler : IRequestHandler<TriggerTranscriptionCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TriggerTranscriptionCommandHandler> _logger;

    public TriggerTranscriptionCommandHandler(
        IMeetingRepository repo,
        IUnitOfWork uow,
        ILogger<TriggerTranscriptionCommandHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Unit> Handle(TriggerTranscriptionCommand request, CancellationToken ct)
    {
        _logger.LogInformation("[{Time}] Transcription trigger requested for Meeting {MeetingId}.",
            DateTime.UtcNow, request.MeetingId);

        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct);
        if (meeting is null)
        {
            _logger.LogWarning("[{Time}] Meeting {MeetingId} not found.",
                DateTime.UtcNow, request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found.");
        }

        try
        {
            meeting.QueueTranscription();
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("[{Time}] Transcription successfully queued for Meeting {MeetingId}.",
                DateTime.UtcNow, meeting.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Time}] Failed to queue transcription for Meeting {MeetingId}.",
                DateTime.UtcNow, request.MeetingId);
            throw;
        }

        return Unit.Value;
    }
}