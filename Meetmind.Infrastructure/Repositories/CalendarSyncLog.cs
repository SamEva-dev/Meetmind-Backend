using AutoMapper;
using AutoMapper.QueryableExtensions;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories;

public class CalendarSyncLog : ICalendarSyncLogRepository
{
    private readonly MeetMindDbContext _dbContext;
    private readonly IMapper _mapper;
    public CalendarSyncLog(MeetMindDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public bool ExistsExternal(string externalId, string source, out object existingId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<CalendarSyncLogDto>> GetCalendarSyncLogs(CancellationToken cancellationToken)
    {
        return _dbContext.CalendarSyncLogs
                .AsNoTracking()
                .OrderByDescending(l => l.TimestampUtc)
                 .Take(100)
                 .ProjectTo<CalendarSyncLogDto>(_mapper.ConfigurationProvider)
                 .ToListAsync(cancellationToken);
    }
}
