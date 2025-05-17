
using Meetmind.Application.Dto;

namespace Meetmind.Application.Repositories;

public interface ICalendarSyncLogRepository
{
    bool ExistsExternal(string externalId, string source, out object existingId, CancellationToken cancellationToken);
    Task<List<CalendarSyncLogDto>> GetCalendarSyncLogs(CancellationToken cancellationToken);
}
