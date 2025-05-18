
using AutoMapper;
using MediatR;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Meetmind.Application.Command.Recording;

public sealed class StartRecordingHandler : IRequestHandler<StartRecordingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<StartRecordingHandler> _logger;
    private readonly IMapper _mapper;

    public StartRecordingHandler(IMeetingRepository repo, IUnitOfWork uow, IMapper mapper, ILogger<StartRecordingHandler> logger)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Unit> Handle(StartRecordingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recording for meeting {MeetingId}", request.MeetingId);
        var meeting = await _repo.GetByIdAsync(request.MeetingId, cancellationToken);
        if (meeting is null)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", request.MeetingId);
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");
        }
        meeting.StartRecording();

        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
