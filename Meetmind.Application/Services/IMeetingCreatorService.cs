
using Meetmind.Application.Dto;

namespace Meetmind.Application.Services;

public interface IMeetingCreatorService
{
    Task<List<CalendarMeetingDto>> GetTodayMeetingsFromCalendarsAsync(DateTime utcNow, CancellationToken token);
    Task<bool> CreateMeetingIfNotExistsAsync(CalendarMeetingDto dto, CancellationToken token);
    Task NotifyMeetingCreatedAsync(CalendarMeetingDto dto);
    Task NotifyImminentMeetingsAsync(CancellationToken token);
}
