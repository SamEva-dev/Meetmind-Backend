using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Dtos;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly MeetMindDbContext _db;
    public MeetingRepository(MeetMindDbContext db) => _db = db;

    public Task<Meeting?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Meetings.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<List<MeetingDto>> GetMeetingsTodayAsync(DateTime today, CancellationToken ct)
    {
        return _db.Meetings
            .Where(m => m.StartUtc.Date == today)
            .Select(m => new MeetingDto
            {
                Id = m.Id,
                Title = m.Title,
                StartUtc = m.StartUtc,
                EndUtc = m.EndUtc,
                State = m.State
            })
            .ToListAsync(ct);
    }
}
