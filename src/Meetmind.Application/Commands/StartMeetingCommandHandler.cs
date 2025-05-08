using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;

namespace Meetmind.Application.Commands;

public class StartMeetingCommandHandler : IRequestHandler<StartMeetingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly INotifier _notifier;

    public StartMeetingCommandHandler(IMeetingRepository repo, IUnitOfWork uow, INotifier notifier)
    {
        _repo = repo;
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<Unit> Handle(StartMeetingCommand request, CancellationToken ct)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct);
        if (meeting is null)
            throw new KeyNotFoundException($"Meeting {request.MeetingId} not found");

        meeting.Start();
        await _uow.SaveChangesAsync(ct);
        await _notifier.BroadcastMeetingStateAsync(meeting.Id, meeting.State.ToString(), ct);
        return Unit.Value;
    }
}