using Meetmind.Application.Dto;

namespace Meetmind.Application.Connectors;

public interface ICalendarConnector
{
    Task<List<CalendarMeetingDto>> GetTodayMeetingsAsync(CancellationToken cancellationToken);
    Task NotifyConfirmAccesAsync(object dto, CancellationToken token);
}
