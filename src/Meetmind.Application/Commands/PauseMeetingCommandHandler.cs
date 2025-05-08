using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;

namespace Meetmind.Application.Commands;

public class PauseMeetingCommandHandler : IRequestHandler<PauseMeetingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;

    public PauseMeetingCommandHandler(IMeetingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Unit> Handle(PauseMeetingCommand request, CancellationToken ct)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct)
            ?? throw new KeyNotFoundException("Meeting not found");

        meeting.Pause();
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}