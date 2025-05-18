
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
    private readonly ILogger<ResumeRecordingHandler> _logger;

    public ResumeRecordingHandler(IMeetingRepository repo, IUnitOfWork uow, IMapper mapper, ILogger<ResumeRecordingHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResumeRecordingCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Resume recording for meeting {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        meeting.ResumeRecording();

        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
