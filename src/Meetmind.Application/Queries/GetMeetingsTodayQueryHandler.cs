using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Dtos;

namespace Meetmind.Application.Queries;

public class GetMeetingsTodayQueryHandler : IRequestHandler<GetMeetingsTodayQuery, List<MeetingDto>>
{
    private readonly IMeetingRepository _repo;

    public GetMeetingsTodayQueryHandler(IMeetingRepository repo) => _repo = repo;

    public async Task<List<MeetingDto>> Handle(GetMeetingsTodayQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        return await _repo.GetMeetingsTodayAsync(today, ct);

        
    }
}