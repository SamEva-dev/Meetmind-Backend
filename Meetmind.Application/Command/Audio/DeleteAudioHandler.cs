
using AutoMapper;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Audio;

public class DeleteAudioHandler : IRequestHandler<DeleteAudioCommand, Unit>
{
    private readonly IAudioFragmentRepository _audioFragmentRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAudioHandler> _logger;
    private readonly IMapper _mapper;

    public DeleteAudioHandler(IAudioFragmentRepository audioFragmentRepository, 
        IMeetingRepository meetingRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DeleteAudioHandler> logger)
    {
        _audioFragmentRepository = audioFragmentRepository;
        _meetingRepository = meetingRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(DeleteAudioCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogError("DeleteAudioCommand is null");
            throw new ArgumentNullException(nameof(request));
        }
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting == null)
        {
            _logger.LogError($"Meeting with ID {request.MeetingId} not found");
            throw new KeyNotFoundException($"Meeting with ID {request.MeetingId} not found");
        }
        if (meeting.AudioPath == null)
        {
            _logger.LogError($"Audio path for meeting ID {request.MeetingId} not found");
            throw new FileNotFoundException($"Audio path for meeting ID {request.MeetingId} not found");
        }

        var audioFragments = await _audioFragmentRepository.GetFragmentIdAsync(request.MeetingId, cancellationToken);
        if (audioFragments == null)
        {
            _logger.LogError($"Audio fragments for meeting ID {request.MeetingId} not found");
            throw new KeyNotFoundException($"Audio fragments for meeting ID {request.MeetingId} not found");
        }
        await _audioFragmentRepository.DeleteAsync(audioFragments, cancellationToken);

        if (!File.Exists(meeting.AudioPath))
        {
            _logger.LogDebug($"Fichier audio introuvable pour suppression : {meeting.AudioPath}");
            throw new FileNotFoundException($"Fichier audio introuvable pour la réunion {request.MeetingId}");
        }

        File.Delete(meeting.AudioPath);

        meeting.DetachAudio();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAudioDeletedAsync(_mapper.Map<MeetingDto>(meeting), cancellationToken);

        return Unit.Value;

    }
}
