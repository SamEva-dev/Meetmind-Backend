using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Events.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace Meetmind.Application.Command.Audio;

public class AddAudioFragmentHandler : IRequestHandler<AddAudioFragmentCommand, Unit>
{
    private readonly IAudioFragmentRepository _audioFragment;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AddAudioFragmentHandler> _logger;

    public AddAudioFragmentHandler(IAudioFragmentRepository audioFragment, 
        IUnitOfWork unitOfWork, 
        INotificationService notificationService, 
        ILogger<AddAudioFragmentHandler> logger)
    {
        _audioFragment = audioFragment;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddAudioFragmentCommand request, CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "audio", request.MeetingId.ToString());
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var filePath = Path.Combine(basePath, $"frag_{request.SequenceNumber}.wav");
        using (var stream = File.Create(filePath))
        {
            await request.AudioChunk.CopyToAsync(stream);
        }

        // Ajoute en DB
        var fragment = new AudioMetadata
        {
            MeetingId = request.MeetingId,
            FragmentCount = request.SequenceNumber,
            FilePath = filePath,
            UploadedUtc = DateTime.UtcNow,
        };
        await _audioFragment.AddFragment(fragment, cancellationToken);

        await _unitOfWork.SaveChangesAsync();

        // Option : notifie SignalR progression
        await _notificationService.NotifyFragmentUploaded(request.MeetingId, request.SequenceNumber);

        return Unit.Value;
    }
}
