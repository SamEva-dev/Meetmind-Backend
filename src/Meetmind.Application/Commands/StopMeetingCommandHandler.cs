using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;

namespace Meetmind.Application.Commands;

public class StopMeetingCommandHandler : IRequestHandler<StopMeetingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;

    public StopMeetingCommandHandler(IMeetingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Unit> Handle(StopMeetingCommand request, CancellationToken ct)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct)
            ?? throw new KeyNotFoundException("Meeting not found");

        meeting.Stop(request.EndUtc);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}