
using AutoMapper;
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Recording;

public sealed class StopRecordingHandler : IRequestHandler<StopRecordingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAudioRecordingService _audioService;
    private readonly ILogger<StopRecordingHandler> _logger;

    public StopRecordingHandler(IMeetingRepository repo, 
        IUnitOfWork unitOfWork,
        IAudioRecordingService audioService, 
        IMapper mapper, ILogger<StopRecordingHandler> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _audioService = audioService;
    }

    public async Task<Unit> Handle(StopRecordingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping recording for meeting {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        meeting.StopRecording(DateTime.UtcNow);

        // On stoppe l'enregistrement audio et on récupère le chemin du fichier
        var audioPath = await _audioService.StopAsync(meeting.Id, cancellationToken);

        // On attache le chemin du fichier à l'agrégat
        meeting.AttachAudio(audioPath);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recording stopped for meeting {MeetingId} ({Path})", meeting.Id, audioPath);


        return Unit.Value;
    }
}
