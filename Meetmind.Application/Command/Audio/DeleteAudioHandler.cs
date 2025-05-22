
using MediatR;
using Meetmind.Application.Helper;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Audio;

public class DeleteAudioHandler : IRequestHandler<DeleteAudioCommand, Unit>
{
    private readonly IAudioFragmentRepository _audioFragmentRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ITranscriptionRepository _transcriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAudioHandler> _logger;

    public DeleteAudioHandler(IAudioFragmentRepository audioFragmentRepository, 
        IMeetingRepository meetingRepository, 
        ITranscriptionRepository transcriptionRepository, 
        IUnitOfWork unitOfWork, 
        ILogger<DeleteAudioHandler> logger)
    {
        _audioFragmentRepository = audioFragmentRepository;
        _meetingRepository = meetingRepository;
        _transcriptionRepository = transcriptionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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

        var files = Directory.GetFiles(meeting.AudioPath);
        if (files.Length == 0)
        {
            _logger.LogError($"Audio not found for meeting ID {request.MeetingId}");
            throw new FileNotFoundException($"Audio not found for meeting ID {request.MeetingId}");
        }

        if (!File.Exists(meeting.AudioPath))
        {
            _logger.LogDebug($"Fichier audio introuvable pour suppression : {meeting.AudioPath}");
            throw new FileNotFoundException($"Fichier audio introuvable pour la réunion {request.MeetingId}");
        }

        File.Delete(meeting.AudioPath);

        meeting.DetachAudio();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;

    }
}
