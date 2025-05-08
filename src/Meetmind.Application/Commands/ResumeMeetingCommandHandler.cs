using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;

namespace Meetmind.Application.Commands;

public class ResumeMeetingCommandHandler : IRequestHandler<ResumeMeetingCommand, Unit>
{
    private readonly IMeetingRepository _repo;
    private readonly IUnitOfWork _uow;

    public ResumeMeetingCommandHandler(IMeetingRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Unit> Handle(ResumeMeetingCommand request, CancellationToken ct)
    {
        var meeting = await _repo.GetByIdAsync(request.MeetingId, ct)
            ?? throw new KeyNotFoundException("Meeting not found");

        meeting.Resume();
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}