using Meetmind.Application.Dto;

namespace Meetmind.Application.Connectors;

public interface ICalendarConnector
{
    string Source { get; }
    Task<List<CalendarMeetingDto>> GetTodayMeetingsAsync(CancellationToken cancellationToken);
    Task<bool> IsCancelledAsync(string v, CancellationToken ct);
}
