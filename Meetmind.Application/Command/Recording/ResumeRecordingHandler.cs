
using System.Threading;
using AutoMapper;
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Recording;

public sealed class ResumeRecordingHandler : IRequestHandler<ResumeRecordingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAudioRecordingService _audioService;
    private readonly ILogger<ResumeRecordingHandler> _logger;

    public ResumeRecordingHandler(IMeetingRepository repo, 
        IUnitOfWork uow,
        IAudioRecordingService audioService, 
        IMapper mapper, ILogger<ResumeRecordingHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _audioService = audioService;
    }

    public async Task<Unit> Handle(ResumeRecordingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resume recording for meeting {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        meeting.ResumeRecording();
        await _audioService.ResumeAsync(meeting.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
