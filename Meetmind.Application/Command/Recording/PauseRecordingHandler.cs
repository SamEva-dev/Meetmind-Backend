using System.Threading;
using AutoMapper;
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Recording;

public sealed class PauseRecordingHandler : IRequestHandler<PauseRecordingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PauseRecordingHandler> _logger;
    private readonly IAudioRecordingService _audioService;
    private readonly IMapper _mapper;

    public PauseRecordingHandler(IMeetingRepository repo, 
        IUnitOfWork uow, IAudioRecordingService audioService, 
        ILogger<PauseRecordingHandler> logger, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
        _mapper = mapper;
        _audioService = audioService;
    }

    public async Task<Unit> Handle(PauseRecordingCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Pause recording for meeting {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }

        meeting.PauseRecording();

        await _audioService.PauseAsync(meeting.Id, ct);

        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
